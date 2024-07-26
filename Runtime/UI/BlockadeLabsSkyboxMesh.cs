using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BlockadeLabsSDK
{
    public enum MeshDensity
    {
        Low,
        Medium,
        High,
        Epic
    }

    [ExecuteAlways, RequireComponent(typeof(MeshRenderer)), RequireComponent(typeof(MeshFilter))]
    public class BlockadeLabsSkyboxMesh : MonoBehaviour
    {
        [SerializeField]
        private SkyboxAI _skyboxMetadata;

        public SkyboxAI SkyboxAsset
        {
            get => _skyboxMetadata;
            internal set => _skyboxMetadata = value;
        }

        [SerializeField]
        private MeshDensity _meshDensity = MeshDensity.Medium;
        public MeshDensity MeshDensity
        {
            get => _meshDensity;
            set
            {
                if (_meshDensity != value)
                {
                    _meshDensity = value;
                    UpdateMesh();
                    OnPropertyChanged?.Invoke();
                }
            }
        }

        [SerializeField, Range(3.0f, 10.0f)]
        private float _depthScale = 3.0f;
        public float DepthScale
        {
            get => _depthScale;
            set
            {
                if (_depthScale != value)
                {
                    _depthScale = value;
                    OnMaterialPropertyChanged();
                }
            }
        }

        [SerializeField]
        private Mesh _bakedMesh;
        public Mesh BakedMesh
        {
            get => _bakedMesh;
            set
            {
                if (_bakedMesh != value)
                {
                    _bakedMesh = value;
                    UpdateMesh();
                    OnMaterialPropertyChanged();
                }
            }
        }

        public event Action OnPropertyChanged;
        public event Action<bool> OnLoadingChanged;

        public bool HasDepthTexture => MeshRenderer &&
            MeshRenderer.sharedMaterial &&
            MeshRenderer.sharedMaterial.GetTexture("_DepthMap");

        private MeshRenderer _meshRenderer;
        public MeshRenderer MeshRenderer => _meshRenderer ? _meshRenderer : _meshRenderer = GetComponent<MeshRenderer>();

        private MeshFilter _meshFilter;
        private MeshFilter MeshFilter => _meshFilter ? _meshFilter : _meshFilter = GetComponent<MeshFilter>();

        private MaterialPropertyBlock _materialPropertyBlock;
        private Dictionary<int, Mesh> _meshes = new Dictionary<int, Mesh>();

        private string _loadingText;
        public string LoadingText => _loadingText;

        private void OnEnable()
        {
            UpdateMesh();
            UpdateMaterialProperties();
        }

        private void OnDisable()
        {
            if (MeshRenderer != null && _materialPropertyBlock != null)
            {
                MeshRenderer.SetPropertyBlock(null);
                _materialPropertyBlock = null;
            }
        }

        private void UpdateMesh()
        {
            if (BakedMesh != null)
            {
                MeshFilter.sharedMesh = BakedMesh;
                return;
            }

            int subdivisions = 0;

            switch (_meshDensity)
            {
                case MeshDensity.Low:
                    subdivisions = 64;
                    break;
                case MeshDensity.Medium:
                    subdivisions = 128;
                    break;
                case MeshDensity.High:
                    subdivisions = 256;
                    break;
                case MeshDensity.Epic:
                    subdivisions = 768;
                    break;
            }

            if (_meshes.TryGetValue(subdivisions, out var mesh))
            {
                MeshFilter.sharedMesh = mesh;
                return;
            }

            StartLoading("Generating mesh...", () =>
            {
                MeshFilter.sharedMesh = CreateMesh(subdivisions);
            });
        }

        private Mesh CreateMesh(int subdivisions)
        {
#if UNITY_EDITOR
            // Look for a mesh in the project
            AssetUtils.TryCreateFolder("Meshes", out var folder);
            var meshPath = $"{folder}/Tetrahedron_{subdivisions}.asset";
            var existingMesh = AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
            if (existingMesh != null)
            {
                _meshes.Add(subdivisions, existingMesh);
                return existingMesh;
            }
#endif

            var newMesh = TetrahedronMesh.GenerateMesh(subdivisions);

#if UNITY_EDITOR
            AssetDatabase.CreateAsset(newMesh, meshPath);
            AssetDatabase.SaveAssets();
#endif
            _meshes.Add(subdivisions, newMesh);
            return newMesh;
        }

        public void UpdateMaterialProperties()
        {
            if (_materialPropertyBlock == null)
            {
                _materialPropertyBlock = new MaterialPropertyBlock();
            }

            int depthMode = HasDepthTexture && BakedMesh == null ? 1 : 0;

            if (MeshRenderer.sharedMaterial.HasProperty("_DepthMode"))
            {
                // Shader graph doesn't support Integer type
                _materialPropertyBlock.SetInt("_DepthMode", depthMode);
            }

            _materialPropertyBlock.SetFloat("_DepthScale", _depthScale);
            MeshRenderer.SetPropertyBlock(_materialPropertyBlock);

#if UNITY_VISIONOS
            // Workaround: MaterialPropertyBlock doesn't work correctly in VisionOS
            // https://unity3d.atlassian.net/servicedesk/customer/portal/2/IN-81259
            if (Application.isPlaying)
            {
                if (MeshRenderer.sharedMaterial.HasProperty("_DepthMode"))
                {
                    // Shader graph doesn't support Integer type
                    MeshRenderer.material.SetInt("_DepthMode", depthMode);
                }

                MeshRenderer.material.SetFloat("_DepthScale", _depthScale);
            }
#endif
        }

        public void EditorPropertyChanged()
        {
            UpdateMesh();
            OnMaterialPropertyChanged();
        }

        private void OnMaterialPropertyChanged()
        {
            UpdateMaterialProperties();
            OnPropertyChanged?.Invoke();
        }

        public void SavePrefab()
        {
#if UNITY_EDITOR
            AssetUtils.SavePrefabNextTo(gameObject, _meshRenderer.sharedMaterial);
            OnPropertyChanged?.Invoke();
#endif
        }

        public void BakeMesh()
        {
            BakedMesh = null;
            StartLoading("Baking depth to mesh...", () =>
            {
                var depthMap = _meshRenderer.sharedMaterial.GetTexture("_DepthMap") as Texture2D;
                var depthScale = _materialPropertyBlock.GetFloat("_DepthScale");
                BakedMesh = BakeDepth.Bake(_meshFilter.sharedMesh, depthMap, depthScale);

#if UNITY_EDITOR
                AssetUtils.SaveAssetNextTo(_bakedMesh, _meshRenderer.sharedMaterial);
#endif
            });
        }

        private void StartLoading(string text, Action action)
        {
            if (Application.isPlaying)
            {
                StartCoroutine(CoStartLoading(text, action));
            }
            else
            {
                action();
            }
        }

        private IEnumerator CoStartLoading(string text, Action action)
        {
            _loadingText = text;
            OnLoadingChanged?.Invoke(true);

            yield return null;

            try
            {
                action();
            }
            finally
            {
                _loadingText = null;
                OnLoadingChanged?.Invoke(false);
            }
        }
    }
}
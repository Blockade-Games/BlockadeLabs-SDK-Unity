using UnityEngine;
using System.IO;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

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

    [RequireComponent(typeof(MeshRenderer))]
    public class BlockadeLabsSkybox : MonoBehaviour
    {
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

        [SerializeField]
        private float _depthScale = 0.0f;
        public float DepthScale
        {
            get => _depthScale;
            set
            {
                if (_depthScale != value)
                {
                    _depthScale = value;
                    UpdateDepthScale();
                    OnPropertyChanged?.Invoke();
                }
            }
        }

        public event Action OnPropertyChanged;

        public bool HasDepthTexture => _meshRenderer?.sharedMaterial?.GetTexture("_DepthMap") != null;

        private int _remixId;
        private MeshRenderer _meshRenderer;
        private MeshFilter _meshFilter;
        private Material _material;
        private MaterialPropertyBlock _materialPropertyBlock;
        private Dictionary<int, Mesh> _meshes = new Dictionary<int, Mesh>();

        public void SetSkyboxMaterial(Material material, int remixId)
        {
            _meshRenderer = GetComponent<MeshRenderer>();
            _meshRenderer.sharedMaterial = material;
        }

        private void OnEnable()
        {
            UpdateMesh();
            UpdateDepthScale();
        }

        public int? GetRemixId()
        {
            if (!TryGetComponent<Renderer>(out var renderer) || renderer.sharedMaterial == null || renderer.sharedMaterial.mainTexture == null)
            {
                return null;
            }

            if (renderer.sharedMaterial.mainTexture.name == "default_skybox_texture")
            {
                return 0;
            }

            if (renderer.sharedMaterial == _material)
            {
                return _remixId;
            }

#if UNITY_EDITOR
            // In editor, read the remix ID from the data file saved next to the texture.
            var texturePath = AssetDatabase.GetAssetPath(renderer.sharedMaterial.mainTexture);
            var resultPath = texturePath.Substring(0, texturePath.LastIndexOf('_')) + "_data.txt";
            if (File.Exists(resultPath))
            {
                return JsonConvert.DeserializeObject<GetImagineResult>(File.ReadAllText(resultPath)).request.id;
            }
#endif
            return null;
        }

        public void UpdateMesh()
        {
            _meshFilter = GetComponent<MeshFilter>();
            switch (_meshDensity)
            {
                case MeshDensity.Low:
                    _meshFilter.sharedMesh = GetOrCreateMesh(64);
                    break;
                case MeshDensity.Medium:
                    _meshFilter.sharedMesh = GetOrCreateMesh(128);
                    break;
                case MeshDensity.High:
                    _meshFilter.sharedMesh = GetOrCreateMesh(256);
                    break;
                case MeshDensity.Epic:
                    _meshFilter.sharedMesh = GetOrCreateMesh(768);
                    break;
            }
        }

        private Mesh GetOrCreateMesh(int subdivisions)
        {
            if (_meshes.TryGetValue(subdivisions, out var mesh))
            {
                return mesh;
            }

            mesh = TetrahedronMesh.GenerateMesh(subdivisions);
            _meshes.Add(subdivisions, mesh);
            return mesh;
        }

        private void OnDisable()
        {
#if UNITY_EDITOR
            foreach (var mesh in _meshes.Values)
            {
                if (mesh != null && string.IsNullOrWhiteSpace(AssetDatabase.GetAssetPath(mesh)))
                {
                    DestroyImmediate(mesh);
                }
            }

            _meshes.Clear();
#endif
        }

        public void UpdateDepthScale()
        {
            if (_materialPropertyBlock == null)
            {
                _materialPropertyBlock = new MaterialPropertyBlock();
            }

            _meshRenderer = GetComponent<MeshRenderer>();
            _materialPropertyBlock.SetFloat("_DepthScale", _depthScale);
            _meshRenderer.SetPropertyBlock(_materialPropertyBlock);
        }

        public void EditorPropertyChanged()
        {
            UpdateMesh();
            UpdateDepthScale();
            OnPropertyChanged?.Invoke();
        }
    }
}
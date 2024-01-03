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

    [ExecuteAlways, RequireComponent(typeof(MeshRenderer))]
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
            _material = material;
            _remixId = remixId;
        }

        private void OnEnable()
        {
            _meshDensity = MeshDensity.Medium;
            UpdateMesh();
            UpdateDepthScale();
            HDRPCameraFix();
        }

        public int? GetRemixId()
        {
            if (!TryGetComponent<Renderer>(out var renderer) || renderer.sharedMaterial == null || renderer.sharedMaterial.mainTexture == null)
            {
                return null;
            }

            if (renderer.sharedMaterial.mainTexture.name == "default_skybox_texture")
            {
                // Should be 0, but currently getting 404 errors.
                return null;
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
            mesh.hideFlags = HideFlags.DontSave;
            _meshes.Add(subdivisions, mesh);
            return mesh;
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

        private void HDRPCameraFix()
        {
            // If using HDRP, and this is the default scene, set the camera environment volume mask to nothing
            // So the camera isn't affected by the default volume and we can see the skybox properly.
#if UNITY_HDRP && UNITY_EDITOR
            bool isHDRP = UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset.GetType().Name == "HDRenderPipelineAsset";
            bool isDefaultScene = AssetDatabase.GUIDFromAssetPath(gameObject.scene.path).ToString() == "d9b6ab5207db7f8438e56b4c66ea03aa";
            if (isHDRP && isDefaultScene)
            {
                var components = Camera.main.GetComponents<MonoBehaviour>();
                foreach (var component in components)
                {
                    if (component.GetType().Name == "HDAdditionalCameraData")
                    {
                        var field = component.GetType().GetField("volumeLayerMask");
                        LayerMask mask = 0;
                        field.SetValue(component, mask);
                    }
                }
            }
#endif
        }
    }
}
using UnityEngine;
using System.IO;
using Newtonsoft.Json;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BlockadeLabsSDK
{
    public enum MeshDensity
    {
        Low,
        Medium,
        High
    }

    [RequireComponent(typeof(MeshRenderer))]
    public class BlockadeLabsSkybox : MonoBehaviour
    {
        [SerializeField]
        private Mesh _lowDensityMesh;

        [SerializeField]
        private Mesh _mediumDensityMesh;

        [SerializeField]
        private Mesh _highDensityMesh;

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
                    _meshFilter.sharedMesh = _lowDensityMesh;
                    break;
                case MeshDensity.Medium:
                    _meshFilter.sharedMesh = _mediumDensityMesh;
                    break;
                case MeshDensity.High:
                    _meshFilter.sharedMesh = _highDensityMesh;
                    break;
            }
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
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

    [ExecuteAlways, RequireComponent(typeof(MeshRenderer)), RequireComponent(typeof(MeshFilter))]
    public class BlockadeLabsSkyboxMesh : MonoBehaviour
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
                    _somethingChangedSinceSave = true;
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
                    _somethingChangedSinceSave = true;
                    _depthScale = value;
                    UpdateDepthScale();
                    OnPropertyChanged?.Invoke();
                }
            }
        }

        private bool _somethingChangedSinceSave = true;

        public bool CanSave => MeshRenderer && MeshFilter && MeshRenderer.sharedMaterial &&
            MeshRenderer.sharedMaterial.mainTexture && MeshFilter.sharedMesh && _somethingChangedSinceSave &&
            MeshRenderer.sharedMaterial.mainTexture.name != "default_skybox_texture";

        public event Action OnPropertyChanged;

        public bool HasDepthTexture => _meshRenderer?.sharedMaterial?.GetTexture("_DepthMap") != null;

        private MeshRenderer _meshRenderer;
        private MeshRenderer MeshRenderer => _meshRenderer ? _meshRenderer : _meshRenderer = GetComponent<MeshRenderer>();

        private MeshFilter _meshFilter;
        private MeshFilter MeshFilter => _meshFilter ? _meshFilter : _meshFilter = GetComponent<MeshFilter>();

        private GetImagineResult _metadata;
        private MaterialPropertyBlock _materialPropertyBlock;
        private Dictionary<int, Mesh> _meshes = new Dictionary<int, Mesh>();

        internal void SetMetadata(GetImagineResult metadata)
        {
            _metadata = metadata;
            _somethingChangedSinceSave = true;
        }

        private void OnEnable()
        {
            _meshDensity = MeshDensity.Medium;
            UpdateMesh();
            UpdateDepthScale();
        }

        public int? GetRemixId()
        {
            return GetMetadata()?.request.id;
        }

        internal GetImagineResult GetMetadata()
        {
            if (!TryGetComponent<Renderer>(out var renderer) || renderer.sharedMaterial == null || renderer.sharedMaterial.mainTexture == null)
            {
                return null;
            }

            if (renderer.sharedMaterial.mainTexture.name == "default_skybox_texture")
            {
                new GetImagineResult() { request = new GetImagineRequest() { id = 0 } };
            }

#if UNITY_EDITOR
            // In editor, read the remix ID from the data file saved next to the texture.
            var texturePath = AssetDatabase.GetAssetPath(renderer.sharedMaterial.mainTexture);
            var folder = texturePath.Substring(0, texturePath.LastIndexOf('/'));
            var dataFiles = Directory.GetFiles(folder, "*data.txt", SearchOption.TopDirectoryOnly);
            if (dataFiles.Length > 0)
            {
                return JsonConvert.DeserializeObject<GetImagineResult>(File.ReadAllText(dataFiles[0]));
            }
#endif
            return _metadata;
        }

        public void UpdateMesh()
        {
            switch (_meshDensity)
            {
                case MeshDensity.Low:
                    MeshFilter.sharedMesh = GetOrCreateMesh(64);
                    break;
                case MeshDensity.Medium:
                    MeshFilter.sharedMesh = GetOrCreateMesh(128);
                    break;
                case MeshDensity.High:
                    MeshFilter.sharedMesh = GetOrCreateMesh(256);
                    break;
                case MeshDensity.Epic:
                    MeshFilter.sharedMesh = GetOrCreateMesh(768);
                    break;
            }
        }

        private Mesh GetOrCreateMesh(int subdivisions)
        {
            if (_meshes.TryGetValue(subdivisions, out var mesh))
            {
                return mesh;
            }

#if UNITY_EDITOR
            // Look for a mesh in the project
            var folder = AssetUtils.GetOrCreateFolder("Meshes");
            var meshPath = $"{folder}/Tetrahedron_{subdivisions}.asset";
            mesh = AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
            if (mesh != null)
            {
                _meshes.Add(subdivisions, mesh);
                return mesh;
            }
#endif

            mesh = TetrahedronMesh.GenerateMesh(subdivisions);
#if UNITY_EDITOR
            AssetDatabase.CreateAsset(mesh, meshPath);
            AssetDatabase.SaveAssets();
#endif
            _meshes.Add(subdivisions, mesh);
            return mesh;
        }

        public void UpdateDepthScale()
        {
            if (_materialPropertyBlock == null)
            {
                _materialPropertyBlock = new MaterialPropertyBlock();
            }

            _materialPropertyBlock.SetFloat("_DepthScale", _depthScale);
            MeshRenderer.SetPropertyBlock(_materialPropertyBlock);
        }

        public void EditorPropertyChanged()
        {
            UpdateMesh();
            UpdateDepthScale();
            _somethingChangedSinceSave = true;
            OnPropertyChanged?.Invoke();
        }

        public void SavePrefab()
        {
#if UNITY_EDITOR
            if (!CanSave)
            {
                return;
            }

            AssetUtils.SavePrefabNextTo(gameObject, _meshRenderer.sharedMaterial);
            OnPropertyChanged?.Invoke();
#endif
        }
    }
}
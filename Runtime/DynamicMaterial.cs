using UnityEngine;
using UnityEngine.Rendering;

namespace BlockadeLabsSDK
{
    // Automatically selects the right material based on render pipeline and provides helpers for setting the color.
    [ExecuteAlways]
    internal class DynamicMaterial : MonoBehaviour
    {
        public Material Standard;
        public Material URP;
        public Material HDRP;

        private MeshRenderer _meshRenderer;

        void OnEnable()
        {
            UpdateMeshRenderer();
            if (_meshRenderer)
            {
                if (GraphicsSettings.renderPipelineAsset?.GetType().Name.Contains("HDRenderPipelineAsset") ?? false)
                {
                    _meshRenderer.sharedMaterial = HDRP;
                }
                else if (GraphicsSettings.renderPipelineAsset?.GetType().Name.Contains("UniversalRenderPipelineAsset") ?? false)
                {
                    _meshRenderer.sharedMaterial = URP;
                }
                else
                {
                    _meshRenderer.sharedMaterial = Standard;
                }
            }
        }

        private void UpdateMeshRenderer()
        {
            if (_meshRenderer == null)
            {
                _meshRenderer = GetComponent<MeshRenderer>();
            }
        }
    }
}
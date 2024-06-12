using Newtonsoft.Json;
using UnityEngine;

#if UNITY_HDRP
using UnityEngine.Rendering;
#endif

namespace BlockadeLabsSDK
{
    public sealed class SkyboxAI : ScriptableObject
    {
        [SerializeField]
        private int _id;

        [JsonProperty("id")]
        public int Id
        {
            get => _id;
            internal set => _id = value;
        }

        [SerializeField]
        private string _obfuscatedId;

        [JsonProperty("obfuscated_id")]
        public string ObfuscatedId
        {
            get => _obfuscatedId;
            internal set => _obfuscatedId = value;
        }

        [SerializeField]
        private int _skyboxStyleId;

        [JsonProperty("skybox_style_id")]
        public int SkyboxStyleId
        {
            get => _skyboxStyleId;
            internal set => _skyboxStyleId = value;
        }

        [SerializeField]
        private string _skyboxStyleName;

        [JsonProperty("skybox_style_name")]
        public string SkyboxStyleName
        {
            get => _skyboxStyleName;
            internal set => _skyboxStyleName = value;
        }

        [SerializeField]
        private Status _status;

        [JsonProperty("status")]
        public Status Status
        {
            get => _status;
            internal set => _status = value;
        }

        [SerializeField]
        private string _type;

        [JsonProperty("type")]
        public string Type
        {
            get => _type;
            internal set => _type = value;
        }

        [SerializeField]
        private Material _depthMaterial;

        public Material DepthMaterial
        {
            get => _depthMaterial;
            internal set => _depthMaterial = value;
        }

        [SerializeField]
        private Material _skyboxMaterial;

        public Material SkyboxMaterial
        {
            get => _skyboxMaterial;
            internal set => _skyboxMaterial = value;
        }

        [SerializeField]
        private Texture2D _depthTexture;

        public Texture2D DepthTexture
        {
            get => _depthTexture;
            internal set => _depthTexture = value;
        }

        [SerializeField]
        private Cubemap _skyboxTexture;

        public Cubemap SkyboxTexture
        {
            get => _skyboxTexture;
            internal set => _skyboxTexture = value;
        }

#if UNITY_HDRP
        [SerializeField]
        private VolumeProfile _volumeProfile;

        public VolumeProfile VolumeProfile
        {
            get => _volumeProfile;
            internal set => _volumeProfile = value;
        }
#endif // UNITY_HDRP

        [SerializeField]
        private string _prompt;
        public string Prompt
        {
            get => _prompt;
            internal set => _prompt = value;
        }

        [SerializeField]
        private string _negativeText;
        public string NegativeText
        {
            get => _negativeText;
            internal set => _negativeText = value;
        }

        [SerializeField]
        private SkyboxAiModelVersion _model;
        public SkyboxAiModelVersion Model
        {
            get => _model;
            internal set => _model = value;
        }

        internal void SetMetadata(ImagineResult result)
        {
            _id = result.id;
            _obfuscatedId = result.obfuscated_id;
            _skyboxStyleId = result.skybox_style_id;
            _skyboxStyleName = result.skybox_style_name;
            _status = result.status;
            _type = result.type;
            _prompt = result.prompt;
            _negativeText = result.negative_text;
            _model = result.model == "Model 3" ? SkyboxAiModelVersion.Model3 : SkyboxAiModelVersion.Model2;
        }

        internal ImagineResult GetMetadata()
        {
            return new ImagineResult
            {
                id = _id,
                obfuscated_id = _obfuscatedId,
                skybox_style_id = _skyboxStyleId,
                skybox_style_name = _skyboxStyleName,
                status = _status,
                type = _type,
                prompt = _prompt,
                negative_text = _negativeText,
                model = _model == SkyboxAiModelVersion.Model3 ? "Model 3" : "Model 2"
            };
        }
    }
}
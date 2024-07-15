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
        private SkyboxModel _model;
        public SkyboxModel Model
        {
            get => _model;
            internal set => _model = value;
        }

        internal void SetMetadata(SkyboxInfo skybox)
        {
            _id = skybox.Id;
            _obfuscatedId = skybox.ObfuscatedId;
            _skyboxStyleId = skybox.SkyboxStyleId;
            _skyboxStyleName = skybox.SkyboxStyleName;
            _type = skybox.Type;
            _prompt = skybox.Prompt;
            _negativeText = skybox.NegativeText;
            _model = skybox.Model;
        }

        public static implicit operator SkyboxAI(SkyboxInfo skybox)
        {
            var skyboxAI = CreateInstance<SkyboxAI>();
            skyboxAI.SetMetadata(skybox);
            return skyboxAI;
        }
    }
}
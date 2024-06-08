using Newtonsoft.Json;
using UnityEngine;

namespace BlockadeLabsSDK
{
    public sealed class SkyboxAI : ScriptableObject
    {
        [SerializeField]
        private int id;

        [JsonProperty("id")]
        public int Id
        {
            get => id;
            internal set => id = value;
        }

        [SerializeField]
        private string obfuscated_id;

        [JsonProperty("obfuscated_id")]
        public string ObfuscatedId
        {
            get => obfuscated_id;
            internal set => obfuscated_id = value;
        }

        [SerializeField]
        private int skybox_style_id;

        [JsonProperty("skybox_style_id")]
        public int SkyboxStyleId
        {
            get => skybox_style_id;
            internal set => skybox_style_id = value;
        }

        [SerializeField]
        private string skybox_style_name;

        [JsonProperty("skybox_style_name")]
        public string SkyboxStyleName
        {
            get => skybox_style_name;
            internal set => skybox_style_name = value;
        }

        [SerializeField]
        private Status status;

        [JsonProperty("status")]
        public Status Status
        {
            get => status;
            internal set => status = value;
        }

        [SerializeField]
        private string type;

        [JsonProperty("type")]
        public string Type
        {
            get => type;
            internal set => type = value;
        }

        internal void SetMetadata(ImagineResult result)
        {
            id = result.id;
            obfuscated_id = result.obfuscated_id;
            skybox_style_id = result.skybox_style_id;
            skybox_style_name = result.skybox_style_name;
            status = result.status;
            type = result.type;
        }
    }
}
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace BlockadeLabsSDK
{
    [Preserve]
    public sealed class SkyboxExportOption : BaseResponse
    {
        public const string Equirectangular_JPG = "equirectangular-jpg";
        public const string Equirectangular_PNG = "equirectangular-png";
        public const string CubeMap_Roblox_PNG = "cube-map-roblox-png";
        public const string HDRI_HDR = "hdri-hdr";
        public const string HDRI_EXR = "hdri-exr";
        public const string DepthMap_PNG = "depth-map-png";
        public const string Video_LandScape_MP4 = "video-landscape-mp4";
        public const string Video_Portrait_MP4 = "video-portrait-mp4";
        public const string Video_Square_MP4 = "video-square-mp4";
        public const string CubeMap_PNG = "cube-map-default-png";

        [Preserve]
        [JsonConstructor]
        internal SkyboxExportOption(
            [JsonProperty("id")] int id,
            [JsonProperty("name")] string name,
            [JsonProperty("key")] string key,
            [JsonProperty("isPremium")] bool isPremium)
        {
            Name = name;
            Id = id;
            Key = key;
            IsPremium = isPremium;
        }

        [Preserve]
        [JsonProperty("name")]
        public string Name { get; }

        [Preserve]
        [JsonProperty("id")]
        public int Id { get; }

        [Preserve]
        [JsonProperty("key")]
        public string Key { get; }

        [Preserve]
        [JsonProperty("isPremium")]
        public bool IsPremium { get; }

        public static implicit operator string(SkyboxExportOption option) => option?.Key;
    }
}

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace BlockadeLabsSDK
{
    [Preserve, Serializable]
    internal class CreateSkyboxRequest
    {
        public string prompt;
        public string negative_text;
        public bool enhance_prompt;
        public int seed;
        public int skybox_style_id;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? remix_imagine_id;
    }

    [Preserve, Serializable]
    internal class CreateSkyboxResult
    {
        public string id;
        public string obfuscated_id;
        public string prompt;
        public string status;
        public string error_message;
        public string pusher_channel;
        public string pusher_event;
    }

    [Preserve, Serializable]
    internal class GetImagineResult
    {
        public GetImagineRequest request;
    }

    [Preserve, Serializable]
    internal class GetImagineRequest
    {
        public int id;
        public string obfuscated_id;
        public string file_url;
        public string depth_map_url;
        public string status;
        public string prompt;
        public string error_message;
    }

    [Preserve, Serializable]
    public class SkyboxStyle
    {
        public string type;
        public int id;
        public string name;
        [JsonProperty("sort_order")]
        public int sortOrder;
        public string description;
        [JsonProperty("max-char")]
        public int maxChar;
        [JsonProperty("negative-text-max-char")]
        public int negativeTextMaxChar;
        public string image;
        public string image_jpg;
        public bool premium;
        [JsonProperty("new")]
        public bool isNew;
        public bool experimental;
        public string status;
    }

    [Preserve, Serializable]
    public class SkyboxStyleFamily : SkyboxStyle
    {
        public List<SkyboxStyle> items;
    }
}
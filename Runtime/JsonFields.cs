using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

namespace BlockadeLabsSDK
{
    [Preserve, Serializable]
    internal class CreateSkyboxRequest
    {
        public string prompt;
        public string negative_text;
        public bool? enhance_prompt;
        public int? seed;
        public int? skybox_style_id;
        public int? remix_imagine_id;
        public string control_model;
        [JsonIgnore]
        public byte[] control_image;
    }

    [Preserve, Serializable]
    internal class CreateSkyboxResult
    {
        public string id;
        public string obfuscated_id;
        public string prompt;
        public Status status;
        public string error_message;
        public string pusher_channel;
        public string pusher_event;
    }

    [Preserve, Serializable]
    internal class GetImagineResult
    {
        public ImagineResult request;
        public ImagineResult imagine;
    }

    [Preserve, Serializable]
    internal class ImagineResult
    {
        public int id;
        public int api_key_id;
        public string obfuscated_id;
        public string file_url;
        public string thumb_url;
        public string depth_map_url;
        public Status status;
        public string error_message;
        public string prompt;
        public string negative_text;
        public int seed;
        public int skybox_style_id;
        public string skybox_style_name;
        public DateTime completed_at;
        public bool isMyFavorite;
        public string model;
        public string type;
        public int? remix_imagine_id;
        public string remix_imagine_obfuscated_id;
        public int? remix_starter_id;
    }

    [Preserve]
    public enum Status
    {
        All,
        Pending,
        Dispatched,
        Processing,
        Complete,
        Abort,
        Error
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
        public string model;
        public string model_version;
    }

    [Preserve, Serializable]
    public class SkyboxStyleFamily : SkyboxStyle
    {
        public List<SkyboxStyle> items;
    }

    public enum SkyboxAiModelVersion
    {
        Model2 = 2,
        Model3 = 3
    }

    [Preserve, Serializable]
    public class SkyboxTip
    {
        public string tip;
    }

    [Preserve, Serializable]
    internal class GetHistoryResult
    {
        public List<ImagineResult> data;
        public int totalCount;
        public bool has_more;
    }

    [Preserve, Serializable]
    internal class OperationResult
    {
        public string error;
        public string success;
    }
}
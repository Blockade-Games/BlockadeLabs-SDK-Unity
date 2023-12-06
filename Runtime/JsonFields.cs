using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace BlockadeLabsSDK
{
    public class CreateSkyboxRequest
    {
        public string prompt;
        public string negative_text;
        public bool enhance_prompt;
        public int seed;
        public int skybox_style_id;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int remix_imagine_id;
    }

    [System.Serializable]
    public class CreateSkyboxResult
    {
        public string id;
        public string obfuscated_id;
        public string status;
        public string error_message;
    }

    [System.Serializable]
    public class GetImagineResult
    {
        public GetImagineRequest request;
    }

    public class GetImagineRequest
    {
        public int id;
        public string obfuscated_id;
        public string file_url;
        public string depth_map_url;
        public string status;
        public string prompt;
        public string error_message;
    }

    [System.Serializable]
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
        public bool premium;
        [JsonProperty("new")]
        public bool isNew;
        public bool experimental;
        public string status;

        // Used for GUI
        private bool _selected;
        public bool selected
        {
            get => _selected;
            set
            {
                if (_selected != value)
                {
                    _selected = value;
                    OnSelectedChanged?.Invoke();
                }
            }
        }

        public event Action OnSelectedChanged;
    }

    [System.Serializable]
    public class SkyboxStyleFamily : SkyboxStyle
    {
        public List<SkyboxStyle> items;
    }

    public class UserInput
    {
        public string key;
        public int id;
        public string name;
        public string placeholder;
        public string type;

        // Constructor to initialize user input with data from API response
        public UserInput(string key, int id, string name, string placeholder, string type)
        {
            this.key = key;
            this.id = id;
            this.name = name;
            this.placeholder = placeholder;
            this.type = type;
        }
    }
}
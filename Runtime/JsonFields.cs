using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace BlockadeLabsSDK
{
    [System.Serializable]
    public class CreateSkyboxResult
    {
        public string id { get; set; }
        public string obfuscated_id { get; set; }
    }

    [System.Serializable]
    public class GetImagineResult
    {
        public GetImagineRequest request { get; set; }
    }

    public class GetImagineRequest
    {
        public string file_url { get; set; }
        public string depth_map_url { get; set; }
        public string status { get; set; }
        public string prompt { get; set; }
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

    [System.Serializable]
    public class SkyboxStyleField
    {
        public string key;
        public string name;
        public string value;
        public string type;

        // Constructor to initialize skybox style field with data from API response
        public SkyboxStyleField(UserInput fieldData)
        {
            key = fieldData.key; // "prompt"
            name = fieldData.name; // "Prompt"
            value = fieldData.placeholder ?? "";
            type = fieldData.type ?? "text";
        }
    }
}
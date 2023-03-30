using System.Collections.Generic;

namespace BlockadeLabsSDK
{
    [System.Serializable]
    public class CreateSkyboxResult
    {
        public string id { get; set; }
    }

    [System.Serializable]
    public class CreateImagineResult
    {
        public CreateImagineRequest request { get; set; }
    }
    
    public class CreateImagineRequest
    {
        public string id { get; set; }
    }
    
    [System.Serializable]
    public class GetImagineResult
    {
        public GetImagineRequest request { get; set; }
    }
    
    public class GetImagineRequest
    {
        public string file_url { get; set; }
        public string status { get; set; }
        public string prompt { get; set; }
    }
    
    [System.Serializable]
    public class Generator
    {
        public int id { get; set; }
        public string generator { get; set; }
        public Dictionary<string, Param> @params { get; set; }
    }
    
    [System.Serializable]
    public class Param
    {
        public string name { get; set; }
        public string type { get; set; }
        public string default_value { get; set; }
        public bool required { get; set; }
    }
    
    public class GeneratorField
    {
        public string key;
        
        public string name;
        
        public string value;
        
        public bool required;
    
        // Constructor to initialize field with data from API response
        public GeneratorField(KeyValuePair<string, Param> fieldData)
        {
            key = fieldData.Key; // "prompt"
            name = fieldData.Value.name; // "Imagine text prompt"
            value = fieldData.Value.default_value ?? "";
            required = fieldData.Value.required;
        }
    }
    
    public class SkyboxStyle
    {
        public int id;
        public string name;
    }
    
    public class UserInput
    {
        public string key;
        public int id;
        public string name;
        public string placeholder;

        // Constructor to initialize user input with data from API response
        public UserInput(string key, int id, string name, string placeholder)
        {
            this.key = key; 
            this.id = id; 
            this.name = name;
            this.placeholder = placeholder;
        }
    }
    
    public class SkyboxStyleField
    {
        public string key;
        public string name;
        public string value;

        // Constructor to initialize skybox style field with data from API response
        public SkyboxStyleField(UserInput fieldData)
        {
            key = fieldData.key; // [USER_INPUT_1]
            name = fieldData.name; // "prompt"
            value = "";
        }
    }
}
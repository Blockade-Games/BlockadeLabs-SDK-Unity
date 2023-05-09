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
        public string status { get; set; }
        public string prompt { get; set; }
    }

    [System.Serializable]
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
    
    [System.Serializable]
    public class SkyboxStyleField
    {
        public string key;
        public string name;
        public string value;

        // Constructor to initialize skybox style field with data from API response
        public SkyboxStyleField(UserInput fieldData)
        {
            key = fieldData.key; // "prompt"
            name = fieldData.name; // "Prompt"
            value = fieldData.placeholder ?? "";
        }
    }
}
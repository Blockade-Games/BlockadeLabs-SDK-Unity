using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace BlockadeLabsSDK
{
    public class ApiRequests
    {
        private static readonly string ApiEndpoint = "https://backend.blockadelabs.com/api/v1/";

        public static async Task<T> GetAsync<T>(string path, string apiKey)
        {
            using var request = UnityWebRequest.Get(ApiEndpoint + path + "?api_key=" + apiKey);
            LogVerbose("Get Request: " + request.url);
            await request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.Log("Get error: " + request.error);
                return default(T);
            }

            LogVerbose("Get response: " + request.downloadHandler.text);
            return JsonConvert.DeserializeObject<T>(request.downloadHandler.text);
        }

        public static async Task<List<SkyboxStyleFamily>> GetSkyboxStylesMenuAsync(string apiKey)
        {
            return await GetAsync<List<SkyboxStyleFamily>>("skybox/menu", apiKey);
        }

        public static async Task<List<SkyboxStyle>> GetSkyboxStylesAsync(string apiKey)
        {
            return await GetAsync<List<SkyboxStyle>>("skybox/styles", apiKey);
        }

        public static async Task<CreateSkyboxResult> GenerateSkyboxAsync(CreateSkyboxRequest requestData, string apiKey)
        {
            string requestJson = JsonConvert.SerializeObject(requestData);
            using var request = new UnityWebRequest();
            request.url = ApiEndpoint + "skybox?api_key=" + apiKey;
            request.method = "POST";
            request.downloadHandler = new DownloadHandlerBuffer();
            request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(requestJson));
            request.timeout = 60;
            request.SetRequestHeader("Accept", "application/json");
            request.SetRequestHeader("Content-Type", "application/json; charset=UTF-8");

            LogVerbose("Generate Skybox Request: " + request.url + "\n" + requestJson);

            await request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Create Skybox Error: " + request.error);
                return null;
            }

            LogVerbose("Generate Skybox Response: " + request.downloadHandler.text);
            return JsonConvert.DeserializeObject<CreateSkyboxResult>(request.downloadHandler.text);
        }

        public static async Task<GetImagineResult> GetRequestStatusAsync(string imagineObfuscatedId, string apiKey)
        {
            return await GetAsync<GetImagineResult>("imagine/requests/obfuscated-id/" + imagineObfuscatedId, apiKey);
        }

        public static async Task<Texture2D> DownloadTextureAsync(string textureUrl)
        {
            LogVerbose("Start texture download: " + textureUrl);
            using var imagineImageRequest = UnityWebRequest.Get(textureUrl);
            imagineImageRequest.downloadHandler = new DownloadHandlerTexture(true);
            await imagineImageRequest.SendWebRequest();

            if (imagineImageRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Get Imagine Image Error: " + imagineImageRequest.downloadHandler.error);
                return null;
            }

            LogVerbose("Complete texture download: " + textureUrl);
            var texture = (imagineImageRequest.downloadHandler as DownloadHandlerTexture).texture;
            return texture;
        }

        [System.Diagnostics.Conditional("BLOCKADE_SDK_LOG")]
        private static void LogVerbose(string log)
        {
            Debug.Log(log);
        }
    }
}
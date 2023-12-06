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
            using (var request = UnityWebRequest.Get(ApiEndpoint + path + "?api_key=" + apiKey))
            {
                await request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.Log("Get error: " + request.error);
                    return default(T);
                }

                return JsonConvert.DeserializeObject<T>(request.downloadHandler.text);
            }
        }

        public static async Task<List<SkyboxStyleFamily>> GetSkyboxStylesMenuAsync(string apiKey)
        {
            return await GetAsync<List<SkyboxStyleFamily>>("skybox/menu", apiKey);
        }

        public static async Task<List<SkyboxStyle>> GetSkyboxStylesAsync(string apiKey)
        {
            return await GetAsync<List<SkyboxStyle>>("skybox/styles", apiKey);
        }

        public static async Task<CreateSkyboxResult> GenerateSkyboxAsync(CreateSkyboxRequest request, string apiKey)
        {
            string requestJson = JsonConvert.SerializeObject(request);
            using var createSkyboxRequest = new UnityWebRequest();
            createSkyboxRequest.url = ApiEndpoint + "skybox?api_key=" + apiKey;
            createSkyboxRequest.method = "POST";
            createSkyboxRequest.downloadHandler = new DownloadHandlerBuffer();
            createSkyboxRequest.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(requestJson));
            createSkyboxRequest.timeout = 60;
            createSkyboxRequest.SetRequestHeader("Accept", "application/json");
            createSkyboxRequest.SetRequestHeader("Content-Type", "application/json; charset=UTF-8");

            await createSkyboxRequest.SendWebRequest();

            if (createSkyboxRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Create Skybox Error: " + createSkyboxRequest.error);
                return null;
            }

            return JsonConvert.DeserializeObject<CreateSkyboxResult>(createSkyboxRequest.downloadHandler.text);
        }

        public static async Task<string> GetRequestStatusAsync(string imagineObfuscatedId, string apiKey)
        {
            using var getImagineRequest = UnityWebRequest.Get(
                ApiEndpoint + "imagine/requests/obfuscated-id/" + imagineObfuscatedId + "?api_key=" + apiKey
            );

            await getImagineRequest.SendWebRequest();

            if (getImagineRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Get Imagine Error: " + getImagineRequest.error);
                return null;
            }

            return getImagineRequest.downloadHandler.text;
        }

        public static async Task<Texture2D> DownloadTextureAsync(string textureUrl)
        {
            using var imagineImageRequest = UnityWebRequest.Get(textureUrl);
            imagineImageRequest.downloadHandler = new DownloadHandlerTexture(true);
            await imagineImageRequest.SendWebRequest();

            if (imagineImageRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Get Imagine Image Error: " + imagineImageRequest.error);
                return null;
            }

            var texture = (imagineImageRequest.downloadHandler as DownloadHandlerTexture).texture;
            texture.Compress(true);
            return texture;
        }
    }
}
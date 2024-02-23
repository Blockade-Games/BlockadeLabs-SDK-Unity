using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace BlockadeLabsSDK
{
    internal class ApiRequests
    {
        private static readonly string ApiEndpoint = "https://backend-staging.blockadelabs.com/api/v1/";

        public static async Task<T> GetAsync<T>(string path, string apiKey)
        {
            using var request = UnityWebRequest.Get(ApiEndpoint + path + "?api_key=" + apiKey);
            LogVerbose("Get Request: " + request.url);
            await request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Get error: " + request.error);
                return default(T);
            }

            LogVerbose("Get response: " + request.downloadHandler.text);
            var resp = request.downloadHandler.text;
            if (resp.StartsWith('['))
            {
                resp = resp.Substring(1, resp.Length - 2);
            }
            if (string.IsNullOrWhiteSpace(resp))
            {
                LogVerbose("Empty response");
                throw new System.Exception("Empty response");
            }

            return JsonConvert.DeserializeObject<T>(resp);
        }

        public static async Task<List<SkyboxStyleFamily>> GetSkyboxStylesMenuAsync(string apiKey)
        {
            return await GetAsync<List<SkyboxStyleFamily>>("skybox/menu", apiKey);
        }

        public static async Task<List<SkyboxStyle>> GetSkyboxStylesAsync(string apiKey)
        {
            return await GetAsync<List<SkyboxStyle>>("skybox/styles", apiKey);
        }

        private static async Task<TResponse> PostAsync<TResponse>(object requestData, string path, string apiKey) where TResponse : class
        {
            string requestJson = JsonConvert.SerializeObject(requestData);
            using var request = new UnityWebRequest();
            request.url = ApiEndpoint + path + "?api_key=" + apiKey;
            request.method = "POST";
            request.downloadHandler = new DownloadHandlerBuffer();
            request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(requestJson));
            request.timeout = 60;
            request.SetRequestHeader("Accept", "application/json");
            request.SetRequestHeader("Content-Type", "application/json; charset=UTF-8");

            LogVerbose("Post Request: " + request.url + "\n" + requestJson);

            await request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Post Error: " + request.error + " " + request.downloadHandler.text);
                return null;
            }

            LogVerbose("Post Response: " + request.downloadHandler.text);
            return JsonConvert.DeserializeObject<TResponse>(request.downloadHandler.text);
        }

        public static async Task<CreateSkyboxResult> GenerateSkyboxAsync(CreateSkyboxRequest requestData, string apiKey)
        {
            return await PostAsync<CreateSkyboxResult>(requestData, "skybox", apiKey);
        }

        public static async Task<GetImagineResult> GetRequestStatusAsync(string imagineObfuscatedId, string apiKey)
        {
            return await GetAsync<GetImagineResult>("imagine/requests/obfuscated-id/" + imagineObfuscatedId, apiKey);
        }

        public static async Task<Texture2D> DownloadTextureAsync(string textureUrl, bool readable = false)
        {
            LogVerbose("Start texture download: " + textureUrl);
            using var request = UnityWebRequest.Get(textureUrl);
            request.downloadHandler = new DownloadHandlerTexture(readable);
            await request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Download error: " + textureUrl + " " + request.error);
                return null;
            }

            LogVerbose("Complete texture download: " + textureUrl);
            var texture = (request.downloadHandler as DownloadHandlerTexture).texture;
            return texture;
        }

        public static async Task DownloadFileAsync(string url, string path)
        {
            LogVerbose("Start download: " + url + " to " + path);
            using var request = UnityWebRequest.Get(url);
            request.downloadHandler = new DownloadHandlerFile(path);
            await request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Download error: " + request.downloadHandler.error);
            }

            LogVerbose("Complete download: " + url);
        }

        public static Task<GetFeedbacksResponse> GetFeedbacksAsync(string apiKey)
        {
            return GetAsync<GetFeedbacksResponse>("feedbacks", apiKey);
        }

        public static Task PostFeedbackAsync(PostFeedbacksRequest requestData, string apiKey)
        {
            return PostAsync<PostFeedbacksResponse>(requestData, "feedbacks", apiKey);
        }

        public static Task PostFeedbackSkipAsync(PostFeedbacksSkipRequest requestData, string apiKey)
        {
            return PostAsync<PostFeedbacksResponse>(requestData, "feedbacks", apiKey);
        }

        [System.Diagnostics.Conditional("BLOCKADE_SDK_LOG")]
        private static void LogVerbose(string log)
        {
            Debug.Log(log);
        }
    }
}
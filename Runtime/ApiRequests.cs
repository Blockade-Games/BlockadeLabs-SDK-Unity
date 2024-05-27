using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace BlockadeLabsSDK
{
    internal static class ApiRequests
    {
        private static readonly string ApiEndpoint = "https://backend.blockadelabs.com/api/v1/";

        private static async Task<T> GetAsync<T>(string path, string apiKey, Dictionary<string, string> queryParams = null)
        {
            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                queryParams ??= new();
                queryParams.Add("api_key", UnityWebRequest.EscapeURL(apiKey));
            }

            var queryString = string.Empty;

            if (queryParams != null && queryParams.Count > 0)
            {
                queryString = $"?{string.Join("&", queryParams.Select(parameter => $"{UnityWebRequest.EscapeURL(parameter.Key)}={UnityWebRequest.EscapeURL(parameter.Value)}"))}";
            }

            using var request = UnityWebRequest.Get(ApiEndpoint + path + queryString);
            LogVerbose("Get Request: " + request.url);
            await request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Get error: " + request.error);
                return default;
            }

            LogVerbose("Get response: " + request.downloadHandler.text);
            return JsonConvert.DeserializeObject<T>(request.downloadHandler.text);
        }

        public static async Task<List<SkyboxStyleFamily>> GetSkyboxStylesMenuAsync(string apiKey, SkyboxAiModelVersion modelVersion)
        {
            var modelQuery = new Dictionary<string, string> { { "model_version", ((int)modelVersion).ToString() } };
            return await GetAsync<List<SkyboxStyleFamily>>("skybox/menu", apiKey, modelQuery);
        }

        public static async Task<List<SkyboxStyle>> GetSkyboxStylesAsync(string apiKey)
        {
            return await GetAsync<List<SkyboxStyle>>("skybox/styles", apiKey);
        }

        public static async Task<CreateSkyboxResult> GenerateSkyboxAsync(CreateSkyboxRequest requestData, string apiKey)
        {
            string requestJson = JsonConvert.SerializeObject(requestData);
            using var request = new UnityWebRequest();
            request.url = ApiEndpoint + "skybox?api_key=" + UnityWebRequest.EscapeURL(apiKey);
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

        public static async Task<Texture2D> DownloadTextureAsync(string textureUrl, bool readable = false, CancellationToken cancellationToken = default)
        {
            LogVerbose("Start texture download: " + textureUrl);
            using var request = UnityWebRequest.Get(textureUrl);
            request.downloadHandler = new DownloadHandlerTexture(readable);
            await request.SendWebRequest();

            if (cancellationToken.IsCancellationRequested)
            {
                throw new TaskCanceledException();
            }

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Download error: " + textureUrl + " " + request.error);
                return null;
            }

            LogVerbose("Complete texture download: " + textureUrl);
            var texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
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

        public static async Task<SkyboxTip> GetSkyboxTipAsync(string apiKey, SkyboxAiModelVersion modelVersion)
        {
            if (modelVersion == SkyboxAiModelVersion.Model3)
            {
                return await GetAsync<SkyboxTip>("skybox/get-one-tip-m3", apiKey);
            }
            else
            {
                return await GetAsync<SkyboxTip>("skybox/get-one-tip", apiKey);
            }
        }

        public static async Task<GetHistoryResult> GetSkyboxHistoryAsync(string apiKey, HistorySearchQueryParameters searchQueryParams = null)
        {
            Dictionary<string, string> searchQuery = null;

            if (searchQueryParams != null)
            {
                searchQuery = new();

                if (!string.IsNullOrWhiteSpace(searchQueryParams.StatusFilter))
                {
                    searchQuery.Add("status", searchQueryParams.StatusFilter.ToLower());
                }

                if (searchQueryParams.Limit.HasValue)
                {
                    searchQuery.Add("limit", searchQueryParams.Limit.ToString());
                }

                if (searchQueryParams.Offset.HasValue)
                {
                    searchQuery.Add("offset", searchQueryParams.Offset.ToString());
                }

                if (!string.IsNullOrWhiteSpace(searchQueryParams.Order))
                {
                    searchQuery.Add("order", searchQueryParams.Order.ToUpper());
                }

                if (searchQueryParams.ImagineId.HasValue)
                {
                    searchQuery.Add("imagine_id", searchQueryParams.ImagineId.ToString());
                }

                if (!string.IsNullOrWhiteSpace(searchQueryParams.QueryFilter))
                {
                    searchQuery.Add("query", UnityWebRequest.EscapeURL(searchQueryParams.QueryFilter));
                }

                if (!string.IsNullOrWhiteSpace(searchQueryParams.GeneratorFilter))
                {
                    searchQuery.Add("generator", UnityWebRequest.EscapeURL(searchQueryParams.GeneratorFilter));
                }

                if (searchQueryParams.FavoritesOnly.HasValue &&
                    searchQueryParams.FavoritesOnly.Value)
                {
                    searchQuery.Add("my_likes", searchQueryParams.FavoritesOnly.Value.ToString().ToLower());
                }

                if (searchQueryParams.GeneratedBy.HasValue)
                {
                    searchQuery.Add("api_key_id", searchQueryParams.GeneratedBy.Value.ToString());
                }

                //if (!string.IsNullOrWhiteSpace(searchQueryParams.SkyboxStyleId))
                //{
                //    searchQuery.Add("skybox_style_id", searchQueryParams.SkyboxStyleId);
                //}
            }

            return await GetAsync<GetHistoryResult>("imagine/myRequests", apiKey, searchQuery);
        }

        public static async Task<GetImagineResult> ToggleFavorite(int imagineId)
        {
            return await GetAsync<GetImagineResult>("toggleFavorite", null, new Dictionary<string, string> { { "id", imagineId.ToString() } });
        }

        [System.Diagnostics.Conditional("BLOCKADE_DEBUG")]
        private static void LogVerbose(string log)
        {
            Debug.Log(log);
        }
    }
}
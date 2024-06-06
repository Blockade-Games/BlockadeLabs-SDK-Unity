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

        public static string ApiKey { get; set; }

        private static async Task<T> GetAsync<T>(string path, Dictionary<string, string> queryParams = null, CancellationToken cancellationToken = default)
        {
            var queryString = string.Empty;

            if (queryParams != null && queryParams.Count > 0)
            {
                queryString = $"?{string.Join("&", queryParams.Select(parameter => $"{UnityWebRequest.EscapeURL(parameter.Key)}={UnityWebRequest.EscapeURL(parameter.Value)}"))}";
            }

            using var request = UnityWebRequest.Get(ApiEndpoint + path + queryString);
            request.SetRequestHeader("x-api-key", ApiKey);
            LogVerbose("Get Request: " + request.url);
            await request.SendWebRequest();

            if (cancellationToken.IsCancellationRequested)
            {
                return default;
            }

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Get error: " + request.error);
                return default;
            }

            LogVerbose("Get response: " + request.downloadHandler.text);
            return JsonConvert.DeserializeObject<T>(request.downloadHandler.text);
        }

        public static async Task<List<SkyboxStyleFamily>> GetSkyboxStylesMenuAsync(SkyboxAiModelVersion modelVersion)
        {
            var modelQuery = new Dictionary<string, string> { { "model_version", ((int)modelVersion).ToString() } };
            return await GetAsync<List<SkyboxStyleFamily>>("skybox/menu", modelQuery);
        }

        public static async Task<List<SkyboxStyle>> GetSkyboxStylesAsync()
        {
            return await GetAsync<List<SkyboxStyle>>("skybox/styles");
        }

        public static async Task<CreateSkyboxResult> GenerateSkyboxAsync(CreateSkyboxRequest requestData)
        {
            string requestJson = JsonConvert.SerializeObject(requestData);
            using var request = new UnityWebRequest();
            request.url = ApiEndpoint + "skybox";
            request.method = "POST";
            request.downloadHandler = new DownloadHandlerBuffer();
            request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(requestJson));
            request.timeout = 60;
            request.SetRequestHeader("x-api-key", UnityWebRequest.EscapeURL(ApiKey));
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

        public static async Task<GetImagineResult> GetRequestStatusAsync(string imagineObfuscatedId)
        {
            return await GetAsync<GetImagineResult>("imagine/requests/obfuscated-id/" + imagineObfuscatedId);
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

        public static async Task<SkyboxTip> GetSkyboxTipAsync(SkyboxAiModelVersion modelVersion)
        {
            if (modelVersion == SkyboxAiModelVersion.Model3)
            {
                return await GetAsync<SkyboxTip>("skybox/get-one-tip-m3");
            }
            else
            {
                return await GetAsync<SkyboxTip>("skybox/get-one-tip");
            }
        }

        public static async Task<GetHistoryResult> GetSkyboxHistoryAsync(HistorySearchQueryParameters searchQueryParams = null)
        {
            Dictionary<string, string> searchQuery = null;

            if (searchQueryParams != null)
            {
                searchQuery = new Dictionary<string, string>();

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

                if (searchQueryParams.SkyboxStyleId != 0)
                {
                   searchQuery.Add("skybox_style_id", searchQueryParams.SkyboxStyleId.ToString());
                }
            }

            return await GetAsync<GetHistoryResult>("imagine/myRequests", searchQuery);
        }

        public static async Task<ImagineResult> ToggleFavorite(int imagineId)
        {
#if UNITY_2022_1_OR_NEWER
            using var request = UnityWebRequest.PostWwwForm($"{ApiEndpoint}imagine/favorite/{imagineId}", null);
#else
            using var request = UnityWebRequest.Post($"{ApiEndpoint}imagine/favorite/{imagineId}", null as string);
#endif
            request.SetRequestHeader("x-api-key", ApiKey);
            using var downloadHandler = new DownloadHandlerBuffer();
            request.downloadHandler = downloadHandler;
            LogVerbose("Toggle Favorite Request: " + request.url);
            await request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Get error: " + request.error);
                return null;
            }

            LogVerbose("Toggle Favorite response: " + downloadHandler.text);
            var result = JsonConvert.DeserializeObject<GetImagineResult>(downloadHandler.text);
            return result.imagine ?? result.request;
        }

        public static async Task<bool> DeleteSkyboxAsync(int id, CancellationToken cancellationToken = default)
        {
            using var request = UnityWebRequest.Delete($"{ApiEndpoint}imagine/deleteImagine/{id}");
            request.SetRequestHeader("x-api-key", ApiKey);
            using var downloadHandler = new DownloadHandlerBuffer();
            request.downloadHandler = downloadHandler;
            LogVerbose("Delete Skybox Request: " + request.url);
            await request.SendWebRequest();

            if (cancellationToken.IsCancellationRequested)
            {
                return false;
            }

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Get error: " + request.error);
                return false;
            }

            LogVerbose("Delete Skybox Response: " + downloadHandler.text);
            var result = JsonConvert.DeserializeObject<OperationResult>(downloadHandler.text);
            const string successStatus = "Item deleted successfully";
            return result.success.Equals(successStatus);
        }

        [System.Diagnostics.Conditional("BLOCKADE_DEBUG")]
        private static void LogVerbose(string log)
        {
            Debug.Log(log);
        }
    }
}
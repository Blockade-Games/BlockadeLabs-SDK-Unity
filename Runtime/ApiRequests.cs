using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        public static async Task<CreateSkyboxResult> GenerateSkyboxAsync(CreateSkyboxRequest skyboxRequest)
        {
            var formData = new WWWForm();
            formData.AddField("prompt", skyboxRequest.prompt);

            if (!string.IsNullOrWhiteSpace(skyboxRequest.negative_text))
            {
                formData.AddField("negative_text", skyboxRequest.negative_text);
            }

            if (skyboxRequest.enhance_prompt.HasValue)
            {
                formData.AddField("enhance_prompt", skyboxRequest.enhance_prompt.ToString());
            }

            if (skyboxRequest.seed.HasValue)
            {
                formData.AddField("seed", skyboxRequest.seed.Value);
            }

            if (skyboxRequest.skybox_style_id.HasValue)
            {
                formData.AddField("skybox_style_id", skyboxRequest.skybox_style_id.Value);
            }

            if (skyboxRequest.remix_imagine_id.HasValue)
            {
                formData.AddField("remix_imagine_id", skyboxRequest.remix_imagine_id.Value);
            }

            if (skyboxRequest.control_image != null)
            {
                if (!string.IsNullOrWhiteSpace(skyboxRequest.control_model))
                {
                    formData.AddField("control_model", skyboxRequest.control_model);
                }

                formData.AddBinaryData("control_image", skyboxRequest.control_image);
            }

            using var request = UnityWebRequest.Post($"{ApiEndpoint}skybox", formData);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.timeout = 60;
            request.SetRequestHeader("x-api-key", UnityWebRequest.EscapeURL(ApiKey));
            request.SetRequestHeader("Accept", "application/json");
            var contentType = request.GetRequestHeader("Content-Type");

            if (!string.IsNullOrWhiteSpace(contentType))
            {
                contentType = contentType.Replace("\"", string.Empty);
                request.SetRequestHeader("Content-Type", contentType);
            }

            LogVerbose($"Generate Skybox Request: {request.url}\n{JsonConvert.SerializeObject(skyboxRequest, Formatting.Indented)}");

            await request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Create Skybox Error: {request.downloadHandler.text}\n{request.error}");
                return null;
            }

            LogVerbose($"Generate Skybox Response: {request.downloadHandler.text}");
            return JsonConvert.DeserializeObject<CreateSkyboxResult>(request.downloadHandler.text);
        }

        public static async Task<ImagineResult> GetRequestStatusAsync(string imagineObfuscatedId)
        {
            var result = await GetAsync<GetImagineResult>("imagine/requests/obfuscated-id/" + imagineObfuscatedId);
            return result.imagine ?? result.request;
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
            if (File.Exists(path)) { return; }

            LogVerbose($"Start download: {url} to {path}");
            using var request = UnityWebRequest.Get(url);
            request.downloadHandler = new DownloadHandlerFile(path);
            await request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Download error: {request.downloadHandler.error}");
            }

            LogVerbose($"Complete download: {url}");
        }

        public static async Task<SkyboxTip> GetSkyboxTipAsync(SkyboxAiModelVersion model)
        {
            return await GetAsync<SkyboxTip>("skybox/get-one-tip-unity");
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
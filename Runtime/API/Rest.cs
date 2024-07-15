using BlockadeLabsSDK.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace BlockadeLabsSDK
{
    internal static class Rest
    {
        private const string FileUriPrefix = "file://";
        private const string content_type = "Content-Type";
        private const string content_length = "Content-Length";
        private const string application_json = "application/json";
        private const string multipart_form_data = "multipart/form-data";
        private const string application_octet_stream = "application/octet-stream";

        private static readonly UnityMainThread UnityMainThread = new UnityMainThread();

        public static async Task<RestResponse> GetAsync(string url, IReadOnlyDictionary<string, string> headers, CancellationToken cancellationToken)
        {
            await UnityMainThread;
            using var webRequest = UnityWebRequest.Get(url);
            return await webRequest.SendAsync(headers, cancellationToken);
        }

        public static async Task<RestResponse> PostAsync(string url, WWWForm formData, IReadOnlyDictionary<string, string> headers, CancellationToken cancellationToken)
        {
            await UnityMainThread;
            using var webRequest = UnityWebRequest.Post(url, formData);
            return await webRequest.SendAsync(headers, cancellationToken);
        }

        public static async Task<RestResponse> PostAsync(string url, string jsonPayload, IReadOnlyDictionary<string, string> headers, CancellationToken cancellationToken)
        {
            await UnityMainThread;
            using var webRequest = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST);
            var data = new UTF8Encoding().GetBytes(jsonPayload);
            using var uploadHandler = new UploadHandlerRaw(data);
            webRequest.uploadHandler = uploadHandler;
            using var downloadHandler = new DownloadHandlerBuffer();
            webRequest.downloadHandler = downloadHandler;
            webRequest.SetRequestHeader(content_type, application_json);
            return await webRequest.SendAsync(headers, cancellationToken);
        }

        public static async Task<RestResponse> DeleteAsync(string url, IReadOnlyDictionary<string, string> headers, CancellationToken cancellationToken)
        {
            await UnityMainThread;
            using var webRequest = UnityWebRequest.Delete(url);
            using var downloadHandler = new DownloadHandlerBuffer();
            webRequest.downloadHandler = downloadHandler;
            return await webRequest.SendAsync(headers, cancellationToken);
        }

        public static async Task<Texture2D> DownloadTextureAsync(string url, string fileName = null, bool debug = false, CancellationToken cancellationToken = default)
        {
            await UnityMainThread;
            if (string.IsNullOrWhiteSpace(fileName))
            {
                TryGetFileNameFromUrl(url, out fileName);
            }

            bool isCached;
            string cachePath;

            if (url.Contains(FileUriPrefix))
            {
                isCached = true;
                cachePath = url;
            }
            else
            {
                isCached = TryGetDownloadCacheItem(fileName, out cachePath);
            }

            if (isCached)
            {
                url = cachePath;
            }

            using var webRequest = UnityWebRequestTexture.GetTexture(url);
            var response = await webRequest.SendAsync(null, cancellationToken);
            response.Validate(debug);

            if (!isCached && Application.platform != RuntimePlatform.WebGLPlayer)
            {
                await WriteCacheItemAsync(webRequest.downloadHandler.data, cachePath, cancellationToken).ConfigureAwait(true);
            }

            var texture = ((DownloadHandlerTexture)webRequest.downloadHandler).texture;

            if (texture == null)
            {
                throw new RestException(response, $"Failed to load texture from \"{url}\"!");
            }

            texture.name = Path.GetFileNameWithoutExtension(cachePath);
            return texture;
        }

        public static async Task<string> DownloadFileAsync(string url, string fileName = null, string destination = null, bool debug = false, CancellationToken cancellationToken = default)
        {
            await UnityMainThread;

            if (string.IsNullOrWhiteSpace(fileName))
            {
                TryGetFileNameFromUrl(url, out fileName);
            }

            if (string.IsNullOrWhiteSpace(destination) &&
                TryGetDownloadCacheItem(fileName, out var filePath))
            {
                return filePath;
            }

            if (File.Exists(destination))
            {
                return destination;
            }

            filePath = destination;

            using var webRequest = UnityWebRequest.Get(url);
            using var fileDownloadHandler = new DownloadHandlerFile(filePath);
            fileDownloadHandler.removeFileOnAbort = true;
            webRequest.downloadHandler = fileDownloadHandler;
            var response = await webRequest.SendAsync(null, cancellationToken);
            response.Validate(debug);
            return filePath;
        }

        private static async Task<RestResponse> SendAsync(this UnityWebRequest webRequest, IReadOnlyDictionary<string, string> headers = null, CancellationToken cancellationToken = default)
        {
            await UnityMainThread;

            if (headers != null)
            {
                foreach (var header in headers)
                {
                    webRequest.SetRequestHeader(header.Key, header.Value);
                }
            }

            var hasUpload = webRequest.method == UnityWebRequest.kHttpVerbPOST || webRequest.method == UnityWebRequest.kHttpVerbPUT;

            // HACK: Workaround for extra quotes around boundary.
            if (hasUpload)
            {
                var contentType = webRequest.GetRequestHeader(content_type);

                if (!string.IsNullOrWhiteSpace(contentType))
                {
                    contentType = contentType.Replace("\"", string.Empty);
                    webRequest.SetRequestHeader(content_type, contentType);
                }
            }

            var requestBody = string.Empty;

            if (hasUpload && webRequest.uploadHandler != null)
            {
                var contentType = webRequest.GetRequestHeader(content_type);

                if (webRequest.uploadHandler.data != null &&
                    webRequest.uploadHandler.data.Length > 0)
                {
                    var encodedData = Encoding.UTF8.GetString(webRequest.uploadHandler.data);

                    if (contentType.Contains(multipart_form_data))
                    {
                        var boundary = contentType.Split(';')[1].Split('=')[1];
                        var formData = encodedData.Split(new[] { $"\r\n--{boundary}\r\n", $"\r\n--{boundary}--\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                        var formParts = new Dictionary<string, string>();

                        foreach (var form in formData)
                        {
                            const string eol = "\r\n\r\n";
                            var formFields = form.Split(new[] { eol }, StringSplitOptions.RemoveEmptyEntries);
                            var fieldHeader = formFields[0];
                            const string fieldName = "name=\"";
                            var key = fieldHeader.Split(new[] { fieldName }, StringSplitOptions.RemoveEmptyEntries)[1].Split('"')[0];

                            if (fieldHeader.Contains(application_octet_stream))
                            {
                                const string filename = "filename=\"";
                                var fileName = fieldHeader.Split(new[] { filename }, StringSplitOptions.RemoveEmptyEntries)[1].Split('"')[0];
                                formParts.Add(key, fileName);
                            }
                            else
                            {
                                var value = formFields[1];
                                formParts.Add(key, value);
                            }
                        }

                        requestBody = JsonConvert.SerializeObject(new { contentType, formParts });
                    }
                    else
                    {
                        requestBody = encodedData;
                    }
                }
            }


            async void CallbackThread()
            {
                try
                {
                    await UnityMainThread;

                    while (!webRequest.isDone)
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            webRequest.Abort();
                        }

                        await UnityMainThread;
                    }
                }
                catch (Exception)
                {
                    // Throw away
                }
            }

#pragma warning disable CS4014 // We purposefully don't await this task, so it will run on a background thread.
            Task.Run(CallbackThread, cancellationToken);
#pragma warning restore CS4014

            try
            {
                await webRequest.SendWebRequest();
            }
            catch (Exception e)
            {
                return new RestResponse(webRequest.url, webRequest.method, requestBody, false, $"{nameof(Rest)}.{nameof(SendAsync)}::{nameof(UnityWebRequest.SendWebRequest)} Failed!", null, -1, null, e.ToString());
            }

            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError && webRequest.responseCode >= 400)
            {
                return new RestResponse(webRequest, requestBody, false);
            }

            return new RestResponse(webRequest, requestBody, true);
        }

        #region Download Cache

        private const string blockadelabs_download_cache = nameof(blockadelabs_download_cache);

        public static string DownloadCacheDirectory
            => Path.Combine(Application.temporaryCachePath, blockadelabs_download_cache);

        public static bool TryGetDownloadCacheItem(string uri, out string filePath)
        {
#if UNITY_WEBGL
            filePath = string.Empty;
            return false;
#endif

            if (!Directory.Exists(DownloadCacheDirectory))
            {
                Directory.CreateDirectory(DownloadCacheDirectory);
            }

            bool exists;

            if (uri.Contains(FileUriPrefix))
            {
                filePath = uri;
                return File.Exists(uri.Replace(FileUriPrefix, string.Empty));
            }

            if (TryGetFileNameFromUrl(uri, out var fileName))
            {
                filePath = Path.Combine(DownloadCacheDirectory, fileName);
                exists = File.Exists(filePath);
            }
            else
            {
                filePath = Path.Combine(DownloadCacheDirectory, StringExtensions.GenerateGuid(uri).ToString());
                exists = File.Exists(filePath);
            }

            if (exists)
            {
                filePath = $"{FileUriPrefix}{Path.GetFullPath(filePath)}";
            }

            return exists;
        }

        public static bool TryGetFileNameFromUrl(string url, out string fileName)
        {
            var baseUrl = UnityWebRequest.UnEscapeURL(url);
            var rootUrl = baseUrl.Split('?')[0];
            var index = rootUrl.LastIndexOf('/') + 1;
            fileName = rootUrl.Substring(index, rootUrl.Length - index);
            return Path.HasExtension(fileName);
        }

        public static async Task WriteCacheItemAsync(byte[] data, string cachePath, CancellationToken cancellationToken)
        {
#if UNITY_WEBGL
            // no op
            await Task.CompletedTask;
#else
            if (File.Exists(cachePath)) { return; }

            try
            {
#if UNITY_2021_1_OR_NEWER
                await File.WriteAllBytesAsync(cachePath, data, cancellationToken).ConfigureAwait(true);
#else
                await Task.CompletedTask;
                File.WriteAllBytes(cachePath, data);
#endif
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to write asset to disk! {e}");
            }
#endif
        }

        #endregion Download Cache
    }
}
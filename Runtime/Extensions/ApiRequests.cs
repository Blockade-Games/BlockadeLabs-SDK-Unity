using BlockadeLabsSDK.Extensions;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace BlockadeLabsSDK
{
    internal static class ApiRequests
    {
        public static async Task DownloadFileAsync(string url, string path)
        {
            if (File.Exists(path)) { return; }

            LogVerbose($"Start download: {url} to {path}");
            using var request = UnityWebRequest.Get(url);
            request.downloadHandler = new DownloadHandlerFile(path) { removeFileOnAbort = true };
            await request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Download error: {request.downloadHandler.error}");
            }

            LogVerbose($"Complete download: {url}");
        }

        [System.Diagnostics.Conditional("BLOCKADE_DEBUG")]
        private static void LogVerbose(string log)
        {
            Debug.Log(log);
        }
    }
}
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace BlockadeLabsSDK
{
    public class RestResponse
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="request">The request that prompted the response.</param>
        /// <param name="method">The request method that prompted the response.</param>
        /// <param name="requestBody">The request body that prompted the response.</param>
        /// <param name="successful">Was the REST call successful?</param>
        /// <param name="body">Response body from the resource.</param>
        /// <param name="data">Response data from the resource.</param>
        /// <param name="responseCode">Response code from the resource.</param>
        /// <param name="headers">Response headers from the resource.</param>
        /// <param name="error">Optional, error message from the resource.</param>
        public RestResponse(string request, string method, string requestBody, bool successful, string body, byte[] data, long responseCode, IReadOnlyDictionary<string, string> headers, string error = null)
        {
            Request = request;
            RequestBody = requestBody;
            Method = method;
            Successful = successful;
            Body = body;
            Data = data;
            Code = responseCode;
            Headers = headers;
            Error = error;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="webRequest">The request that prompted the response.</param>
        /// <param name="requestBody">The request body that prompted the response.</param>
        /// <param name="successful">Was the request successful?</param>
        /// <param name="responseBody">Optional, response body override.</param>
        public RestResponse(UnityWebRequest webRequest, string requestBody, bool successful, string responseBody = null)
        {
            Request = webRequest.url;
            RequestBody = requestBody;
            Method = webRequest.method;
            Successful = successful;

            if (string.IsNullOrWhiteSpace(responseBody))
            {
                switch (webRequest.downloadHandler)
                {
                    case DownloadHandlerFile _:
                    case DownloadHandlerTexture _:
                    case DownloadHandlerAudioClip _:
                    case DownloadHandlerAssetBundle _:
                        Body = null;
                        Data = null;
                        break;
                    case DownloadHandlerBuffer downloadHandlerBuffer:
                        Body = downloadHandlerBuffer.text;
                        Data = downloadHandlerBuffer.data;
                        break;
                    case DownloadHandlerScript downloadHandlerScript:
                        Body = downloadHandlerScript.text;
                        Data = downloadHandlerScript.data;
                        break;
                    default:
                        Body = webRequest.responseCode == 401 ? "Invalid Credentials" : webRequest.downloadHandler?.text;
                        Data = webRequest.downloadHandler?.data;
                        break;
                }
            }
            else
            {
                Body = responseBody;
            }

            Code = webRequest.responseCode;
            Headers = webRequest.GetResponseHeaders() ?? invalidHeaders;

            if (!successful)
            {
                Error = $"{webRequest.error}\n{webRequest.downloadHandler?.error}";
            }
        }

        private static readonly Dictionary<string, string> invalidHeaders = new Dictionary<string, string> { { "Invalid Headers", "Invalid Headers" } };

        /// <summary>
        /// The original request that prompted the response.
        /// </summary>
        public string Request { get; }

        /// <summary>
        /// The original request body.
        /// </summary>
        public string RequestBody { get; }

        /// <summary>
        /// The request method that prompted the response.
        /// </summary>
        public string Method { get; }

        /// <summary>
        /// Was the REST call successful?
        /// </summary>
        public bool Successful { get; }

        /// <summary>
        /// Response body from the resource.
        /// </summary>
        public string Body { get; }

        /// <summary>
        /// Response data from the resource.
        /// </summary>
        public byte[] Data { get; }

        /// <summary>
        /// Response code from the resource.
        /// </summary>
        public long Code { get; }

        /// <summary>
        /// Response headers from the resource.
        /// </summary>
        public IReadOnlyDictionary<string, string> Headers { get; }

        /// <summary>
        /// Error string
        /// </summary>
        public string Error { get; }

        public override string ToString() => ToString(string.Empty);

        public string ToString(string methodName)
        {
            var debugMessage = new StringBuilder();

            if (!string.IsNullOrWhiteSpace(methodName))
            {
                debugMessage.Append($"{methodName} -> ");
            }

            debugMessage.Append($"<b>[{Method}:{(int)Code}]</b> <color=\"#{ColorUtility.ToHtmlStringRGB(Color.cyan)}\">{Request}</color>");
            debugMessage.Append(!Successful ? " <color=\"red\">Failed!</color>" : " <color=\"green\">Success!</color>");
            debugMessage.Append("\n");

            var debugMessageObject = new Dictionary<string, Dictionary<string, object>>
            {
                ["request"] = new Dictionary<string, object>
                {
                    ["url"] = Request
                }
            };

            if (!string.IsNullOrWhiteSpace(RequestBody))
            {
                try
                {
                    debugMessageObject["request"]["body"] = JToken.Parse(RequestBody);
                }
                catch
                {
                    debugMessageObject["request"]["body"] = RequestBody;
                }
            }

            debugMessageObject["response"] = new Dictionary<string, object>
            {
                ["code"] = Code
            };

            if (Headers != null)
            {
                debugMessageObject["response"]["headers"] = Headers;
            }

            if (Data != null && Data.Length > 0)
            {
                debugMessageObject["response"]["data"] = Data.Length;
            }

            if (!string.IsNullOrWhiteSpace(Body))
            {
                try
                {
                    debugMessageObject["response"]["body"] = JToken.Parse(Body);
                }
                catch
                {
                    debugMessageObject["response"]["body"] = Body;
                }
            }

            if (!string.IsNullOrWhiteSpace(Error))
            {
                debugMessageObject["response"]["error"] = Error;
            }

            var jsonSettings = new JsonSerializerSettings
            {
                StringEscapeHandling = StringEscapeHandling.EscapeNonAscii,
                Formatting = Formatting.Indented
            };
            debugMessage.Append(JsonConvert.SerializeObject(debugMessageObject, jsonSettings));
            return debugMessage.ToString();
        }
    }
}
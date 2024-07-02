using System.Runtime.CompilerServices;
using UnityEngine;

namespace BlockadeLabsSDK
{
    internal static class ResponseExtensions
    {
        private const string RateLimit = "X-RateLimit-Limit";
        private const string RateLimitRemaining = "X-RateLimit-Remaining";

        internal static void SetResponseData(this BaseResponse response, Response restResponse, BlockadeLabsClient client)
        {
            if (response is IListResponse<BaseResponse> listResponse)
            {
                foreach (var item in listResponse.Items)
                {
                    SetResponseData(item, restResponse, client);
                }
            }

            response.Client = client;

            if (restResponse.Headers == null || restResponse.Headers.Count == 0) { return; }

            if (restResponse.Headers.TryGetValue(RateLimit, out var rateLimit) &&
            int.TryParse(rateLimit, out var rateLimitValue))
            {
                response.RateLimit = rateLimitValue;
            }

            if (restResponse.Headers.TryGetValue(RateLimitRemaining, out var rateLimitRemaining) &&
                int.TryParse(rateLimitRemaining, out var rateLimitRemainingValue))
            {
                response.RateLimitRemaining = rateLimitRemainingValue;
            }
        }

        /// <summary>
        /// Validates the <see cref="Response"/> and will throw a <see cref="RestException"/> if the response is unsuccessful.
        /// </summary>
        /// <param name="response"><see cref="Response"/>.</param>
        /// <param name="debug">Print debug information of <see cref="Response"/>.</param>
        /// <param name="methodName">Optional, <see cref="CallerMemberNameAttribute"/>.</param>
        /// <exception cref="RestException"></exception>
        internal static void Validate(this Response response, bool debug = false, [CallerMemberName] string methodName = null)
        {
            if (!response.Successful)
            {
                throw new RestException(response);
            }

            if (debug)
            {
                Debug.Log(response.ToString(methodName));
            }
        }
    }
}

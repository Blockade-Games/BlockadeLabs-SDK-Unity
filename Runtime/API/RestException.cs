using System;

namespace BlockadeLabsSDK
{
    public sealed class RestException : Exception
    {
        public RestException(RestResponse response, string message = null, Exception innerException = null)
            : base(string.IsNullOrWhiteSpace(message) ? response.ToString() : message, innerException)
        {
            RestResponse = response;
        }

        public RestResponse RestResponse { get; }
    }
}
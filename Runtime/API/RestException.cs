using System;

namespace BlockadeLabsSDK
{
    public sealed class RestException : Exception
    {
        public RestException(Response response, string message = null, Exception innerException = null)
            : base(string.IsNullOrWhiteSpace(message) ? response.ToString() : message, innerException)
        {
            Response = response;
        }

        public Response Response { get; }
    }
}
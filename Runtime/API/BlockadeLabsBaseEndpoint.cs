using System.Collections.Generic;
using System.Linq;
using UnityEngine.Networking;

namespace BlockadeLabsSDK
{
    public abstract class BlockadeLabsBaseEndpoint
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="client"></param>
        protected BlockadeLabsBaseEndpoint(BlockadeLabsClient client) => this.client = client;

        /// <summary>
        /// <see cref="BlockadeLabsClient"/> for this endpoint.
        /// </summary>
        /// ReSharper disable once InconsistentNaming
        protected readonly BlockadeLabsClient client;

        /// <summary>
        /// The root endpoint address.
        /// </summary>
        protected abstract string Root { get; }

        protected string GetUrl(string endpoint = "", Dictionary<string, string> queryParameters = null)
        {
            var result = string.Format(client.Settings.BaseRequestUrlFormat, $"{Root}{endpoint}");

            if (queryParameters != null && queryParameters.Count != 0)
            {
                result += $"?{string.Join("&", queryParameters.Select(parameter => $"{UnityWebRequest.EscapeURL(parameter.Key)}={UnityWebRequest.EscapeURL(parameter.Value)}"))}";
            }

            return result;
        }

        private bool enableDebug;

        /// <summary>
        /// Enables or disables the logging of all http responses of header and body information for this endpoint.<br/>
        /// WARNING! Enabling this in your production build, could potentially leak sensitive information!
        /// </summary>
        public bool EnableDebug
        {
            get => enableDebug || client.EnableDebug;
            set => enableDebug = value;
        }
    }
}

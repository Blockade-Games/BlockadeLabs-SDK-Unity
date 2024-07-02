using BlockadeLabsSDK.Skyboxes;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Security.Authentication;

namespace BlockadeLabsSDK
{
    public sealed class BlockadeLabsClient
    {
        /// <inheritdoc />
        public BlockadeLabsClient(BlockadeLabsConfiguration configuration)
            : this(
                configuration != null ? new BlockadeLabsAuthentication(configuration) : BlockadeLabsAuthentication.Default,
                configuration != null ? new BlockadeLabsSettings(configuration) : BlockadeLabsSettings.Default)
        {
        }

        /// <summary>
        /// Creates a new client for the BlockadeLabs API, handling auth and allowing for access to various API endpoints.
        /// </summary>
        /// <param name="authentication">The API authentication information to use for API calls,
        /// or <see langword="null"/> to attempt to use the <see cref="BlockadeLabsAuthentication.Default"/>,
        /// potentially loading from environment vars or from a config file.</param>
        /// <param name="settings">Optional, <see cref="BlockadeLabsSettings"/> for specifying a proxy domain.</param>
        /// <exception cref="AuthenticationException">Raised when authentication details are missing or invalid.</exception>
        public BlockadeLabsClient(BlockadeLabsAuthentication authentication = null, BlockadeLabsSettings settings = null)
        {
            Authentication = authentication ?? BlockadeLabsAuthentication.Default;

            if (Authentication is null)
            {
                throw new AuthenticationException($"Missing {nameof(Authentication)} for {GetType().Name}");
            }

            Settings = settings ?? BlockadeLabsSettings.Default;

            if (Settings is null)
            {
                throw new ArgumentNullException($"Missing {nameof(Settings)} for {GetType().Name}");
            }

            ValidateAuthentication();
            SetupDefaultRequestHeaders();

            SkyboxEndpoint = new SkyboxEndpoint(this);
        }

        private void ValidateAuthentication()
        {
            if (Authentication?.Info == null)
            {
                throw new InvalidCredentialException($"Invalid {nameof(BlockadeLabsAuthentication)}");
            }

            if (!HasValidAuthentication)
            {
                throw new AuthenticationException("You must provide API authentication.  Please refer to https://github.com/RageAgainstThePixel/com.rest.blockadelabs#authentication for details.");
            }
        }

        public bool HasValidAuthentication => !string.IsNullOrWhiteSpace(Authentication?.Info?.ApiKey);

        private void SetupDefaultRequestHeaders()
        {
            DefaultRequestHeaders = new Dictionary<string, string>
            {
#if !UNITY_WEBGL
                { "User-Agent", "com.rest.blockadelabs" },
#endif
                { "x-api-key", Authentication.Info.ApiKey }
            };
        }

        internal static JsonSerializerSettings JsonSerializationOptions { get; } = new JsonSerializerSettings()
        {
            DefaultValueHandling = DefaultValueHandling.Ignore,
            Converters = new List<JsonConverter>
            {
                new StringEnumConverter()
            }
        };

        public BlockadeLabsAuthentication Authentication { get; }

        public BlockadeLabsSettings Settings { get; }

        public IReadOnlyDictionary<string, string> DefaultRequestHeaders { get; private set; }

        public bool EnableDebug { get; set; }

        public SkyboxEndpoint SkyboxEndpoint { get; }
    }
}

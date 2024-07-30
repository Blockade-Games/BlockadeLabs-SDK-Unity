using System.Linq;
using UnityEngine;

namespace BlockadeLabsSDK
{
    public sealed class BlockadeLabsClientSettings
    {
        /// <summary>
        /// Creates a new instance of <see cref="BlockadeLabsClientSettings"/> with default <see cref="BlockadeLabsClientSettingsInfo"/>.
        /// </summary>
        public BlockadeLabsClientSettings()
        {
            Info = new BlockadeLabsClientSettingsInfo();
            cachedDefault = this;
        }

        /// <summary>
        /// Creates a new instance of <see cref="BlockadeLabsClientSettings"/> with provided <see cref="configuration"/>.
        /// </summary>
        /// <param name="configuration"><see cref="BlockadeLabsConfiguration"/>.</param>
        public BlockadeLabsClientSettings(BlockadeLabsConfiguration configuration)
        {
            if (configuration == null)
            {
                Debug.LogWarning($"This can be speed up by directly passing a {nameof(BlockadeLabsConfiguration)} to the {nameof(BlockadeLabsClientSettings)}.ctr");
                configuration = Resources.LoadAll<BlockadeLabsConfiguration>(string.Empty).FirstOrDefault(asset => asset != null);
            }

            if (configuration != null)
            {
                Info = new BlockadeLabsClientSettingsInfo(configuration.ProxyDomainUrl);
                cachedDefault = this;
            }
            else
            {
                Info = new BlockadeLabsClientSettingsInfo();
                cachedDefault = this;
            }
        }

        /// <summary>
        /// Creates a new instance of <see cref="BlockadeLabsClientSettings"/> with the provided <see cref="clientSettingsInfo"/>.
        /// </summary>
        /// <param name="clientSettingsInfo"><see cref="BlockadeLabsClientSettingsInfo"/>.</param>
        public BlockadeLabsClientSettings(BlockadeLabsClientSettingsInfo clientSettingsInfo)
        {
            Info = clientSettingsInfo;
            cachedDefault = this;
        }

        /// <summary>
        /// Creates a new instance of <see cref="BlockadeLabsClientSettings"/>.
        /// </summary>
        /// <param name="domain">Base api domain.</param>
        public BlockadeLabsClientSettings(string domain)
        {
            Info = new BlockadeLabsClientSettingsInfo(domain);
            cachedDefault = this;
        }

        private static BlockadeLabsClientSettings cachedDefault;

        public static BlockadeLabsClientSettings Default
        {
            get => cachedDefault ??= new BlockadeLabsClientSettings(configuration: null);
            internal set => cachedDefault = value;
        }

        public BlockadeLabsClientSettingsInfo Info { get; }

        public string BaseRequestUrlFormat => Info.BaseRequestUrlFormat;
    }
}

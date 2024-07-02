// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Linq;
using UnityEngine;

namespace BlockadeLabsSDK
{
    public sealed class BlockadeLabsSettings
    {
        /// <summary>
        /// Creates a new instance of <see cref="BlockadeLabsSettings"/> with default <see cref="BlockadeLabsSettingsInfo"/>.
        /// </summary>
        public BlockadeLabsSettings()
        {
            Info = new BlockadeLabsSettingsInfo();
            cachedDefault = this;
        }

        /// <summary>
        /// Creates a new instance of <see cref="BlockadeLabsSettings"/> with provided <see cref="configuration"/>.
        /// </summary>
        /// <param name="configuration"><see cref="BlockadeLabsConfiguration"/>.</param>
        public BlockadeLabsSettings(BlockadeLabsConfiguration configuration)
        {
            if (configuration == null)
            {
                Debug.LogWarning($"This can be speed up by directly passing a {nameof(BlockadeLabsConfiguration)} to the {nameof(BlockadeLabsSettings)}.ctr");
                configuration = Resources.LoadAll<BlockadeLabsConfiguration>(string.Empty).FirstOrDefault(asset => asset != null);
            }

            if (configuration != null)
            {
                Info = new BlockadeLabsSettingsInfo(configuration.ProxyDomain);
                cachedDefault = this;
            }
            else
            {
                Info = new BlockadeLabsSettingsInfo();
                cachedDefault = this;
            }
        }

        /// <summary>
        /// Creates a new instance of <see cref="BlockadeLabsSettings"/> with the provided <see cref="settingsInfo"/>.
        /// </summary>
        /// <param name="settingsInfo"><see cref="BlockadeLabsSettingsInfo"/>.</param>
        public BlockadeLabsSettings(BlockadeLabsSettingsInfo settingsInfo)
        {
            Info = settingsInfo;
            cachedDefault = this;
        }

        /// <summary>
        /// Creates a new instance of <see cref="BlockadeLabsSettings"/>.
        /// </summary>
        /// <param name="domain">Base api domain.</param>
        public BlockadeLabsSettings(string domain)
        {
            Info = new BlockadeLabsSettingsInfo(domain);
            cachedDefault = this;
        }

        private static BlockadeLabsSettings cachedDefault;

        public static BlockadeLabsSettings Default
        {
            get => cachedDefault ??= new BlockadeLabsSettings(configuration: null);
            internal set => cachedDefault = value;
        }

        public BlockadeLabsSettingsInfo Info { get; }

        public string BaseRequestUrlFormat => Info.BaseRequestUrlFormat;
    }
}

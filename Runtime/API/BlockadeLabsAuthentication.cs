using System;
using System.IO;
using System.Linq;
using UnityEngine;

namespace BlockadeLabsSDK
{
    public sealed class BlockadeLabsAuthentication
    {
        internal const string CONFIG_FILE = ".blockadelabs";
        private const string BLOCKADELABS_API_KEY = nameof(BLOCKADELABS_API_KEY);
        private const string BLOCKADE_LABS_API_KEY = nameof(BLOCKADE_LABS_API_KEY);

        public static implicit operator BlockadeLabsAuthentication(string apiKey) => new BlockadeLabsAuthentication(apiKey);

        /// <summary>
        /// Instantiates an empty Authentication object.
        /// </summary>
        public BlockadeLabsAuthentication() { }

        /// <summary>
        /// Instantiates a new Authentication object with the given <paramref name="apiKey"/>, which may be <see langword="null"/>.
        /// </summary>
        /// <param name="apiKey">The API key, required to access the API endpoint.</param>
        public BlockadeLabsAuthentication(string apiKey)
        {
            Info = new BlockadeLabsAuthInfo(apiKey);
            cachedDefault = this;
        }

        /// <summary>
        /// Instantiates a new Authentication object with the given <paramref name="authInfo"/>, which may be <see langword="null"/>.
        /// </summary>
        /// <param name="authInfo"></param>
        public BlockadeLabsAuthentication(BlockadeLabsAuthInfo authInfo)
        {
            Info = authInfo;
            cachedDefault = this;
        }

        /// <summary>
        /// Instantiates a new Authentication object with the given <see cref="configuration"/>.
        /// </summary>
        /// <param name="configuration"><see cref="BlockadeLabsConfiguration"/>.</param>
        public BlockadeLabsAuthentication(BlockadeLabsConfiguration configuration) : this(configuration.ApiKey) { }

        public BlockadeLabsAuthInfo Info { get; }

        private static BlockadeLabsAuthentication cachedDefault;

        /// <summary>
        /// The default authentication to use when no other auth is specified.
        /// This can be set manually, or automatically loaded via environment variables or a config file.
        /// <seealso cref="LoadFromEnvironment"/><seealso cref="LoadFromDirectory"/>
        /// </summary>
        public static BlockadeLabsAuthentication Default
        {
            get => cachedDefault ??= new BlockadeLabsAuthentication().LoadDefault();
            set => cachedDefault = value;
        }

        /// <summary>
        /// Attempts to load the authentication from a <see cref="BlockadeLabsConfiguration"/> asset.
        /// </summary>
        /// <returns>
        /// The loaded <see cref="BlockadeLabsAuthentication"/> or <see langword="null"/>.
        /// </returns>
        public BlockadeLabsAuthentication LoadFromAsset(BlockadeLabsConfiguration configuration = null)
        {
            if (configuration == null)
            {
                Debug.LogWarning($"This can be speed this up by passing a {nameof(BlockadeLabsConfiguration)} to the {nameof(BlockadeLabsAuthentication)}.ctr");
                configuration = Resources.LoadAll<BlockadeLabsConfiguration>(string.Empty).FirstOrDefault(o => o != null);
            }

            return configuration != null ? new BlockadeLabsAuthentication(configuration) : null;
        }

        /// <summary>
        /// Attempts to load the authentication from the system environment variables.
        /// </summary>
        /// <returns>
        /// The loaded <see cref="BlockadeLabsAuthentication"/> or <see langword="null"/>.
        /// </returns>
        public BlockadeLabsAuthentication LoadFromEnvironment()
        {
            var apiKey = Environment.GetEnvironmentVariable(BLOCKADELABS_API_KEY);

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                apiKey = Environment.GetEnvironmentVariable(BLOCKADE_LABS_API_KEY);
            }

            return string.IsNullOrEmpty(apiKey) ? null : new BlockadeLabsAuthentication(apiKey);
        }

        /// <summary>
        /// Attempts to load the authentication from a specified configuration file.
        /// </summary>
        /// <param name="path">
        /// The specified path to the configuration file.
        /// </param>
        /// <returns>
        /// The loaded <see cref="BlockadeLabsAuthentication"/> or <see langword="null"/>.
        /// </returns>
        public BlockadeLabsAuthentication LoadFromPath(string path)
            => LoadFromDirectory(Path.GetDirectoryName(path), Path.GetFileName(path), false);

        /// <summary>
        /// Attempts to load the authentication from the specified directory,
        /// optionally traversing up the directory tree.
        /// </summary>
        /// <param name="directory">
        /// The directory to look in, or <see langword="null"/> for the current directory.
        /// </param>
        /// <param name="filename">
        /// The filename of the config file.
        /// </param>
        /// <param name="searchUp">
        /// Whether to recursively traverse up the directory tree if the <paramref name="filename"/> is not found in the <paramref name="directory"/>.
        /// </param>
        /// <returns>
        /// The loaded <see cref="BlockadeLabsAuthentication"/> or <see langword="null"/>.
        /// </returns>
        /// ReSharper disable once OptionalParameterHierarchyMismatch
        public BlockadeLabsAuthentication LoadFromDirectory(string directory = null, string filename = CONFIG_FILE, bool searchUp = true)
        {
            if (string.IsNullOrWhiteSpace(directory))
            {
                directory = Environment.CurrentDirectory;
            }

            if (string.IsNullOrWhiteSpace(filename))
            {
                filename = CONFIG_FILE;
            }

            BlockadeLabsAuthInfo tempAuthInfo = null;

            var currentDirectory = new DirectoryInfo(directory);

            while (tempAuthInfo == null && currentDirectory.Parent != null)
            {
                var filePath = Path.Combine(currentDirectory.FullName, filename);

                if (File.Exists(filePath))
                {
                    try
                    {
                        tempAuthInfo = JsonUtility.FromJson<BlockadeLabsAuthInfo>(File.ReadAllText(filePath));
                        break;
                    }
                    catch (Exception)
                    {
                        // try to parse the old way for backwards support.
                    }

                    var lines = File.ReadAllLines(filePath);
                    string apiKey = null;

                    foreach (var line in lines)
                    {
                        var parts = line.Split('=', ':');

                        for (var i = 0; i < parts.Length - 1; i++)
                        {
                            var part = parts[i];
                            var nextPart = parts[i + 1];

                            switch (part)
                            {
                                case BLOCKADE_LABS_API_KEY:
                                case BLOCKADELABS_API_KEY:
                                    apiKey = nextPart.Trim();
                                    break;
                            }
                        }
                    }

                    tempAuthInfo = new BlockadeLabsAuthInfo(apiKey);
                }

                if (searchUp)
                {
                    currentDirectory = currentDirectory.Parent;
                }
                else
                {
                    break;
                }
            }

            return string.IsNullOrEmpty(tempAuthInfo?.ApiKey) ? null : new BlockadeLabsAuthentication(tempAuthInfo);
        }

        /// <summary>
        /// Attempts to load the default authentication based on order of precedence.<br/>
        /// 1. ScriptableObject<br/>
        /// 2. Directory<br/>
        /// 3. User Directory<br/>
        /// 4. Environment Variables<br/>
        /// </summary>
        /// <returns>
        /// The loaded <see cref="BlockadeLabsAuthentication"/> or <see langword="null"/>.
        /// </returns>
        public BlockadeLabsAuthentication LoadDefault()
            => LoadFromAsset() ??
               LoadFromDirectory() ??
               LoadFromDirectory(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)) ??
               LoadFromEnvironment();

        /// <summary>
        /// Attempts to load the default authentication based on order of precedence.<br/>
        /// 1. Environment Variables<br/>
        /// 2. User Directory<br/>
        /// 3. Directory<br/>
        /// 4. ScriptableObject<br/>
        /// </summary>
        /// <returns>
        /// The loaded <see cref="BlockadeLabsAuthentication"/> or <see langword="null"/>.
        /// </returns>
        public BlockadeLabsAuthentication LoadDefaultsReversed()
            => LoadFromEnvironment() ??
               LoadFromDirectory() ??
               LoadFromDirectory(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)) ??
               LoadFromAsset();
    }
}

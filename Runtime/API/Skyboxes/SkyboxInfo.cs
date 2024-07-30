using BlockadeLabsSDK.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Scripting;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace BlockadeLabsSDK
{
    [Preserve]
    public sealed class SkyboxInfo : BaseResponse, IStatus
    {
        [Preserve]
        [JsonConstructor]
        internal SkyboxInfo(
            [JsonProperty("id")] int id,
            [JsonProperty("obfuscated_id")] string obfuscatedId,
            [JsonProperty("skybox_style_id")] int skyboxStyleId,
            [JsonProperty("skybox_style_name")] string skyboxStyleName,
            [JsonProperty("model")] SkyboxModel model,
            [JsonProperty("status")] Status status,
            [JsonProperty("type")] string type,
            [JsonProperty("queue_position")] int queuePosition,
            [JsonProperty("file_url")] string mainTextureUrl,
            [JsonProperty("thumb_url")] string thumbUrl,
            [JsonProperty("depth_map_url")] string depthTextureUrl,
            [JsonProperty("title")] string title,
            [JsonProperty("prompt")] string prompt,
            [JsonProperty("negative_text")] string negativeText,
            [JsonProperty("seed")] int seed,
            [JsonProperty("remix_imagine_id")] int? remixId,
            [JsonProperty("remix_obfuscated_id")] string remixObfuscatedId,
            [JsonProperty("isMyFavorite")] bool isMyFavorite,
            [JsonProperty("created_at")] DateTime createdAt,
            [JsonProperty("updated_at")] DateTime updatedAt,
            [JsonProperty("dispatched_at")] DateTime dispatchedAt,
            [JsonProperty("processing_at")] DateTime processingAt,
            [JsonProperty("completed_at")] DateTime completedAt,
            [JsonProperty("error_message")] string errorMessage = null,
            [JsonProperty("pusher_channel")] string pusherChannel = null,
            [JsonProperty("pusher_event")] string pusherEvent = null,
            [JsonProperty("api_key_id")] int? apiKeyId = null,
            [JsonProperty("exports")] Dictionary<string, string> exports = null)
        {
            Id = id;
            ObfuscatedId = obfuscatedId;
            SkyboxStyleId = skyboxStyleId;
            SkyboxStyleName = skyboxStyleName;
            Model = model;
            Status = status;
            QueuePosition = queuePosition;
            Type = type;
            MainTextureUrl = mainTextureUrl;
            ThumbUrl = thumbUrl;
            DepthTextureUrl = depthTextureUrl;
            Title = title;
            Prompt = prompt;
            NegativeText = negativeText;
            Seed = seed;
            RemixId = remixId;
            RemixObfuscatedId = remixObfuscatedId;
            IsMyFavorite = isMyFavorite;
            CreatedAt = createdAt;
            UpdatedAt = updatedAt;
            DispatchedAt = dispatchedAt;
            ProcessingAt = processingAt;
            CompletedAt = completedAt;
            ErrorMessage = errorMessage;
            PusherChannel = pusherChannel;
            PusherEvent = pusherEvent;
            ApiKeyId = apiKeyId;
            exports ??= new Dictionary<string, string>();

            if (!exports.ContainsKey(SkyboxExportOption.Equirectangular_PNG))
            {
                exports.Add(SkyboxExportOption.Equirectangular_PNG, mainTextureUrl);
            }

            if (!exports.ContainsKey(SkyboxExportOption.DepthMap_PNG))
            {
                exports.Add(SkyboxExportOption.DepthMap_PNG, depthTextureUrl);
            }

            Exports = exports;
        }

        [Preserve]
        [JsonProperty("id")]
        public int Id { get; }

        [Preserve]
        [JsonProperty("obfuscated_id")]
        public string ObfuscatedId { get; }

        [Preserve]
        [JsonProperty("skybox_style_id")]
        public int SkyboxStyleId { get; }

        [Preserve]
        [JsonProperty("skybox_style_name")]
        public string SkyboxStyleName { get; }

        [Preserve]
        [JsonProperty("model")]
        [JsonConverter(typeof(StringEnumConverter))]
        public SkyboxModel Model { get; }

        [Preserve]
        [JsonProperty("status")]
        [JsonConverter(typeof(StringEnumConverter))]
        public Status Status { get; }

        [Preserve]
        [JsonProperty("queue_position")]
        public int QueuePosition { get; }

        [Preserve]
        [JsonProperty("type")]
        public string Type { get; }

        [Preserve]
        [JsonProperty("file_url")]
        public string MainTextureUrl { get; private set; }

        [Preserve]
        [JsonProperty("thumb_url")]
        public string ThumbUrl { get; }

        [Preserve]
        [JsonIgnore]
        public Texture2D Thumbnail { get; internal set; }

        [Preserve]
        [JsonProperty("depth_map_url")]
        public string DepthTextureUrl { get; private set; }

        [Preserve]
        [JsonProperty("title")]
        public string Title { get; }

        [Preserve]
        [JsonProperty("prompt")]
        public string Prompt { get; }

        [Preserve]
        [JsonProperty("negative_text")]
        public string NegativeText { get; }

        [Preserve]
        [JsonProperty("seed")]
        public int Seed { get; }

        [Preserve]
        [JsonProperty("remix_imagine_id")]
        public int? RemixId { get; }

        [Preserve]
        [JsonProperty("remix_obfuscated_id")]
        public string RemixObfuscatedId { get; }

        [Preserve]
        [JsonProperty("isMyFavorite")]
        public bool IsMyFavorite { get; private set; }

        [Preserve]
        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; }

        [Preserve]
        [JsonProperty("updated_at")]
        public DateTime UpdatedAt { get; }

        [Preserve]
        [JsonProperty("dispatched_at")]
        public DateTime DispatchedAt { get; }

        [Preserve]
        [JsonProperty("processing_at")]
        public DateTime ProcessingAt { get; }

        [Preserve]
        [JsonProperty("completed_at")]
        public DateTime CompletedAt { get; }

        [Preserve]
        [JsonProperty("error_message")]
        public string ErrorMessage { get; }

        [Preserve]
        [JsonProperty("pusher_channel")]
        public string PusherChannel { get; private set; }

        [Preserve]
        [JsonProperty("pusher_event")]
        public string PusherEvent { get; private set; }

        [JsonProperty("api_key_id")]
        internal int? ApiKeyId { get; }

        [Preserve]
        [JsonProperty("exports")]
        public IReadOnlyDictionary<string, string> Exports { get; }

        [Preserve]
        // ReSharper disable once InconsistentNaming
        internal readonly Dictionary<string, Object> exportedAssets = new Dictionary<string, Object>();

        [Preserve]
        [JsonIgnore]
        public IReadOnlyDictionary<string, Object> ExportedAssets => exportedAssets;

        [Preserve]
        public override string ToString() => JsonConvert.SerializeObject(this, Formatting.Indented, BlockadeLabsClient.JsonSerializationOptions);

        [Preserve]
        public static implicit operator int(SkyboxInfo skyboxInfo) => skyboxInfo.Id;

        /// <summary>
        /// Downloads and loads all the assets associated with this skybox.
        /// </summary>
        /// <param name="debug">Optional, debug downloads.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        [Preserve]
        public async Task LoadAssetsAsync(bool debug = false, CancellationToken cancellationToken = default)
        {
            async Task DownloadThumbnail()
            {
                if (!string.IsNullOrWhiteSpace(ThumbUrl))
                {
                    await new UnityMainThread();
                    Rest.TryGetFileNameFromUrl(ThumbUrl, out var filename);
                    Thumbnail = await Rest.DownloadTextureAsync(ThumbUrl, fileName: $"{ObfuscatedId}-thumb{Path.GetExtension(filename)}", debug, cancellationToken);
                }
            }

            var downloadTasks = new List<Task> { DownloadThumbnail() };
            downloadTasks.AddRange(Exports.Select(kvp => DownloadExport(kvp, debug, cancellationToken)));
            await Task.WhenAll(downloadTasks).ConfigureAwait(true);
        }

        /// <summary>
        /// Downloads and loads all the assets associated with this skybox using the specified export option.
        /// </summary>
        /// <param name="exportOption"><see cref="SkyboxExportOption"/>.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        [Preserve]
        public async Task LoadAssetAsync(SkyboxExportOption exportOption, CancellationToken cancellationToken = default)
            => await LoadAssetsAsync(new[] { exportOption }, cancellationToken);

        /// <summary>
        /// Downloads and loads all the assets associated with this skybox using the specified export option.
        /// </summary>
        /// <param name="exportOptions"><see cref="SkyboxExportOption"/>.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        [Preserve]
        public async Task LoadAssetsAsync(SkyboxExportOption[] exportOptions, CancellationToken cancellationToken = default)
        {
            try
            {
                await new UnityMainThread();
                var downloadTasks = new List<Task>();
                foreach (var export in exportOptions)
                {
                    if (Exports.TryGetValue(export, out var exportUrl))
                    {
                        downloadTasks.Add(DownloadExport(new KeyValuePair<string, string>(export, exportUrl), false, cancellationToken));
                    }
                }

                await Task.WhenAll(downloadTasks).ConfigureAwait(true);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        [Preserve]
        private async Task DownloadExport(KeyValuePair<string, string> export, bool debug, CancellationToken cancellationToken)
        {
            try
            {
                await new UnityMainThread();
                var exportUrl = export.Value;

                if (!string.IsNullOrWhiteSpace(exportUrl))
                {
                    Rest.TryGetFileNameFromUrl(exportUrl, out var filename);
                    var path = $"{ObfuscatedId}-{export.Key}{Path.GetExtension(filename)}";

                    switch (export.Key)
                    {
                        case SkyboxExportOption.DepthMap_PNG:
                        case SkyboxExportOption.Equirectangular_PNG:
                        case SkyboxExportOption.Equirectangular_JPG:
                            var texture = await Rest.DownloadTextureAsync(exportUrl, path, debug, cancellationToken);
                            exportedAssets[export.Key] = texture;
                            break;
                        case SkyboxExportOption.CubeMap_PNG:
                        case SkyboxExportOption.CubeMap_Roblox_PNG:
                            var zipPath = await Rest.DownloadFileAsync(exportUrl, path, debug: debug, cancellationToken: cancellationToken);
                            var files = await ExportUtilities.UnZipAsync(zipPath, cancellationToken);
                            var textures = new List<Texture2D>();

                            foreach (var file in files)
                            {
                                var face = await Rest.DownloadTextureAsync($"file://{file}", debug: debug, cancellationToken: cancellationToken);
                                textures.Add(face);
                            }

                            exportedAssets[export.Key] = ExportUtilities.BuildCubemap(textures);
                            break;
                        case SkyboxExportOption.HDRI_HDR:
                        case SkyboxExportOption.HDRI_EXR:
                        case SkyboxExportOption.Video_LandScape_MP4:
                        case SkyboxExportOption.Video_Portrait_MP4:
                        case SkyboxExportOption.Video_Square_MP4:
                            await Rest.DownloadFileAsync(exportUrl, path, debug: debug, cancellationToken: cancellationToken);
                            break;
                        default:
                            Debug.LogWarning($"No download task defined for {export.Key}!");
                            break;
                    }
                }
                else
                {
                    Debug.LogError($"No valid url for skybox {ObfuscatedId}.{export.Key}");
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        /// <summary>
        /// Attempts to get the local cached path for the specified export option.
        /// </summary>
        /// <param name="key"><see cref="SkyboxExportOption"/>.</param>
        /// <param name="localCachedPath">The file cache path.</param>
        /// <returns>True, if asset is found, otherwise false.</returns>
        [Preserve]
        public bool TryGetAssetCachePath(SkyboxExportOption key, out string localCachedPath)
        {
            if (Exports.TryGetValue(key, out var exportUrl) &&
                Rest.TryGetFileNameFromUrl(exportUrl, out var filename))
            {
                var cachePath = Path.Combine(Rest.DownloadCacheDirectory, $"{ObfuscatedId}-{key}{Path.GetExtension(filename)}");
                return Rest.TryGetDownloadCacheItem($"file://{cachePath}", out localCachedPath);
            }

            localCachedPath = string.Empty;
            return false;
        }

        /// <summary>
        /// Attempts to get the asset for the specified export option.
        /// </summary>
        /// <typeparam name="T">Type of asset to load.</typeparam>
        /// <param name="key"><see cref="SkyboxExportOption"/>.</param>
        /// <param name="asset">The asset to load.</param>
        /// <returns>True, if the asset exists and was loaded.</returns>
        [Preserve]
        public bool TryGetAsset<T>(SkyboxExportOption key, out T asset) where T : Object
        {
            if (ExportedAssets.TryGetValue(key, out var obj))
            {
                asset = (T)obj;
                return true;
            }

            if (Exports.ContainsKey(key))
            {
                Debug.LogWarning($"{key} exists, but has not been loaded. Have you called {nameof(LoadAssetsAsync)}?");
            }

            asset = default;
            return false;
        }
    }
}

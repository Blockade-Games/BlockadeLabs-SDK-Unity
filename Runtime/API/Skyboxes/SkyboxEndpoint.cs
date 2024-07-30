using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Scripting;

namespace BlockadeLabsSDK
{
    public sealed class SkyboxEndpoint : BlockadeLabsBaseEndpoint
    {
        [Preserve]
        internal class SkyboxInfoRequest
        {
            [Preserve]
            [JsonConstructor]
            public SkyboxInfoRequest(
                [JsonProperty("request")] SkyboxInfo request = null,
                [JsonProperty("imagine")] SkyboxInfo imagine = null)
            {
                SkyboxInfo = request ?? imagine;
            }

            [Preserve]
            [JsonProperty("request")]
            public SkyboxInfo SkyboxInfo { get; }
        }

        [Preserve]
        private class SkyboxOperation
        {
            [Preserve]
            [JsonConstructor]
            public SkyboxOperation(
                [JsonProperty("success")] string success,
                [JsonProperty("error")] string error)
            {
                Success = success;
                Error = error;
            }

            [Preserve]
            [JsonProperty("success")]
            public string Success { get; }

            [Preserve]
            [JsonProperty("error")]
            public string Error { get; }
        }

        private class SkyboxTip
        {
            [JsonConstructor]
            public SkyboxTip([JsonProperty("tip")] string tip)
            {
                Tip = tip;
            }

            [JsonProperty("tip")]
            public string Tip { get; }
        }

        public SkyboxEndpoint(BlockadeLabsClient client) : base(client) { }

        protected override string Root => string.Empty;

        internal async Task<string> GetOneTipAsync()
        {
            var response = await Rest.GetAsync(GetUrl("skybox/get-one-tip-unity"), client.DefaultRequestHeaders, CancellationToken.None);
            response.Validate(EnableDebug);
            return JsonConvert.DeserializeObject<SkyboxTip>(response.Body, BlockadeLabsClient.JsonSerializationOptions)?.Tip;
        }

        internal async Task<SkyboxInfo> ToggleFavoriteAsync(int id, CancellationToken cancellationToken = default)
        {
            var response = await Rest.PostAsync(GetUrl($"imagine/favorite/{id}"), formData: null, client.DefaultRequestHeaders, cancellationToken);
            response.Validate(EnableDebug);
            var skyboxInfo = JsonConvert.DeserializeObject<SkyboxInfoRequest>(response.Body, BlockadeLabsClient.JsonSerializationOptions).SkyboxInfo;
            skyboxInfo.SetResponseData(response, client);
            return skyboxInfo;
        }

        /// <summary>
        /// Returns the list of predefined styles that can influence the overall aesthetic of your skybox generation.
        /// </summary>
        /// <param name="model">The <see cref="SkyboxModel"/> to get styles for.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns>A list of <see cref="SkyboxStyle"/>s.</returns>
        public async Task<IReadOnlyList<SkyboxStyle>> GetSkyboxStylesAsync(SkyboxModel model, CancellationToken cancellationToken = default)
        {
            var @params = new Dictionary<string, string> { { "model_version", ((int)model).ToString() } };
            var response = await Rest.GetAsync(GetUrl("skybox/styles", @params), client.DefaultRequestHeaders, cancellationToken);
            response.Validate(EnableDebug);
            return JsonConvert.DeserializeObject<IReadOnlyList<SkyboxStyle>>(response.Body, BlockadeLabsClient.JsonSerializationOptions)
                .Where(style => style.FamilyStyles != null ? style.FamilyStyles[0].Model == model : style.Model == model).ToList();
        }

        /// <summary>
        /// Returns the list of predefined styles that can influence the overall aesthetic of your skybox generation, sorted by style family.
        /// This route can be used in order to build a menu of styles sorted by family.
        /// </summary>
        /// <param name="model">Optional, The <see cref="SkyboxModel"/> to get styles for.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns>A list of <see cref="SkyboxStyle"/>s.</returns>
        public async Task<IReadOnlyList<SkyboxStyle>> GetSkyboxStyleFamiliesAsync(SkyboxModel? model = null, CancellationToken cancellationToken = default)
        {
            Dictionary<string, string> @params = null;

            if (model != null)
            {
                @params = new Dictionary<string, string> { { "model_version", ((int)model).ToString() } };
            }

            var response = await Rest.GetAsync(GetUrl("skybox/families", @params), client.DefaultRequestHeaders, cancellationToken);
            response.Validate(EnableDebug);
            var families = JsonConvert.DeserializeObject<IReadOnlyList<SkyboxStyle>>(response.Body, BlockadeLabsClient.JsonSerializationOptions);
            return model != null
                ? families.Where(style => style.FamilyStyles != null ? style.FamilyStyles[0].Model == model : style.Model == model).ToList()
                : families;
        }

        /// <summary>
        /// Returns the list of predefined styles for the generation menu.
        /// </summary>
        /// <param name="model">The <see cref="SkyboxModel"/> to get styles for.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns>A list of <see cref="SkyboxStyle"/>s.</returns>
        internal async Task<IReadOnlyList<SkyboxStyle>> GetSkyboxStylesMenuAsync(SkyboxModel model, CancellationToken cancellationToken = default)
        {
            var @params = new Dictionary<string, string> { { "model_version", ((int)model).ToString() } };
            var response = await Rest.GetAsync(GetUrl("skybox/menu", @params), client.DefaultRequestHeaders, cancellationToken);
            response.Validate(EnableDebug);
            return JsonConvert.DeserializeObject<IReadOnlyList<SkyboxStyle>>(response.Body, BlockadeLabsClient.JsonSerializationOptions)
                .Where(style => style.FamilyStyles != null ? style.FamilyStyles[0].Model == model : style.Model == model).ToList();
        }

        /// <summary>
        /// Generate a skybox image.
        /// </summary>
        /// <param name="skyboxRequest"><see cref="SkyboxRequest"/>.</param>
        /// <param name="exportOption">Optional, <see cref="SkyboxExportOption"/>.</param>
        /// <param name="progressCallback">Optional, <see cref="IProgress{SkyboxInfo}"/> progress callback.</param>
        /// <param name="pollingInterval">Optional, polling interval in seconds.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns><see cref="SkyboxInfo"/>.</returns>
        public async Task<SkyboxInfo> GenerateSkyboxAsync(SkyboxRequest skyboxRequest, SkyboxExportOption exportOption, IProgress<SkyboxInfo> progressCallback = null, float? pollingInterval = null, CancellationToken cancellationToken = default)
            => await GenerateSkyboxAsync(skyboxRequest, new[] { exportOption }, progressCallback, pollingInterval, cancellationToken);

        /// <summary>
        /// Generate a skybox image.
        /// </summary>
        /// <param name="skyboxRequest"><see cref="SkyboxRequest"/>.</param>
        /// <param name="exportOptions">Optional, <see cref="SkyboxExportOption"/>s.</param>
        /// <param name="progressCallback">Optional, <see cref="IProgress{SkyboxInfo}"/> progress callback.</param>
        /// <param name="pollingInterval">Optional, polling interval in seconds.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns><see cref="SkyboxInfo"/>.</returns>
        public async Task<SkyboxInfo> GenerateSkyboxAsync(SkyboxRequest skyboxRequest, SkyboxExportOption[] exportOptions = null, IProgress<SkyboxInfo> progressCallback = null, float? pollingInterval = null, CancellationToken cancellationToken = default)
        {
            pollingInterval ??= 1f;
            var formData = new WWWForm();
            formData.AddField("prompt", skyboxRequest.Prompt);

            if (!string.IsNullOrWhiteSpace(skyboxRequest.NegativeText))
            {
                formData.AddField("negative_text", skyboxRequest.NegativeText);
            }

            if (skyboxRequest.EnhancePrompt.HasValue)
            {
                formData.AddField("enhance_prompt", skyboxRequest.EnhancePrompt.ToString().ToLower());
            }

            if (skyboxRequest.Seed.HasValue)
            {
                formData.AddField("seed", skyboxRequest.Seed.Value);
            }

            if (skyboxRequest.SkyboxStyleId.HasValue)
            {
                formData.AddField("skybox_style_id", skyboxRequest.SkyboxStyleId.Value);
            }

            if (skyboxRequest.RemixImagineId.HasValue)
            {
                formData.AddField("remix_imagine_id", skyboxRequest.RemixImagineId.Value);
            }

            if (skyboxRequest.HqDepth.HasValue)
            {
                formData.AddField("return_depth_hq", skyboxRequest.HqDepth.Value.ToString().ToLower());
            }

            if (skyboxRequest.ControlImage != null)
            {
                if (!string.IsNullOrWhiteSpace(skyboxRequest.ControlModel))
                {
                    formData.AddField("control_model", skyboxRequest.ControlModel);
                }

                using var imageData = new MemoryStream();
#if UNITY_2021_1_OR_NEWER
                await skyboxRequest.ControlImage.CopyToAsync(imageData, cancellationToken);
#else
                await skyboxRequest.ControlImage.CopyToAsync(imageData);
#endif
                formData.AddBinaryData("control_image", imageData.ToArray(), skyboxRequest.ControlImageFileName);
                skyboxRequest.Dispose();
            }

            var response = await Rest.PostAsync(GetUrl("skybox"), formData, client.DefaultRequestHeaders, cancellationToken);
            response.Validate(EnableDebug);
            var skyboxInfo = JsonConvert.DeserializeObject<SkyboxInfo>(response.Body, BlockadeLabsClient.JsonSerializationOptions);
            progressCallback?.Report(skyboxInfo);
#if PUSHER_PRESENT
            try
            {
                skyboxInfo = await WaitForStatusChange(skyboxInfo.ObfuscatedId, skyboxInfo.PusherChannel, skyboxInfo.PusherEvent, progressCallback, pollingInterval.Value, cancellationToken);
            }
            finally
#else
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay((int)(pollingInterval.Value * 1000), CancellationToken.None).ConfigureAwait(true);
                skyboxInfo = await GetSkyboxInfoAsync(skyboxInfo, CancellationToken.None);
                progressCallback?.Report(skyboxInfo);

                if (skyboxInfo.Status == Status.Pending ||
                    skyboxInfo.Status == Status.Processing ||
                    skyboxInfo.Status == Status.Dispatched)
                {
                    continue;
                }

                break;
            }
#endif // PUSHER_PRESENT
            {
                if (cancellationToken.IsCancellationRequested && skyboxInfo != null)
                {
                    var cancelResult = await CancelSkyboxGenerationAsync(skyboxInfo, CancellationToken.None);

                    if (!cancelResult)
                    {
                        throw new Exception($"Failed to cancel generation for {skyboxInfo.Id}");
                    }
                }
            }

            cancellationToken.ThrowIfCancellationRequested();

            if (skyboxInfo.Status == Status.Abort)
            {
                throw new OperationCanceledException($"Generation aborted for skybox {skyboxInfo.Id}\n{skyboxInfo.ErrorMessage}\n{skyboxInfo}");
            }

            if (skyboxInfo.Status != Status.Complete)
            {
                throw new Exception($"Failed to generate skybox! {skyboxInfo.Id} -> {skyboxInfo.Status}\nError: {skyboxInfo.ErrorMessage}\n{skyboxInfo}");
            }

            skyboxInfo.SetResponseData(response, client);
            var exportTasks = new List<Task>();

            try
            {
                if (exportOptions != null)
                {
                    exportTasks.AddRange(exportOptions.Select(exportOption => ExportSkyboxAsync(skyboxInfo, exportOption, null, pollingInterval, cancellationToken)));
                }
                else
                {
                    exportTasks.Add(ExportSkyboxAsync(skyboxInfo, DefaultExportOptions.Equirectangular_JPG, null, pollingInterval, cancellationToken));
                    exportTasks.Add(ExportSkyboxAsync(skyboxInfo, DefaultExportOptions.DepthMap_PNG, null, pollingInterval, cancellationToken));
                }

                await Task.WhenAll(exportTasks).ConfigureAwait(true);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to download skybox export!\n{e}");
            }

            skyboxInfo = await GetSkyboxInfoAsync(skyboxInfo.Id, cancellationToken);
            await skyboxInfo.LoadAssetsAsync(EnableDebug, cancellationToken);
            skyboxInfo.SetResponseData(response, client);
            return skyboxInfo;
        }

        /// <summary>
        /// Returns the skybox metadata for the given skybox id.
        /// </summary>
        /// <param name="id">Skybox Id.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns><see cref="SkyboxInfo"/>.</returns>
        public async Task<SkyboxInfo> GetSkyboxInfoAsync(int id, CancellationToken cancellationToken = default)
        {
            var response = await Rest.GetAsync(GetUrl($"imagine/requests/{id}"), client.DefaultRequestHeaders, cancellationToken);
            response.Validate(EnableDebug);
            var skyboxInfo = JsonConvert.DeserializeObject<SkyboxInfoRequest>(response.Body, BlockadeLabsClient.JsonSerializationOptions).SkyboxInfo;
            skyboxInfo.SetResponseData(response, client);
            return skyboxInfo;
        }

        /// <summary>
        /// Returns the skybox metadata for the given skybox obfuscatedId.
        /// </summary>
        /// <param name="obfuscatedId">Skybox obfuscatedId.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns><see cref="SkyboxInfo"/>.</returns>
        public async Task<SkyboxInfo> GetSkyboxInfoAsync(string obfuscatedId, CancellationToken cancellationToken = default)
        {
            var response = await Rest.GetAsync(GetUrl($"imagine/requests/obfuscated-id/{obfuscatedId}"), client.DefaultRequestHeaders, cancellationToken);
            response.Validate(EnableDebug);
            var skyboxInfo = JsonConvert.DeserializeObject<SkyboxInfoRequest>(response.Body, BlockadeLabsClient.JsonSerializationOptions).SkyboxInfo;
            skyboxInfo.SetResponseData(response, client);
            return skyboxInfo;
        }

        /// <summary>
        /// Deletes a skybox by id.
        /// </summary>
        /// <param name="id">The id of the skybox.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns>True, if skybox was successfully deleted.</returns>
        public async Task<bool> DeleteSkyboxAsync(int id, CancellationToken cancellationToken = default)
        {
            var response = await Rest.DeleteAsync(GetUrl($"imagine/deleteImagine/{id}"), client.DefaultRequestHeaders, cancellationToken);
            response.Validate(EnableDebug);
            var skyboxOp = JsonConvert.DeserializeObject<SkyboxOperation>(response.Body, BlockadeLabsClient.JsonSerializationOptions);

            const string successStatus = "Item deleted successfully";

            if (skyboxOp == null || skyboxOp.Success != successStatus)
            {
                throw new Exception($"Failed to delete skybox {id}!\n{skyboxOp?.Error}");
            }

            return skyboxOp.Success.Equals(successStatus);
        }

        /// <summary>
        /// Gets the previously generated skyboxes.
        /// </summary>
        /// <param name="parameters">Optional, <see cref="SkyboxHistoryParameters"/>.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns><see cref="SkyboxHistory"/>.</returns>
        public async Task<SkyboxHistory> GetSkyboxHistoryAsync(SkyboxHistoryParameters parameters = null, CancellationToken cancellationToken = default)
        {
            var historyRequest = parameters ?? new SkyboxHistoryParameters();

            var @params = new Dictionary<string, string>();

            if (historyRequest.StatusFilter.HasValue)
            {
                @params.Add("status", historyRequest.StatusFilter.ToString().ToLower());
            }

            if (historyRequest.Limit.HasValue)
            {
                @params.Add("limit", historyRequest.Limit.ToString());
            }

            if (historyRequest.Offset.HasValue)
            {
                @params.Add("offset", historyRequest.Offset.ToString());
            }

            if (historyRequest.Order.HasValue)
            {
                @params.Add("order", historyRequest.Order.ToString().ToUpper());
            }

            if (historyRequest.ImagineId.HasValue)
            {
                @params.Add("imagine_id", historyRequest.ImagineId.ToString());
            }

            if (!string.IsNullOrWhiteSpace(historyRequest.QueryFilter))
            {
                @params.Add("query", UnityWebRequest.EscapeURL(historyRequest.QueryFilter));
            }

            if (!string.IsNullOrWhiteSpace(historyRequest.GeneratorFilter))
            {
                @params.Add("generator", UnityWebRequest.EscapeURL(historyRequest.GeneratorFilter));
            }

            if (historyRequest.FavoritesOnly.HasValue &&
                historyRequest.FavoritesOnly.Value)
            {
                @params.Add("my_likes", historyRequest.FavoritesOnly.Value.ToString().ToLower());
            }

            if (historyRequest.GeneratedBy.HasValue)
            {
                @params.Add("api_key_id", historyRequest.GeneratedBy.Value.ToString());
            }

            if (historyRequest.SkyboxStyleId.HasValue && historyRequest.SkyboxStyleId.Value > 0)
            {
                @params.Add("skybox_style_id", historyRequest.SkyboxStyleId.ToString());
            }

            var response = await Rest.GetAsync(GetUrl("imagine/myRequests", @params), client.DefaultRequestHeaders, cancellationToken);
            response.Validate(EnableDebug);
            var skyboxHistory = JsonConvert.DeserializeObject<SkyboxHistory>(response.Body, BlockadeLabsClient.JsonSerializationOptions);
            skyboxHistory.SetResponseData(response, client);
            return skyboxHistory;
        }

        /// <summary>
        /// Cancels a pending skybox generation request by id.
        /// </summary>
        /// <param name="id">The id of the skybox.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns>True, if generation was cancelled.</returns>
        public async Task<bool> CancelSkyboxGenerationAsync(int id, CancellationToken cancellationToken = default)
        {
            var response = await Rest.DeleteAsync(GetUrl($"imagine/requests/{id}"), client.DefaultRequestHeaders, cancellationToken);
            response.Validate(EnableDebug);
            var skyboxOp = JsonConvert.DeserializeObject<SkyboxOperation>(response.Body, BlockadeLabsClient.JsonSerializationOptions);

            if (skyboxOp == null || skyboxOp.Success != "true")
            {
                throw new Exception($"Failed to cancel generation for skybox {id}!\n{skyboxOp?.Error}");
            }

            return skyboxOp.Success.Equals("true");
        }

        /// <summary>
        /// Cancels ALL pending skybox generation requests.
        /// </summary>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns>True, if all generations are cancelled.</returns>
        public async Task<bool> CancelAllPendingSkyboxGenerationsAsync(CancellationToken cancellationToken = default)
        {
            var response = await Rest.DeleteAsync(GetUrl("imagine/requests/pending"), client.DefaultRequestHeaders, cancellationToken);
            response.Validate(EnableDebug);
            var skyboxOp = JsonConvert.DeserializeObject<SkyboxOperation>(response.Body, BlockadeLabsClient.JsonSerializationOptions);

            if (skyboxOp == null || skyboxOp.Success != "true")
            {
                if (skyboxOp != null &&
                    skyboxOp.Error.Contains("You don't have any pending"))
                {
                    return false;
                }

                throw new Exception($"Failed to cancel all pending skybox generations!\n{skyboxOp?.Error}");
            }

            return skyboxOp.Success.Equals("true");
        }

        /// <summary>
        /// Returns the list of all available export types.
        /// </summary>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns>A list of available export types.</returns>
        public async Task<IReadOnlyList<SkyboxExportOption>> GetAllSkyboxExportOptionsAsync(CancellationToken cancellationToken = default)
        {
            var response = await Rest.GetAsync(GetUrl("skybox/export"), client.DefaultRequestHeaders, cancellationToken);
            response.Validate(EnableDebug);
            return JsonConvert.DeserializeObject<IReadOnlyList<SkyboxExportOption>>(response.Body, BlockadeLabsClient.JsonSerializationOptions);
        }

        /// <summary>
        /// Exports the <see cref="SkyboxInfo"/> using the provided <see cref="SkyboxExportOption"/>.
        /// </summary>
        /// <param name="skyboxInfo">Skybox to export.</param>
        /// <param name="exportOption">Export option to use.</param>
        /// <param name="progressCallback">Optional, <see cref="IProgress{SkyboxExportRequest}"/> progress callback.</param>
        /// <param name="pollingInterval">Optional, polling interval in seconds.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns>Updated <see cref="SkyboxInfo"/> with exported assets loaded into memory.</returns>
        public async Task<SkyboxInfo> ExportSkyboxAsync(SkyboxInfo skyboxInfo, SkyboxExportOption exportOption, IProgress<SkyboxExportRequest> progressCallback = null, float? pollingInterval = null, CancellationToken cancellationToken = default)
        {
            pollingInterval ??= 1f;
            var payload = $"{{\"skybox_id\":\"{skyboxInfo.ObfuscatedId}\",\"type_id\":{exportOption.Id}}}";
            var response = await Rest.PostAsync(GetUrl("skybox/export"), payload, client.DefaultRequestHeaders, cancellationToken);
            response.Validate(EnableDebug);
            var exportRequest = JsonConvert.DeserializeObject<SkyboxExportRequest>(response.Body, BlockadeLabsClient.JsonSerializationOptions);
            progressCallback?.Report(exportRequest);

#if PUSHER_PRESENT
            if (exportRequest.Status != Status.Complete)
            {
                exportRequest = await WaitForStatusChange(exportRequest, exportRequest.PusherChannel, exportRequest.PusherEvent, progressCallback, pollingInterval.Value, cancellationToken);
            }
#else
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay((int)(pollingInterval.Value * 1000), CancellationToken.None).ConfigureAwait(true);
                exportRequest = await GetExportRequestStatusAsync(exportRequest, CancellationToken.None);
                progressCallback?.Report(exportRequest);

                if (exportRequest.Status == Status.Pending ||
                    exportRequest.Status == Status.Processing ||
                    exportRequest.Status == Status.Dispatched)
                {
                    continue;
                }

                break;
            }
#endif // PUSHER_PRESENT
            if (cancellationToken.IsCancellationRequested && exportRequest != null)
            {
                var cancelResult = await CancelSkyboxExportAsync(exportRequest, CancellationToken.None);

                if (!cancelResult)
                {
                    throw new Exception($"Failed to cancel export for {exportRequest.Id}");
                }
            }

            cancellationToken.ThrowIfCancellationRequested();

            if (exportRequest!.Status == Status.Abort)
            {
                throw new OperationCanceledException($"Export aborted for skybox {skyboxInfo.Id}\n{exportRequest.ErrorMessage}\n{exportRequest}");
            }

            if (exportRequest.Status != Status.Complete)
            {
                throw new Exception($"Failed to export skybox! {exportRequest.Id} -> {exportRequest.Status}\nError: {exportRequest.ErrorMessage}\n{exportRequest}");
            }

            skyboxInfo = await GetSkyboxInfoAsync(skyboxInfo.Id, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            skyboxInfo.SetResponseData(response, client);
            return skyboxInfo;
        }

        /// <summary>
        /// Gets the status of a specified <see cref="SkyboxExportRequest"/>.
        /// </summary>
        /// <param name="exportRequestId">The export request id to get the current status for.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns>Updated <see cref="SkyboxExportRequest"/> with latest information.</returns>
        public async Task<SkyboxExportRequest> GetExportRequestStatusAsync(string exportRequestId, CancellationToken cancellationToken = default)
        {
            var response = await Rest.GetAsync(GetUrl($"skybox/export/{exportRequestId}"), client.DefaultRequestHeaders, cancellationToken);
            response.Validate(EnableDebug);
            var exportRequest = JsonConvert.DeserializeObject<SkyboxExportRequest>(response.Body, BlockadeLabsClient.JsonSerializationOptions);
            exportRequest.SetResponseData(response, client);
            return exportRequest;
        }

        /// <summary>
        /// Cancels the specified <see cref="SkyboxExportRequest"/>.
        /// </summary>
        /// <param name="exportRequest">The export option to cancel.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns>True, if generation was cancelled.</returns>
        public async Task<bool> CancelSkyboxExportAsync(SkyboxExportRequest exportRequest, CancellationToken cancellationToken = default)
        {
            var response = await Rest.DeleteAsync(GetUrl($"skybox/export/{exportRequest.Id}"), client.DefaultRequestHeaders, cancellationToken);
            response.Validate(EnableDebug);
            var skyboxOp = JsonConvert.DeserializeObject<SkyboxOperation>(response.Body, BlockadeLabsClient.JsonSerializationOptions);

            if (skyboxOp == null || skyboxOp.Success != "true")
            {
                throw new Exception($"Failed to cancel export for request {exportRequest.Id}!\n{skyboxOp?.Error}");
            }

            return skyboxOp.Success.Equals("true");
        }

#if PUSHER_PRESENT
        private PusherClient.Pusher _pusher;

        private PusherClient.Pusher Pusher
        {
            get
            {
                if (_pusher != null)
                {
                    return _pusher;
                }

                const string key = "a6a7b7662238ce4494d5";
                const string cluster = "mt1";
                _pusher = new PusherClient.Pusher(key, new PusherClient.PusherOptions
                {
                    Cluster = cluster,
                    Encrypted = true
                });

                _pusher.Error += (_, e) => Debug.LogException(e);

                if (EnableDebug)
                {
                    _pusher.ConnectionStateChanged += (_, state) => Debug.Log($"Pusher Connection State: {state}");
                    _pusher.Subscribed += (_, channel) => Debug.Log($"Pusher Subscribed: {channel.Name}");
                }

                return _pusher;
            }
        }

        private async Task<T> WaitForStatusChange<T>(string obfuscatedId, string pusherChannel, string pusherEvent, IProgress<T> progressCallback, float pollingInterval, CancellationToken cancellationToken)
            where T : IStatus
        {
            if (Pusher.State == PusherClient.ConnectionState.Uninitialized)
            {
                await Pusher.ConnectAsync().ConfigureAwait(true);
            }

            var channel = await Pusher.SubscribeAsync(pusherChannel).ConfigureAwait(true);

            if (channel == null)
            {
                throw new Exception($"Failed to subscribe to pusher channel {pusherChannel}!");
            }

            var tcs = new TaskCompletionSource<T>();
            T partial = default;
            int timer = 0;

            try
            {
                channel.Bind(pusherEvent, (string @event) =>
                {
                    if (EnableDebug)
                    {
                        Debug.Log($"Pusher Event: [{pusherChannel}] {@event}");
                    }

                    try
                    {
                        var data = JsonConvert.DeserializeObject<PusherEvent>(@event).Data;
                        var result = JsonConvert.DeserializeObject<T>(data, BlockadeLabsClient.JsonSerializationOptions);
                        progressCallback?.Report(result);
                        partial = result;

                        if (result.Status == Status.Complete ||
                            result.Status == Status.Error ||
                            result.Status == Status.Abort)
                        {
                            tcs.TrySetResult(result);
                        }
                    }
                    catch (Exception e)
                    {
                        tcs.TrySetException(e);
                    }
                });

                while (!tcs.Task.IsCompleted && !tcs.Task.IsCanceled && !tcs.Task.IsFaulted)
                {
                    // ReSharper disable once MethodSupportsCancellation
                    await Task.Delay(16).ConfigureAwait(true);
                    cancellationToken.ThrowIfCancellationRequested();
                    timer += 16;

                    if (partial != null && timer >= pollingInterval * 1000)
                    {
                        timer = 0;
                        progressCallback?.Report(partial);
                    }

                    if (timer >= pollingInterval * 1000 * 10)
                    {
                        timer = 0;

                        if (typeof(T).IsAssignableFrom(typeof(SkyboxInfo)))
                        {
                            var skyboxInfo = await GetSkyboxInfoAsync(obfuscatedId, CancellationToken.None);

                            if (skyboxInfo.Status == Status.Complete ||
                                skyboxInfo.Status == Status.Error ||
                                skyboxInfo.Status == Status.Abort)
                            {
                                tcs.TrySetResult((T)(object)skyboxInfo);
                            }
                            else
                            {
                                progressCallback?.Report((T)(object)skyboxInfo);
                            }
                        }
                        else if (typeof(T).IsAssignableFrom(typeof(SkyboxExportRequest)))
                        {
                            var requestInfo = await GetExportRequestStatusAsync(obfuscatedId, CancellationToken.None);

                            if (requestInfo.Status == Status.Complete ||
                                requestInfo.Status == Status.Error ||
                                requestInfo.Status == Status.Abort)
                            {
                                tcs.TrySetResult((T)(object)requestInfo);
                            }
                            else
                            {
                                progressCallback?.Report((T)(object)requestInfo);
                            }
                        }
                        else
                        {
                            throw new NotSupportedException($"Unsupported type {typeof(T)}");
                        }
                    }
                }

                return await tcs.Task;
            }
            catch (Exception e)
            {
                switch (e)
                {
                    case TaskCanceledException _:
                    case OperationCanceledException _:
                        // ignored
                        return partial;
                    default:
                        Debug.LogException(e);
                        throw;
                }
            }
            finally
            {
                channel.Unbind(pusherEvent);
                await Pusher.UnsubscribeAsync(pusherChannel).ConfigureAwait(true);
            }
        }

        [Preserve]
        private class PusherEvent
        {
            [Preserve]
            [JsonConstructor]
            public PusherEvent([JsonProperty("data")] string data)
            {
                Data = data;
            }

            [Preserve]
            [JsonProperty("data")]
            public string Data;
        }
#endif // PUSHER_PRESENT
    }
}

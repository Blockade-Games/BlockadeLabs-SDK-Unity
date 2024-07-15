using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using UnityEngine.Scripting;

namespace BlockadeLabsSDK
{
    [Preserve]
    public sealed class SkyboxExportRequest : BaseResponse, IStatus
    {
        [Preserve]
        [JsonConstructor]
        internal SkyboxExportRequest(
            [JsonProperty("id")] string id,
            [JsonProperty("file_url")] string fileUrl,
            [JsonProperty("skybox_obfuscated_id")] string skyboxObfuscatedId,
            [JsonProperty("type")] string type,
            [JsonProperty("type_id")] int typeId,
            [JsonProperty("status")] Status status,
            [JsonProperty("queue_position")] int queuePosition,
            [JsonProperty("error_message")] string errorMessage,
            [JsonProperty("pusher_channel")] string pusherChannel,
            [JsonProperty("pusher_event")] string pusherEvent,
            [JsonProperty("webhook_url")] string webhookUrl,
            [JsonProperty("created_at")] DateTime createdAt)
        {
            Id = id;
            FileUrl = fileUrl;
            SkyboxObfuscatedId = skyboxObfuscatedId;
            Type = type;
            TypeId = typeId;
            Status = status;
            QueuePosition = queuePosition;
            ErrorMessage = errorMessage;
            PusherChannel = pusherChannel;
            PusherEvent = pusherEvent;
            WebhookUrl = webhookUrl;
            CreatedAt = createdAt;
        }

        [Preserve]
        [JsonProperty("id")]
        public string Id { get; }

        [Preserve]
        [JsonProperty("file_url")]
        public string FileUrl { get; }

        [Preserve]
        [JsonProperty("skybox_obfuscated_id")]
        public string SkyboxObfuscatedId { get; }

        [Preserve]
        [JsonProperty("type")]
        public string Type { get; }

        [Preserve]
        [JsonProperty("type_id")]
        public int TypeId { get; }

        [Preserve]
        [JsonProperty("status")]
        [JsonConverter(typeof(StringEnumConverter))]
        public Status Status { get; }

        [Preserve]
        [JsonProperty("queue_position")]
        public int QueuePosition { get; }

        [Preserve]
        [JsonProperty("error_message")]
        public string ErrorMessage { get; }

        [Preserve]
        [JsonProperty("pusher_channel")]
        public string PusherChannel { get; }

        [Preserve]
        [JsonProperty("pusher_event")]
        public string PusherEvent { get; }

        [Preserve]
        [JsonProperty("webhook_url")]
        public string WebhookUrl { get; }

        [Preserve]
        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; }

        [Preserve]
        public static implicit operator string(SkyboxExportRequest request) => request.Id;
    }
}

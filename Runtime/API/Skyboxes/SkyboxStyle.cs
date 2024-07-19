using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace BlockadeLabsSDK
{
    [Preserve]
    [Serializable]
    public sealed class SkyboxStyle
    {
        internal SkyboxStyle()
        {
            _name = "All Styles";
            _id = 0;
        }

        [Preserve]
        [JsonConstructor]
        internal SkyboxStyle(
            [JsonProperty("name")] string name,
            [JsonProperty("id")] int id,
            [JsonProperty("type")] string type,
            [JsonProperty("description")] string description,
            [JsonProperty("max-char")] int? maxChar,
            [JsonProperty("negative-text-max-char")] int? negativeTextMaxChar,
            [JsonProperty("image")] string image,
            [JsonProperty("image_jpg")] string imageJpg,
            [JsonProperty("sort_order")] int sortOrder,
            [JsonProperty("premium")] bool premium,
            [JsonProperty("new")] bool isNew,
            [JsonProperty("experimental")] bool experimental,
            [JsonProperty("status")] string status,
            [JsonProperty("model")] SkyboxModel? model,
            [JsonProperty("model_version")] int? modelVersion,
            [JsonProperty("items")] List<SkyboxStyle> familyStyles)
        {
            _name = name;
            _id = id;
            Type = type;
            Description = description;
            MaxChar = maxChar;
            NegativeTextMaxChar = negativeTextMaxChar;
            Image = image;
            ImageJpg = imageJpg;
            SortOrder = sortOrder;
            Premium = premium;
            New = isNew;
            Experimental = experimental;
            Status = status;
            _model = model ?? 0;
            ModelVersion = modelVersion;
            FamilyStyles = familyStyles;
        }

        [SerializeField]
        private string _name;

        [Preserve]
        [JsonProperty("name")]
        public string Name => _name;

        [SerializeField]
        private int _id;

        [Preserve]
        [JsonProperty("id")]
        public int Id => _id;

        [Preserve]
        [JsonProperty("type", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Type { get; }

        [Preserve]
        [JsonProperty("description", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Description { get; }

        [Preserve]
        [JsonProperty("max-char", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int? MaxChar { get; }

        [Preserve]
        [JsonProperty("negative-text-max-char", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int? NegativeTextMaxChar { get; }

        [Preserve]
        [JsonProperty("image", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Image { get; }

        [Preserve]
        [JsonProperty("image_jpg", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string ImageJpg { get; }

        [Preserve]
        [JsonProperty("sort_order")]
        public int SortOrder { get; }

        [Preserve]
        [JsonProperty("premium")]
        public bool Premium { get; }

        [Preserve]
        [JsonProperty("new")]
        public bool New { get; }

        [Preserve]
        [JsonProperty("experimental")]
        public bool Experimental { get; }

        [Preserve]
        [JsonProperty("status", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Status { get; }

        [SerializeField]
        private SkyboxModel _model;

        [Preserve]
        [JsonProperty("model", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public SkyboxModel? Model => _model;

        [Preserve]
        [JsonProperty("model_version", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int? ModelVersion { get; }

        [Preserve]
        [JsonProperty("items", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public IReadOnlyList<SkyboxStyle> FamilyStyles { get; }

        [Preserve]
        public static implicit operator int(SkyboxStyle style) => style?.Id ?? 0;

        [Preserve]
        public static bool operator ==(SkyboxStyle left, SkyboxStyle right) => left?.Id == right?.Id;

        [Preserve]
        public static bool operator !=(SkyboxStyle left, SkyboxStyle right) => !(left == right);

        [Preserve]
        public override bool Equals(object obj)
        {
            if (obj is SkyboxStyle style)
            {
                return Id == style.Id;
            }

            return false;
        }

        [Preserve]
        private bool Equals(SkyboxStyle other)
            => other != null && other.Equals(this);

        [Preserve]
        public override int GetHashCode()
            => Id;

        [Preserve]
        public override string ToString() => JsonConvert.SerializeObject(this, BlockadeLabsClient.JsonSerializationOptions);
    }
}

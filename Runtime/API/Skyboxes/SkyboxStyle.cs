using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine.Scripting;

namespace BlockadeLabsSDK.Skyboxes
{
    [Preserve]
    public sealed class SkyboxStyle
    {
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
            [JsonProperty("skybox_style_families")] List<SkyboxStyleFamily> skyboxStyleFamilies,
            [JsonProperty("items")] List<SkyboxStyle> familyStyles)
        {
            Name = name;
            Id = id;
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
            Model = model;
            ModelVersion = modelVersion;
            SkyboxStyleFamilies = skyboxStyleFamilies;
            FamilyStyles = familyStyles;
        }

        [Preserve]
        [JsonProperty("name")]
        public string Name { get; }

        [Preserve]
        [JsonProperty("id")]
        public int Id { get; }

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

        [Preserve]
        [JsonProperty("model", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public SkyboxModel? Model { get; }

        [Preserve]
        [JsonProperty("model_version", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int? ModelVersion { get; }

        [Preserve]
        [JsonProperty("skybox_style_families", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public IReadOnlyList<SkyboxStyleFamily> SkyboxStyleFamilies { get; }

        [Preserve]
        [JsonProperty("items", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public IReadOnlyList<SkyboxStyle> FamilyStyles { get; }

        public static implicit operator int(SkyboxStyle style) => style.Id;

        public override string ToString() => JsonConvert.SerializeObject(this, BlockadeLabsClient.JsonSerializationOptions);
    }
}

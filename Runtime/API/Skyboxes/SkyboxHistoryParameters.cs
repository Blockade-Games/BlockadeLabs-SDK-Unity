namespace BlockadeLabsSDK
{
    public sealed class SkyboxHistoryParameters
    {
        /// <summary>
        /// Filter by status.<br/>
        /// Options: all, pending, dispatched, processing, complete, abort, error (default: all)
        /// </summary>
        public Status? StatusFilter { get; set; }

        /// <summary>
        /// Number of items to be returned per page (default: 18)
        /// </summary>
        public int? Limit { get; set; }

        /// <summary>
        /// Page number (default: 0)
        /// </summary>
        public int? Offset { get; set; }

        /// <summary>
        /// Sort order. Options: ASC, DESC (default: DESC)
        /// </summary>
        public SortOrder? Order { get; set; }

        /// <summary>
        /// Filter by id
        /// </summary>
        public int? ImagineId { get; set; }

        /// <summary>
        /// Filter by title or prompt
        /// </summary>
        public string QueryFilter { get; set; }

        /// <summary>
        /// Filter by generator
        /// </summary>
        public string GeneratorFilter { get; set; }

        /// <summary>
        /// Filter by favorites only
        /// </summary>
        public bool? FavoritesOnly { get; set; } = null;

        /// <summary>
        /// Filter by API generation type. Options are 'all' or 'web-ui'
        /// </summary>
        public int? GeneratedBy { get; set; } = null;

        /// <summary>
        /// Filter by Skybox Style Id. Defaults to any.
        /// </summary>
        public int? SkyboxStyleId { get; set; } = null;
    }
}

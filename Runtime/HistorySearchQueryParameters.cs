using UnityEngine.Scripting;

namespace BlockadeLabsSDK
{
    [Preserve]
    public class HistorySearchQueryParameters
    {
        /// <summary>
        /// Filter by status.<br/>
        /// Options: all, pending, dispatched, processing, complete, abort, error (default: all)
        /// </summary>
        public string StatusFilter { get; set; } = null;

        /// <summary>
        /// Number of items to be returned per page (default: 18)
        /// </summary>
        public int? Limit { get; set; } = null;

        /// <summary>
        /// Page number (default: 0)
        /// </summary>
        public int? Offset { get; set; } = null;

        /// <summary>
        /// Sort order. Options: ASC, DESC (default: DESC)
        /// </summary>
        public string Order { get; set; } = null;

        /// <summary>
        /// Filter by id
        /// </summary>
        public int? ImagineId { get; set; } = null;

        /// <summary>
        /// Filter by title or prompt
        /// </summary>
        public string QueryFilter { get; set; } = null;

        /// <summary>
        /// Filter by generator
        /// </summary>
        public string GeneratorFilter { get; set; } = null;

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
        public int SkyboxStyleId { get; set; } = 0;
    }
}
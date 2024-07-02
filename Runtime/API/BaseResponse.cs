using Newtonsoft.Json;

namespace BlockadeLabsSDK
{
    public abstract class BaseResponse
    {
        [JsonIgnore]
        public BlockadeLabsClient Client { get; internal set; }

        [JsonIgnore]
        public int RateLimit { get; internal set; }

        [JsonIgnore]
        public int RateLimitRemaining { get; internal set; }
    }
}

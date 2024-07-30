using UnityEngine;

namespace BlockadeLabsSDK
{
    [CreateAssetMenu(fileName = nameof(BlockadeLabsConfiguration), menuName = nameof(BlockadeLabsSDK) + "/" + nameof(BlockadeLabsConfiguration), order = 0)]
    public sealed class BlockadeLabsConfiguration : ScriptableObject
    {
        [SerializeField]
        [Tooltip("The api key.")]
        private string _apiKey;

        public string ApiKey
        {
            get => _apiKey;
            internal set => _apiKey = value;
        }

        [SerializeField]
        [Tooltip("Optional proxy domain to make requests though.")]
        private string _proxyDomainUrl;

        public string ProxyDomainUrl => _proxyDomainUrl;
    }
}

using UnityEngine;

namespace BlockadeLabsSDK
{
    [CreateAssetMenu(fileName = nameof(BlockadeLabsConfiguration), menuName = nameof(BlockadeLabsSDK) + "/" + nameof(BlockadeLabsConfiguration), order = 0)]
    public sealed class BlockadeLabsConfiguration : ScriptableObject
    {
        [SerializeField]
        [Tooltip("The api key.")]
        private string apiKey;

        public string ApiKey
        {
            get => apiKey;
            internal set => apiKey = value;
        }

        [SerializeField]
        [Tooltip("Optional proxy domain to make requests though.")]
        private string proxyDomain;

        public string ProxyDomain => proxyDomain;
    }
}

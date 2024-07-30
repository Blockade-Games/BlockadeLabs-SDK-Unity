using UnityEngine.Networking;

namespace BlockadeLabsSDK.Extensions
{
    internal static class AwaiterExtensions
    {
        public static UnityWebRequestAwaiter GetAwaiter(this UnityWebRequestAsyncOperation asyncOp)
            => new UnityWebRequestAwaiter(asyncOp);

        public static UnityMainThreadAwaiter GetAwaiter(this UnityMainThread _)
            => new UnityMainThreadAwaiter();
    }
}
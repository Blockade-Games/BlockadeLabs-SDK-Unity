using System.Threading;

namespace BlockadeLabsSDK.Extensions
{
    internal static class UnitySynchronizationContext
    {
        public static SynchronizationContext MainThreadContext { get; private set; }

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
#else
        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.AfterAssembliesLoaded)]
#endif
        public static void Initialize()
        {
            MainThreadContext = SynchronizationContext.Current;
        }
    }
}
using System;
using System.Runtime.CompilerServices;

namespace BlockadeLabsSDK.Extensions
{
    internal class UnityMainThreadAwaiter : INotifyCompletion
    {
        public bool IsCompleted => true;

        public void GetResult() { }

        public void OnCompleted(Action continuation)
            => UnitySynchronizationContext.MainThreadContext.Post(_ => continuation(), null);
    }
}
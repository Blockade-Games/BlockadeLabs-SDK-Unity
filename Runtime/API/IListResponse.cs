using System.Collections.Generic;
using UnityEngine.Scripting;

namespace BlockadeLabsSDK
{
    [Preserve]
    public interface IListResponse<out TObject>
        where TObject : BaseResponse
    {
        [Preserve]
        IReadOnlyList<TObject> Items { get; }
    }
}

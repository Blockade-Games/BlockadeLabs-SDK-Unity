using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

namespace BlockadeLabsSDK
{
    internal static class ObjectUtils
    {
        public static void Destroy(Object obj)
        {
            if (Application.isPlaying)
            {
                Object.Destroy(obj);
            }
            else
            {
                Object.DestroyImmediate(obj);
            }
        }

        public static void Destroy(Object[] objects)
        {
            foreach (var obj in objects)
            {
                if (obj)
                {
                    ObjectUtils.Destroy(obj);
                }
            }
        }
    }
}
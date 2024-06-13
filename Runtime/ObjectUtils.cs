using UnityEngine;

namespace BlockadeLabsSDK
{
    internal static class ObjectUtils
    {
        public static void Destroy(this Object obj)
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

        public static void Destroy(this Object[] objects)
        {
            foreach (var obj in objects)
            {
                if (obj)
                {
                    Destroy(obj);
                }
            }
        }
    }
}
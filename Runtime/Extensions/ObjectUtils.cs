using UnityEngine;

namespace BlockadeLabsSDK
{
    internal static class ObjectUtils
    {
        public static void Destroy(this Object obj)
        {
#if UNITY_EDITOR
            // check if Obj is in asset database. Don't destroy it if it is
            if (UnityEditor.AssetDatabase.Contains(obj))
            {
                return;
            }
#endif

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
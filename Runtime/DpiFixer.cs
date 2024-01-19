using System;
using UnityEngine;
using UnityEngine.UI;

namespace BlockadeLabsSDK
{
    [ExecuteAlways]
    internal class DpiFixer : MonoBehaviour
    {
        private CanvasScaler _canvasScaler;

        private void Start()
        {
            _canvasScaler = GetComponent<CanvasScaler>();
            InvokeRepeating(nameof(FixDpi), 0f, 1f);
        }

        private void FixDpi()
        {
            var dpi = Screen.dpi;
            if (dpi < 1)
            {
                dpi = 96;
            }

            var scale = dpi / 96f;
            _canvasScaler.scaleFactor = scale;
        }
    }
}
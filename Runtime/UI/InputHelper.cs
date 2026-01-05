using UnityEngine;

#if UNITY_INPUT_SYSTEM && ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace BlockadeLabsSDK
{
    public static class InputHelper
    {
        private static bool _wasMouseDown;
        private static Vector3 _mousePosition;
        private static bool _mouseDown;
        private static Vector2 _scrollDelta;
        private static int _frame;

        public static Vector2 ScrollDelta
        {
            get
            {
                Update();
                return _scrollDelta;
            }
        }

        public static Vector3 MousePosition
        {
            get
            {
                Update();
                return _mousePosition;
            }
        }

        public static bool GetMouseButton()
        {
            Update();
            return _mouseDown;
        }

        public static bool GetMouseButtonDown()
        {
            Update();
            return _mouseDown && !_wasMouseDown;
        }

        public static bool GetMouseButtonUp()
        {
            Update();
            return !_mouseDown && _wasMouseDown;
        }

        private static void Update()
        {
            if (_frame == Time.frameCount) { return; }

            _frame = Time.frameCount;
            _wasMouseDown = _mouseDown;

            #if UNITY_INPUT_SYSTEM && ENABLE_INPUT_SYSTEM
                _mousePosition = Mouse.current.position.value;
                _mouseDown = Mouse.current.leftButton.isPressed;
                _scrollDelta = Mouse.current.scroll.value;
            #else
                _mousePosition = Input.mousePosition;
                _scrollDelta = Input.mouseScrollDelta;
                _mouseDown = Input.GetMouseButton(0);
            #endif
        }
    }
}
#if UNITY_UI
using UnityEngine;

namespace BlockadeLabsSDK
{
    public class InputModulePicker : MonoBehaviour
    {
        public void Awake()
        {
            var newInputSystem =
#if UNITY_INPUT_SYSTEM && ENABLE_INPUT_SYSTEM
            true;
#else
            false;
#endif


#if UNITY_INPUT_SYSTEM
            var inputSystemModule = GetComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            if (inputSystemModule)
            {
                inputSystemModule.enabled = newInputSystem;
            }
#endif

            var standaloneInputModule = GetComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            if (standaloneInputModule)
            {
                standaloneInputModule.enabled = !newInputSystem;
            }
        }
    }
}

#endif
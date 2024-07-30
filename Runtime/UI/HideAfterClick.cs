using System.Collections;
using UnityEngine;

namespace BlockadeLabsSDK
{
    public class HideAfterClick : MonoBehaviour
    {
        [SerializeField]
        private bool _ignoreClicksInsideRect = true;

        private void OnEnable()
        {
            // listen if mouse button click occurs and then disable this gameObject
            StartCoroutine(CoListenForMouseClick());
        }

        private void OnDisable()
        {
            StopAllCoroutines();
        }

        private IEnumerator CoListenForMouseClick()
        {
            var waitForEndOfFrame = new WaitForEndOfFrame();
            // wait until mouse isn't down so we don't capture the click that enabled this component
            while (Input.GetMouseButton(0) || Input.GetMouseButtonUp(0))
            {
                yield return waitForEndOfFrame;
            }

            while (true)
            {
                if (Input.GetMouseButtonUp(0))
                {
                    // check if pointer is inside rect
                    if (_ignoreClicksInsideRect &&
                        RectTransformUtility.RectangleContainsScreenPoint(GetComponent<RectTransform>(), Input.mousePosition))
                    {
                        // ignore click
                        yield return null;
                        continue;
                    }

                    // wait one frame to make sure to capture the click any mouse clicks on child gameObject
                    yield return waitForEndOfFrame;
                    gameObject.SetActive(false);
                    StopAllCoroutines();
                    yield break;
                }

                yield return null;
            }
        }
    }
}
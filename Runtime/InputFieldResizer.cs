using TMPro;
using UnityEngine;
using System.Collections;

namespace BlockadeLabsSDK
{
    public class InputFieldResizer : MonoBehaviour
    {
        private TMP_InputField _inputField;
        private RectTransform _rectTransform;
        private float _padding;

        private void Awake()
        {
            _inputField = GetComponent<TMP_InputField>();
            _rectTransform = GetComponent<RectTransform>();
            _padding = _rectTransform.rect.height - _inputField.placeholder.GetComponent<TMP_Text>().preferredHeight;

            if (_inputField != null)
            {
                _inputField.onValueChanged.AddListener(_ => StartCoroutine(CoAdjustHeight()));
            }
        }

        private IEnumerator CoAdjustHeight()
        {
            yield return null;
            AdjustHeight();
        }

        private void AdjustHeight()
        {
            var currentHeight = _rectTransform.rect.height;
            var newHeight = _inputField.textComponent.preferredHeight + _padding;
            var heightDifference = newHeight - currentHeight;
            if (heightDifference != 0)
            {
                _rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, newHeight);

                // Since the layout adjusts to the new rect size, the text and caret need to be moved too
                if (heightDifference > 0)
                {
                    _inputField.textComponent.GetComponent<RectTransform>().anchoredPosition -= new Vector2(0, heightDifference);
                    _inputField.textViewport.Find("Caret").GetComponent<RectTransform>().anchoredPosition -= new Vector2(0, heightDifference);
                }
            }
        }
    }
}
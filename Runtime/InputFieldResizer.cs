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
            if (currentHeight != newHeight)
            {
                _rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, newHeight);

                var textPos = _inputField.textComponent.rectTransform.anchoredPosition;
                _inputField.textComponent.rectTransform.offsetMin = Vector2.zero;
                _inputField.textComponent.rectTransform.offsetMax = Vector2.zero;
                var textMovement = _inputField.textComponent.rectTransform.anchoredPosition - textPos;
                _inputField.textViewport.Find("Caret").GetComponent<RectTransform>().anchoredPosition += textMovement;
            }
        }
    }
}
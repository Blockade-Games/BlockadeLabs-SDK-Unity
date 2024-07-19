using TMPro;
using UnityEngine;
using System.Collections;

namespace BlockadeLabsSDK
{
    public class InputFieldResizer : MonoBehaviour
    {
        private float _padding;
        private TMP_InputField _inputField;
        private RectTransform _rectTransform;

        private void Awake()
        {
            _inputField = GetComponent<TMP_InputField>();
            _rectTransform = GetComponent<RectTransform>();
            _padding = _rectTransform.rect.height - GetPreferredTextHeight();
        }

        private float GetPreferredTextHeight()
        {
            var tmpText = _inputField.textComponent.GetComponent<TMP_Text>();
            var textHeight = tmpText.preferredHeight;

            if (textHeight == 0)
            {
                const string A = nameof(A);
                textHeight = tmpText.GetPreferredValues(A).y;
            }

            return textHeight;
        }

        private void OnEnable()
        {
            if (_inputField != null)
            {
                _inputField.onValueChanged.AddListener(OnInputChanged);
            }

            StartCoroutine(CoAdjustHeight());
        }

        private void OnDisable()
        {
            if (_inputField != null)
            {
                _inputField.onValueChanged.RemoveListener(OnInputChanged);
            }
        }

        private void OnInputChanged(string text)
        {
            if (gameObject.activeInHierarchy)
            {
                StartCoroutine(CoAdjustHeight());
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
            var newHeight = GetPreferredTextHeight() + _padding;

            if (currentHeight != newHeight)
            {
                _rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, newHeight);

                var textPos = _inputField.textComponent.rectTransform.anchoredPosition;
                _inputField.textComponent.rectTransform.offsetMin = Vector2.zero;
                _inputField.textComponent.rectTransform.offsetMax = Vector2.zero;
                var textMovement = _inputField.textComponent.rectTransform.anchoredPosition - textPos;
                const string Caret = nameof(Caret);
                _inputField.textViewport.Find(Caret).GetComponent<RectTransform>().anchoredPosition += textMovement;
            }
        }
    }
}
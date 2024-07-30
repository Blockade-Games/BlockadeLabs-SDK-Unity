using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BlockadeLabsSDK
{
    public class DialogBox : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI _headlineText;

        [SerializeField]
        private TextMeshProUGUI _bodyText;

        [SerializeField]
        private Button _confirmButton;

        [SerializeField]
        private Button _cancelButton;

        [SerializeField]
        private GameObject _scrim;

        public void ShowDialog(string headline, string body, Action onConfirm, Action onCancel = null)
        {
            _scrim.SetActive(true);
            gameObject.SetActive(true);
            _headlineText.text = $"<font-weight=\"500\">{headline}</font-weight>";
            _bodyText.text = body;
            _confirmButton.onClick.AddListener(() =>
            {
                onConfirm.Invoke();
                HideDialog();
            });
            _cancelButton.gameObject.SetActive(onCancel != null);

            if (onCancel != null)
            {
                _cancelButton.onClick.AddListener(() =>
                {
                    onCancel.Invoke();
                    HideDialog();
                });
            }
        }

        private void HideDialog()
        {
            _confirmButton.onClick.RemoveAllListeners();
            _cancelButton.onClick.RemoveAllListeners();
            _scrim.SetActive(false);
            gameObject.SetActive(false);
        }
    }
}

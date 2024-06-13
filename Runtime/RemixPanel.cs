using UnityEngine;
using UnityEngine.UI;

namespace BlockadeLabsSDK
{
    public class RemixPanel : MonoBehaviour
    {
        [SerializeField]
        private RuntimeGuiManager _runtimeGuiManager;

        [SerializeField]
        private Button _closeButton;

        [SerializeField]
        private Toggle _viewControlImageToggle;

        private void OnEnable()
        {
            _closeButton.onClick.AddListener(OnCloseButtonClicked);
            _viewControlImageToggle.onValueChanged.AddListener(ToggleViewControlImage);
        }

        private void OnDisable()
        {
            _closeButton.onClick.RemoveListener(OnCloseButtonClicked);
            _viewControlImageToggle.onValueChanged.RemoveListener(ToggleViewControlImage);
        }

        private void OnCloseButtonClicked()
        {
            _runtimeGuiManager.Generator.RemixImage = null;
            gameObject.SetActive(false);
        }

        private void ToggleViewControlImage(bool newValue)
        {
            _runtimeGuiManager.Generator.ViewRemixImage = !newValue;
        }
    }
}

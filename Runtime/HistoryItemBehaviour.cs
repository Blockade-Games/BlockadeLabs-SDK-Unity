using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BlockadeLabsSDK
{
    public class HistoryItemBehaviour : MonoBehaviour
    {
        [SerializeField]
        private RawImage _thumbnailImage;

        [SerializeField]
        private TMP_Text _descriptionText;

        [SerializeField]
        private TMP_Text _timestampText;

        [SerializeField]
        private Toggle _likeToggle;

        [SerializeField]
        private Button _removeButton;

        [SerializeField]
        private Button _downloadButton;

        [SerializeField]
        private MultiToggle _optionsToggle;

        private ImagineResult _imagineResult;
        private RuntimeGuiManager _runtimeGuiManager;

        private void OnEnable()
        {
            _likeToggle.onValueChanged.AddListener(OnLikeToggleValueChanged);
            _removeButton.onClick.AddListener(OnRemoveButtonClicked);
            _downloadButton.onClick.AddListener(OnDownloadButtonClicked);
            _optionsToggle.OnValueChanged.AddListener(OnOptionsToggleValueChanged);
        }

        private void OnDisable()
        {
            _likeToggle.onValueChanged.RemoveListener(OnLikeToggleValueChanged);
            _removeButton.onClick.RemoveListener(OnRemoveButtonClicked);
            _downloadButton.onClick.RemoveListener(OnDownloadButtonClicked);
            _optionsToggle.OnValueChanged.RemoveListener(OnOptionsToggleValueChanged);
        }

        private async void OnLikeToggleValueChanged(bool value)
        {
            if (_imagineResult == null)
            {
                _likeToggle.SetIsOnWithoutNotify(false);
                return;
            }

            var result = await ApiRequests.ToggleFavorite(_imagineResult.id);
            _likeToggle.SetIsOnWithoutNotify(result != null && result.request.isMyFavorite);
        }

        private void OnRemoveButtonClicked()
        {
            Debug.Log($"Remove {_imagineResult.id}");
            // TODO show confirmation dialog
        }

        private void OnDownloadButtonClicked()
        {
            Debug.Log($"Download {_imagineResult.id}");
        }

        private void OnOptionsToggleValueChanged(bool value)
        {
            if (value)
            {
                // listen if mouse button click occurs and turn off the options toggle
                StartCoroutine(ListenForMouseClick());
            }
        }

        private IEnumerator ListenForMouseClick()
        {
            // wait one frame to make sure we don't close the menu
            yield return new WaitForEndOfFrame();

            while (true)
            {
                if (Input.GetMouseButtonUp(0))
                {
                    // wait one frame to make sure to capture the click from remove button
                    yield return new WaitForEndOfFrame();
                    _optionsToggle.IsOn = false;
                    StopAllCoroutines();
                    yield break;
                }

                yield return null;
            }
        }

        private async void RequestThumbnail()
        {
            try
            {
                _thumbnailImage.texture = await ApiRequests.DownloadTextureAsync(_imagineResult.thumb_url);
                gameObject.SetActive(true);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        internal void SetItemData(ImagineResult item, RuntimeGuiManager runtimeGuiManager)
        {
            _imagineResult = item;
            _runtimeGuiManager = runtimeGuiManager;
            _descriptionText.text = $"<b>{item.skybox_style_name}</b> | {item.prompt}";
            _timestampText.text = item.completed_at.ToString("G");
            _likeToggle.SetIsOnWithoutNotify(item.isMyFavorite);
            RequestThumbnail();
        }
    }
}
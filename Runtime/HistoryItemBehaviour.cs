using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
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

        [SerializeField]
        private GameObject _modelBadge;

        private ImagineResult _imagineResult;

#if !UNITY_2022_1_OR_NEWER
        private CancellationTokenSource _destroyCancellationTokenSource;
        // ReSharper disable once InconsistentNaming
        // this is the same name as the unity property introduced in 2022+
        private CancellationToken destroyCancellationToken => _destroyCancellationTokenSource.Token;

        private void Awake()
        {
            _destroyCancellationTokenSource = new CancellationTokenSource();
        }
#endif

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

        private void OnDestroy()
        {
#if !UNITY_2022_1_OR_NEWER
            _destroyCancellationTokenSource.Cancel();
            _destroyCancellationTokenSource.Dispose();
#endif
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
            // TODO show a confirmation dialog
        }

        private void OnDownloadButtonClicked()
        {
            Debug.Log($"Download {_imagineResult.id}");
            // TODO figure how we want to handle loading skybox into scene
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
            var waitForEndOfFrame = new WaitForEndOfFrame();
            // wait one frame to make sure we don't close the menu
            yield return waitForEndOfFrame;

            while (true)
            {
                if (Input.GetMouseButtonUp(0))
                {
                    // wait one frame to make sure to capture the click from remove button
                    yield return waitForEndOfFrame;
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
                _thumbnailImage.texture = await ApiRequests.DownloadTextureAsync(_imagineResult.thumb_url, cancellationToken: destroyCancellationToken);
                gameObject.SetActive(true);
            }
            catch (TaskCanceledException)
            {
                // ignored
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        internal void SetItemData(ImagineResult item)
        {
            _imagineResult = item;
            _descriptionText.text = $"<b>{item.skybox_style_name}</b> | {item.prompt}";
            _timestampText.text = item.completed_at.ToString("G");
            _likeToggle.SetIsOnWithoutNotify(item.isMyFavorite);
            _modelBadge.SetActive(item.model == "Model 3");
            RequestThumbnail();
        }
    }
}
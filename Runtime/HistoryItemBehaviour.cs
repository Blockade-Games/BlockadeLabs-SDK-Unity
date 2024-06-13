using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BlockadeLabsSDK
{
    public class HistoryItemBehaviour : MonoBehaviour
    {
        [SerializeField]
        private Button _button;

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

        [SerializeField]
        private GameObject _apiBadge;

        private ImagineResult _imagineResult;
        private Action<ImagineResult> _clickCallback;
        private Action<ImagineResult> _deleteCallback;
        private Action<ImagineResult> _downloadCallback;

        private static readonly Dictionary<string, Texture2D> _imageCache = new Dictionary<string, Texture2D>();

#if !UNITY_2022_1_OR_NEWER
        private System.Threading.CancellationTokenSource _destroyCancellationTokenSource = new System.Threading.CancellationTokenSource();
        // ReSharper disable once InconsistentNaming
        // this is the same name as the unity property introduced in 2022+
        private System.Threading.CancellationToken destroyCancellationToken => _destroyCancellationTokenSource.Token;
#endif

        private void OnEnable()
        {
            _button.onClick.AddListener(OnClick);
            _likeToggle.onValueChanged.AddListener(OnLikeToggleValueChanged);
            _removeButton.onClick.AddListener(OnRemoveButtonClicked);
            _downloadButton.onClick.AddListener(OnDownloadButtonClicked);
            // disable downloading in runtime builds
            _downloadButton.gameObject.SetActive(Application.isEditor);
            _optionsToggle.OnValueChanged.AddListener(OnOptionsToggleValueChanged);
        }

        private void OnDisable()
        {
            _button.onClick.RemoveListener(OnClick);
            _likeToggle.onValueChanged.RemoveListener(OnLikeToggleValueChanged);
            _removeButton.onClick.RemoveListener(OnRemoveButtonClicked);
            _downloadButton.onClick.RemoveListener(OnDownloadButtonClicked);
            _optionsToggle.OnValueChanged.RemoveListener(OnOptionsToggleValueChanged);
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
#if !UNITY_2022_1_OR_NEWER
            _destroyCancellationTokenSource?.Cancel();
            _destroyCancellationTokenSource?.Dispose();
#endif
        }

        private async void OnLikeToggleValueChanged(bool value)
        {
            var result = await ApiRequests.ToggleFavorite(_imagineResult.id);
            _likeToggle.SetIsOnWithoutNotify(result.isMyFavorite);
        }

        private void OnClick()
            => _clickCallback?.Invoke(_imagineResult);

        private void OnRemoveButtonClicked()
            => _deleteCallback?.Invoke(_imagineResult);

        private void OnDownloadButtonClicked()
            => _downloadCallback?.Invoke(_imagineResult);

        private void OnOptionsToggleValueChanged(bool value)
        {
            if (value)
            {
                // listen if mouse button click occurs and turn off the options toggle
                StartCoroutine(CoListenForMouseClick());
            }
        }

        private IEnumerator CoListenForMouseClick()
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

        internal async void SetItemData(ImagineResult item, Action<ImagineResult> clickCallback, Action<ImagineResult> deleteCallback, Action<ImagineResult> downloadCallback)
        {
            _button.interactable = false;

            try
            {
                _imagineResult = item;
                _clickCallback = clickCallback;
                _deleteCallback = deleteCallback;
                _downloadCallback = downloadCallback;
                _descriptionText.text = $"<b>{item.skybox_style_name}</b> | {item.prompt}";
                _timestampText.text = item.completed_at.ToString("G");
                _likeToggle.SetIsOnWithoutNotify(item.isMyFavorite);
                _modelBadge.SetActive(item.model == "Model 3");
                _apiBadge.SetActive(item.api_key_id != 0);

                if (!_imageCache.TryGetValue(item.obfuscated_id, out var cachedThumbnail))
                {
                    cachedThumbnail = await ApiRequests.DownloadTextureAsync(item.thumb_url, cancellationToken: destroyCancellationToken);
                    _imageCache[item.obfuscated_id] = cachedThumbnail;
                }

                _thumbnailImage.texture = cachedThumbnail;
            }
            catch (Exception e)
            {
                if (e is TaskCanceledException ||
                    e is OperationCanceledException)
                {
                    // ignored
                    return;
                }

                Debug.LogException(e);
            }
            finally
            {
                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                // it is possible that the instance is destroyed when leaving play mode
                if (this != null)
                {
                    gameObject.SetActive(true);
                    _button.interactable = true;
                }
            }
        }
    }
}

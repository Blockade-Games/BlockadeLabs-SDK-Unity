using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace BlockadeLabsSDK
{
    [RequireComponent(typeof(ScrollRect))]
    public class HistoryPanel : MonoBehaviour
    {
        [SerializeField]
        private RuntimeGuiManager _runtimeGuiManager;

        [SerializeField]
        private HistoryItemBehaviour _historyItemPrefab;

        [SerializeField]
        private SearchToolbar _searchToolbar;

        [SerializeField]
        private StylePickerPanel _stylePickerPanel;

        [SerializeField]
        private Transform _historyItemsContainer;

        [SerializeField]
        private ScrollRect _scrollRect;

        private int _pageSize = 9;
        private int _itemsProcessed;
        private bool _isFetchingHistory;
        private SkyboxHistory _lastHistoryResult;
        private SkyboxHistoryParameters _lastQueryParams;

        private readonly Dictionary<int, HistoryItemBehaviour> _historyItems = new Dictionary<int, HistoryItemBehaviour>();

#if !UNITY_2022_1_OR_NEWER
        private System.Threading.CancellationTokenSource _destroyCancellationTokenSource = new System.Threading.CancellationTokenSource();
        // ReSharper disable once InconsistentNaming
        // this is the same name as the unity property introduced in 2022+
        private System.Threading.CancellationToken destroyCancellationToken => _destroyCancellationTokenSource.Token;
#endif

        private void OnValidate()
        {
            if (_scrollRect == null)
            {
                _scrollRect = GetComponent<ScrollRect>();
            }
        }

        private async void OnEnable()
        {
            _scrollRect.onValueChanged.AddListener(OnScrollRectValueChanged);
            _searchToolbar.OnSearchQueryChanged += OnSearchQueryChanged;
            _runtimeGuiManager.Generator.OnStateChanged += OnGeneratorStateChanged;
            OnGeneratorStateChanged(_runtimeGuiManager.Generator.CurrentState);
            await FetchHistoryAsync();
        }

        private void OnDisable()
        {
            _scrollRect.onValueChanged.RemoveListener(OnScrollRectValueChanged);
            _searchToolbar.OnSearchQueryChanged -= OnSearchQueryChanged;
            _runtimeGuiManager.Generator.OnStateChanged -= OnGeneratorStateChanged;
            ClearHistory();
        }

#if !UNITY_2022_1_OR_NEWER
        private void OnDestroy()
        {
            _destroyCancellationTokenSource?.Cancel();
            _destroyCancellationTokenSource?.Dispose();
        }
#endif

        private async void OnScrollRectValueChanged(Vector2 scrollPosition)
        {
            if (_lastHistoryResult is { HasMore: true } &&
                !_isFetchingHistory &&
                scrollPosition.y <= 0)
            {
                _lastQueryParams ??= new SkyboxHistoryParameters();
                _lastQueryParams.Offset = _itemsProcessed / _pageSize;
                await FetchHistoryAsync(_lastQueryParams, clearResults: false);
            }
        }

        private async void OnSearchQueryChanged(SkyboxHistoryParameters searchParameters)
        {
            searchParameters.Offset = 0;
            await FetchHistoryAsync(searchParameters);
        }

        private void OnGeneratorStateChanged(BlockadeLabsSkyboxGenerator.State state)
        {
            if (state == BlockadeLabsSkyboxGenerator.State.Ready)
            {
                _stylePickerPanel.SetStyles(_runtimeGuiManager.Generator.AllModelStyleFamilies);
            }
        }

        private async Task FetchHistoryAsync(SkyboxHistoryParameters searchParameters = null, bool clearResults = true)
        {
            if (_isFetchingHistory) { return; }
            _isFetchingHistory = true;

            try
            {
                if (clearResults)
                {
                    ClearHistory();
                }

                searchParameters ??= _lastQueryParams ??= new SkyboxHistoryParameters();
                searchParameters.Limit = _pageSize;
                _lastHistoryResult = await BlockadeLabsSkyboxGenerator.BlockadeLabsClient.SkyboxEndpoint.GetSkyboxHistoryAsync(_lastQueryParams = searchParameters, destroyCancellationToken);

                foreach (var item in _lastHistoryResult.Items)
                {
                    _itemsProcessed++;
                    if (_historyItems.ContainsKey(item.Id) || item.Status != Status.Complete) { continue; }

                    var historyItemBehaviour = Instantiate(_historyItemPrefab, _historyItemsContainer);
                    historyItemBehaviour.gameObject.SetActive(false);
                    historyItemBehaviour.SetItemData(item, OnHistoryItemClick, OnHistoryItemDelete, OnHistoryItemDownload);
                    _historyItems.Add(item.Id, historyItemBehaviour);
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                _isFetchingHistory = false;
            }
        }

        private void ClearHistory()
        {
            _lastQueryParams = null;

            foreach (var historyItem in _historyItems)
            {
                Destroy(historyItem.Value);
            }

            _historyItems.Clear();
            _itemsProcessed = 0;
        }

        private void OnHistoryItemClick(SkyboxInfo imagineResult)
            => _runtimeGuiManager.PreviewPopup.ShowPreviewPopup(imagineResult);

        private void OnHistoryItemDelete(SkyboxInfo skybox)
            => _runtimeGuiManager.DialogPopup.ShowDialog(
                "Are you absolutely sure?",
                "Are you sure you want to remove your skybox? This is not reversible.",
                onConfirm: async () =>
                {
                    try
                    {
                        var result = await BlockadeLabsSkyboxGenerator.BlockadeLabsClient.SkyboxEndpoint.DeleteSkyboxAsync(skybox, destroyCancellationToken);

                        if (result)
                        {
                            await FetchHistoryAsync();
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                },
                onCancel: () =>
                {
                    // empty callback will close dialog
                });

        private async void OnHistoryItemDownload(SkyboxInfo skybox)
        {
            try
            {
                await _runtimeGuiManager.Generator.DownloadResultAsync(skybox, false, destroyCancellationToken);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}

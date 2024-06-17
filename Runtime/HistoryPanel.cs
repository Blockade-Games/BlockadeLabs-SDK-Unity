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
        private bool _isFetchingHistory;
        private GetHistoryResult _lastHistoryResult;
        private HistorySearchQueryParameters _lastQueryParams;

        private readonly List<HistoryItemBehaviour> _historyItems = new List<HistoryItemBehaviour>();

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

        private async void OnScrollRectValueChanged(Vector2 scrollPosition)
        {
            if (_lastHistoryResult is { has_more: true } &&
                !_isFetchingHistory &&
                scrollPosition.y <= 0)
            {
                _lastQueryParams ??= new HistorySearchQueryParameters();
                _lastQueryParams.Offset = _historyItems.Count / _pageSize;
                await FetchHistoryAsync(_lastQueryParams, clearResults: false);
            }
        }

        private async void OnSearchQueryChanged(HistorySearchQueryParameters searchParameters)
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

        private async Task FetchHistoryAsync(HistorySearchQueryParameters searchParameters = null, bool clearResults = true)
        {
            if (_isFetchingHistory) { return; }
            _isFetchingHistory = true;

            try
            {
                if (clearResults)
                {
                    ClearHistory();
                }

                searchParameters ??= _lastQueryParams ??= new HistorySearchQueryParameters();
                searchParameters.Limit = _pageSize;
                _lastHistoryResult = await ApiRequests.GetSkyboxHistoryAsync(_lastQueryParams = searchParameters);

                foreach (var item in _lastHistoryResult.data)
                {
                    if (item.status != Status.Complete) { continue; }

                    var historyItemBehaviour = Instantiate(_historyItemPrefab, _historyItemsContainer);
                    historyItemBehaviour.gameObject.SetActive(false);
                    historyItemBehaviour.SetItemData(item, OnHistoryItemClick, OnHistoryItemDelete, OnHistoryItemDownload);
                    _historyItems.Add(historyItemBehaviour);
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
                Destroy(historyItem.gameObject);
            }

            _historyItems.Clear();
        }

        private void OnHistoryItemClick(ImagineResult imagineResult)
            => _runtimeGuiManager.PreviewPopup.ShowPreviewPopup(imagineResult);

        private void OnHistoryItemDelete(ImagineResult imagineResult)
            => _runtimeGuiManager.DialogPopup.ShowDialog(
                "Are you absolutely sure?",
                "Are you sure you want to remove your skybox? This is not reversible.",
                onConfirm: async () =>
                {
                    var result = await ApiRequests.DeleteSkyboxAsync(imagineResult.id);

                    if (result)
                    {
                        await FetchHistoryAsync();
                    }
                },
                onCancel: () =>
                {
                    // empty callback will close dialog
                });

        private async void OnHistoryItemDownload(ImagineResult imagineResult)
        {
#if UNITY_EDITOR
            await _runtimeGuiManager.Generator.DownloadResultAsync(imagineResult, false);
#else
            await Task.CompletedTask;
#endif
        }
    }
}

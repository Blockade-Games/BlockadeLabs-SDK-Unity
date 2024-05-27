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
        private HistoryItemBehaviour _historyItemPrefab;

        [SerializeField]
        private SearchToolbar _searchToolbar;

        [SerializeField]
        private Transform _historyItemsContainer;

        [SerializeField]
        private ScrollRect _scrollRect;

        private bool _isFetchingHistory;
        private GetHistoryResult _lastHistoryResult;

        private readonly List<HistoryItemBehaviour> _historyItems = new();

        public RuntimeGuiManager RuntimeGuiManager { get; set; }

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
            await FetchHistoryAsync();
        }

        private void OnDisable()
        {
            _scrollRect.onValueChanged.RemoveListener(OnScrollRectValueChanged);
            _searchToolbar.OnSearchQueryChanged -= OnSearchQueryChanged;
            ClearHistory();
        }

        private async void OnScrollRectValueChanged(Vector2 scrollPosition)
        {
            if (_lastHistoryResult != null &&
                _lastHistoryResult.has_more &&
                !_isFetchingHistory &&
                scrollPosition.y <= 0)
            {
                Debug.Log("load more items...");
                await FetchHistoryAsync(new HistorySearchQueryParameters
                {
                    Offset = _historyItems.Count
                }, clearResults: false);
            }
        }

        private async void OnSearchQueryChanged(HistorySearchQueryParameters searchParameters)
            => await FetchHistoryAsync(searchParameters);

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

                _lastHistoryResult = await ApiRequests.GetSkyboxHistoryAsync(RuntimeGuiManager.Generator.ApiKey, searchParameters);

                foreach (var item in _lastHistoryResult.data)
                {
                    var historyItemBehaviour = Instantiate(_historyItemPrefab, _historyItemsContainer);
                    historyItemBehaviour.SetItemData(item);
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
            foreach (var historyItem in _historyItems)
            {
                Destroy(historyItem.gameObject);
            }

            _historyItems.Clear();
        }
    }
}

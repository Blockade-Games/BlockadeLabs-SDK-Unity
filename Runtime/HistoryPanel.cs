using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace BlockadeLabsSDK
{
    public class HistoryPanel : MonoBehaviour
    {
        [SerializeField]
        private HistoryItemBehaviour _historyItemPrefab;

        [SerializeField]
        private SearchToolbar _searchToolbar;

        [SerializeField]
        private Transform _historyItemsContainer;

        private readonly List<HistoryItemBehaviour> _historyItems = new();

        public RuntimeGuiManager RuntimeGuiManager { get; set; }

        private async void OnEnable()
        {
            _searchToolbar.OnSearchQueryChanged += OnSearchQueryChanged;
            await FetchHistoryAsync();
        }

        private void OnDisable()
        {
            _searchToolbar.OnSearchQueryChanged -= OnSearchQueryChanged;
            ClearHistory();
        }

        private async void OnSearchQueryChanged(HistorySearchQueryParameters searchParameters)
        {
            await FetchHistoryAsync(searchParameters);
        }

        private async Task FetchHistoryAsync(HistorySearchQueryParameters searchParameters = null)
        {
            ClearHistory();
            GetHistoryResult historyItems;

            try
            {
                historyItems = await ApiRequests.GetSkyboxHistoryAsync(RuntimeGuiManager.Generator.ApiKey, searchParameters);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return;
            }

            foreach (var item in historyItems.data)
            {
                var historyItemBehaviour = Instantiate(_historyItemPrefab, _historyItemsContainer);
                historyItemBehaviour.SetItemData(item, RuntimeGuiManager);
                _historyItems.Add(historyItemBehaviour);
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
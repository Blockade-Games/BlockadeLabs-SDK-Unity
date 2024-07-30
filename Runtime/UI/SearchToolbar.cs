using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BlockadeLabsSDK
{
    public class SearchToolbar : MonoBehaviour
    {
        [SerializeField]
        private TMP_InputField _searchInputField;

        [SerializeField]
        private Toggle _onlyLikesToggle;

        [SerializeField]
        private TMP_Dropdown _generatedByDropdown;

        [SerializeField]
        private TMP_Text _stylePickerButtonText;

        [SerializeField]
        private StylePickerPanel _stylePickerPanel;

        [SerializeField]
        private TMP_Dropdown _sortDropdown;

        private SkyboxHistoryParameters _searchQueryParameters = new SkyboxHistoryParameters();

        public event Action<SkyboxHistoryParameters> OnSearchQueryChanged;

        private void Start()
        {
            _generatedByDropdown.options = new List<TMP_Dropdown.OptionData>
            {
                new TMP_Dropdown.OptionData("All"),
                new TMP_Dropdown.OptionData("Web UI")
            };
            _sortDropdown.options = new List<TMP_Dropdown.OptionData>
            {
                new TMP_Dropdown.OptionData("Newest First"),
                new TMP_Dropdown.OptionData("Oldest First")
            };
        }

        private void OnEnable()
        {
            _searchInputField.onValueChanged.AddListener(OnSearchInputValueChanged);
            _onlyLikesToggle.onValueChanged.AddListener(OnOnlyLikesToggleValueChanged);
            _generatedByDropdown.onValueChanged.AddListener(OnGeneratedByDropdownValueChanged);
            _stylePickerPanel.OnStylePicked += OnStylePicked;
        }

        private void OnDisable()
        {
            _searchInputField.onValueChanged.RemoveListener(OnSearchInputValueChanged);
            _onlyLikesToggle.onValueChanged.RemoveListener(OnOnlyLikesToggleValueChanged);
            _generatedByDropdown.onValueChanged.RemoveListener(OnGeneratedByDropdownValueChanged);
            _stylePickerPanel.OnStylePicked -= OnStylePicked;
        }

        private void OnSearchInputValueChanged(string value)
        {
            _searchQueryParameters.QueryFilter = value;
            OnSearchQueryChanged?.Invoke(_searchQueryParameters);
        }

        private void OnOnlyLikesToggleValueChanged(bool value)
        {
            _searchQueryParameters.FavoritesOnly = value;
            OnSearchQueryChanged?.Invoke(_searchQueryParameters);
        }

        private void OnGeneratedByDropdownValueChanged(int value)
        {
            _searchQueryParameters.GeneratedBy = value switch
            {
                1 => 0,
                _ => null
            };
            OnSearchQueryChanged?.Invoke(_searchQueryParameters);
        }

        private void OnStylePicked(SkyboxStyle style)
        {
            if (style != null)
            {
                _stylePickerButtonText.text = style.Name;
                _searchQueryParameters.SkyboxStyleId = style.Id;
            }
            else
            {
                _stylePickerButtonText.text = "All Styles";
                _searchQueryParameters.SkyboxStyleId = null;
            }

            OnSearchQueryChanged?.Invoke(_searchQueryParameters);
        }

        private void OnSortDropdownValueChanged(int value)
        {
            _searchQueryParameters.Order = value switch
            {
                0 => SortOrder.Desc,
                _ => SortOrder.Asc
            };
            OnSearchQueryChanged?.Invoke(_searchQueryParameters);
        }
    }
}

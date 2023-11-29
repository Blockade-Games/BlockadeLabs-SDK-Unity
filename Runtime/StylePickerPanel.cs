using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BlockadeLabsSDK
{
    public class StylePickerPanel : MonoBehaviour
    {
        [SerializeField]
        private StyleItem _styleItemPrefab;

        [SerializeField]
        private GameObject _styleFamilyContainerRoot;

        [SerializeField]
        private Transform _styleFamilyContainer;

        [SerializeField]
        private GameObject _styleContainerRoot;

        [SerializeField]
        private Transform _styleContainer;

        [SerializeField]
        private Button _backButton;

        [SerializeField]
        private Button _dismissButton;

        public event Action<SkyboxStyle> OnStyleSelected;

        public void SetStyles(IReadOnlyList<SkyboxStyleFamily> styles)
        {
            foreach (Transform child in _styleFamilyContainer)
            {
                Destroy(child.gameObject);
            }

            foreach (var styleFamily in styles)
            {
                var styleFamilyItem = Instantiate(_styleItemPrefab, _styleFamilyContainer);
                styleFamilyItem.SetStyleFamily(styleFamily);
                styleFamilyItem.Button.onClick.AddListener(() => SelectStyleFamily(styleFamily));
            }
        }

        private void OnEnable()
        {
            ShowStyleFamilies();
            _dismissButton.gameObject.SetActive(true);
        }

        private void OnDisable()
        {
            _dismissButton.gameObject.SetActive(false);
        }

        private void Start()
        {
            _backButton.onClick.AddListener(ShowStyleFamilies);
            _dismissButton.onClick.AddListener(() => gameObject.SetActive(false));
        }

        private void SelectStyleFamily(SkyboxStyleFamily styleFamily)
        {
            _styleFamilyContainerRoot.gameObject.SetActive(false);
            _styleContainerRoot.gameObject.SetActive(true);

            foreach (Transform child in _styleContainer)
            {
                if (child != _backButton.transform)
                {
                    Destroy(child.gameObject);
                }
            }

            _backButton.GetComponentInChildren<TMP_Text>().text = styleFamily.name;

            if (styleFamily.items.Count > 0) // TODO: How to do Advanced?
            {
                foreach (var style in styleFamily.items)
                {
                    var styleItem = Instantiate(_styleItemPrefab, _styleContainer);
                    styleItem.SetStyle(style);
                    styleItem.Button.onClick.AddListener(() => SelectStyle(style));
                }
            }
        }

        private void ShowStyleFamilies()
        {
            _styleFamilyContainerRoot.gameObject.SetActive(true);
            _styleContainerRoot.gameObject.SetActive(false);
        }

        private void SelectStyle(SkyboxStyle style)
        {
            OnStyleSelected?.Invoke(style);
            gameObject.SetActive(false);
        }
    }
}
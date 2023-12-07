using System;
using System.Collections;
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

        [SerializeField]
        private GameObject _previewPanelRoot;

        [SerializeField]
        private Image _previewImage;

        [SerializeField]
        private TMP_Text _previewText;

        public event Action<SkyboxStyle> OnStylePicked;

        private SkyboxStyle _selectedStyle;
        private SkyboxStyle _previewStyle;

        private Dictionary<string, Sprite> _previewCache = new Dictionary<string, Sprite>();

        private void Awake()
        {
            var backHoverable = _backButton.GetComponent<Hoverable>();
            var backText = backHoverable.GetComponentInChildren<TMP_Text>();
            var backTextColor = backText.color;
            backHoverable.OnHover.AddListener(() => backText.color = Color.white);
            backHoverable.OnUnhover.AddListener(() => backText.color = backTextColor);
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

        public void SetStyles(IReadOnlyList<SkyboxStyleFamily> styleFamilies)
        {
            foreach (Transform child in _styleFamilyContainer)
            {
                Destroy(child.gameObject);
            }

            foreach (var styleFamily in styleFamilies)
            {
                var styleFamilyItem = Instantiate(_styleItemPrefab, _styleFamilyContainer);
                styleFamilyItem.SetStyleFamily(styleFamily);

                if (styleFamily.items.Count == 1)
                {
                    styleFamilyItem.Button.onClick.AddListener(() => SelectStyle(styleFamily.items[0]));
                }
                else
                {
                    styleFamilyItem.Button.onClick.AddListener(() => PickStyleFamily(styleFamily));
                }
            }
        }

        private void PickStyleFamily(SkyboxStyleFamily styleFamily)
        {
            _styleFamilyContainerRoot.gameObject.SetActive(false);
            _styleContainerRoot.gameObject.SetActive(true);

            foreach (Transform child in _styleContainer)
            {
                if (child.GetComponent<StyleItem>() != null)
                {
                    Destroy(child.gameObject);
                }
            }

            _backButton.GetComponentInChildren<TMP_Text>().text = styleFamily.name;

            foreach (var style in styleFamily.items)
            {
                var styleItem = Instantiate(_styleItemPrefab, _styleContainer);
                styleItem.SetStyle(style);
                styleItem.SetSelected(style == _selectedStyle);
                styleItem.Button.onClick.AddListener(() => SelectStyle(style));
                styleItem.Hoverable.OnHover.AddListener(() => ShowPreviewAsync(style));
            }
        }

        public void SetSelectedStyle(SkyboxStyleFamily styleFamily, SkyboxStyle style)
        {
            foreach (Transform child in _styleFamilyContainer)
            {
                var styleFamilyItem = child.GetComponent<StyleItem>();
                styleFamilyItem.SetSelected(styleFamilyItem.Style == styleFamily);
            }

            _selectedStyle = style;
        }

        private void ShowStyleFamilies()
        {
            _styleFamilyContainerRoot.gameObject.SetActive(true);
            _styleContainerRoot.gameObject.SetActive(false);
            _previewPanelRoot.SetActive(false);
        }

        private void SelectStyle(SkyboxStyle style)
        {
            OnStylePicked?.Invoke(style);
            gameObject.SetActive(false);
        }

        private async void ShowPreviewAsync(SkyboxStyle style)
        {
            _previewPanelRoot.SetActive(true);
            _previewText.text = style.description;
            _previewStyle = style;

            if (_previewCache.TryGetValue(style.image_jpg, out var sprite))
            {
                _previewImage.sprite = sprite;
                return;
            }

            // Ensure we only make one download request.
            _previewCache.Add(style.image_jpg, null);

            // Download the image
            var texture = await ApiRequests.DownloadTextureAsync(style.image_jpg);
            sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
            _previewCache[style.image_jpg] = sprite;

            // If we're still showing the style preview, update the preview image.
            if (style == _previewStyle)
            {
                _previewImage.sprite = sprite;
            }
        }
    }
}
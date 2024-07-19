using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        private bool _showAllStylesOption;

        [SerializeField]
        private bool _showPreview;

        [SerializeField]
        private Image _previewImage;

        [SerializeField]
        private TMP_Text _previewText;

        public event Action<SkyboxStyle> OnStylePicked;

        private SkyboxStyle _selectedStyle;
        private SkyboxStyle _selectedFamily;
        private SkyboxStyle _previewStyle;
        private IReadOnlyList<SkyboxStyle> _currentStyleFamily;

        private Dictionary<string, Sprite> _previewCache = new Dictionary<string, Sprite>();

#if !UNITY_2022_1_OR_NEWER
        private System.Threading.CancellationTokenSource _destroyCancellationTokenSource = new System.Threading.CancellationTokenSource();
        // ReSharper disable once InconsistentNaming
        // this is the same name as the unity property introduced in 2022+
        private System.Threading.CancellationToken destroyCancellationToken => _destroyCancellationTokenSource.Token;
#endif

        private void Awake()
        {
            var backHoverable = _backButton.GetComponent<Hoverable>();
            var backText = backHoverable.GetComponentInChildren<TMP_Text>();
            var backTextColor = backText.color;
            backHoverable.OnHover.AddListener(() => backText.color = Color.white);
            backHoverable.OnUnhover.AddListener(() => backText.color = backTextColor);
        }

        private void Start()
        {
            _backButton.onClick.AddListener(ShowStyleFamilies);
            _dismissButton.onClick.AddListener(() => gameObject.SetActive(false));
        }

        private void OnEnable()
        {
            ShowStyleFamilies();
            _dismissButton.gameObject.SetActive(true);
            _previewPanelRoot.SetActive(false);
        }

        private void OnDisable()
        {
            _dismissButton.gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
#if !UNITY_2022_1_OR_NEWER
            _destroyCancellationTokenSource?.Cancel();
            _destroyCancellationTokenSource?.Dispose();
#endif
        }

        public void SetStyles(IReadOnlyList<SkyboxStyle> styleFamilies)
        {
            if (Equals(styleFamilies, _currentStyleFamily)) { return; }

            _currentStyleFamily = styleFamilies;

            foreach (Transform child in _styleFamilyContainer)
            {
                Destroy(child.gameObject);
            }

            if (_showAllStylesOption)
            {
                var allStylesFamilyItem = Instantiate(_styleItemPrefab, _styleFamilyContainer);
                allStylesFamilyItem.SetStyle(new SkyboxStyle());
                allStylesFamilyItem.SetSelected(_selectedStyle == null && _selectedFamily == null);
                allStylesFamilyItem.Button.onClick.AddListener(() =>
                {
                    _selectedFamily = null;
                    SelectStyle(null);
                });
            }

            if (_currentStyleFamily == null) { return; }

            foreach (var style in styleFamilies)
            {
                var styleItem = Instantiate(_styleItemPrefab, _styleFamilyContainer);
                styleItem.SetStyle(style);

                if (style.FamilyStyles != null)
                {
                    styleItem.SetSelected(style == _selectedFamily);
                    styleItem.Button.onClick.AddListener(() => PickStyleFamily(style));
                    styleItem.Hoverable.OnHover.AddListener(() => ShowPreview(null));
                }
                else
                {
                    styleItem.SetSelected(style == _selectedStyle);
                    styleItem.Button.onClick.AddListener(() =>
                    {
                        _selectedFamily = null;
                        SelectStyle(style);
                    });
                    styleItem.Hoverable.OnHover.AddListener(() => ShowPreview(style));
                }
            }
        }

        private void PickStyleFamily(SkyboxStyle styleFamily)
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

            _backButton.GetComponentInChildren<TMP_Text>().text = styleFamily.Name;

            if (styleFamily.FamilyStyles != null)
            {
                foreach (var style in styleFamily.FamilyStyles)
                {
                    var styleItem = Instantiate(_styleItemPrefab, _styleContainer);
                    styleItem.SetStyle(style);
                    styleItem.SetSelected(style == _selectedStyle);
                    styleItem.Button.onClick.AddListener(() =>
                    {
                        _selectedFamily = styleFamily;
                        SelectStyle(style);
                    });
                    styleItem.Hoverable.OnHover.AddListener(() => ShowPreview(style));
                }
            }
        }

        public void SetSelectedStyle(SkyboxStyle style)
        {
            if (style?.Id == 0) { style = null; }
            _selectedStyle = style;
            UpdateSelectionVisual();
        }

        private void UpdateSelectionVisual()
        {
            foreach (Transform child in _styleFamilyContainer)
            {
                if (child.TryGetComponent<StyleItem>(out var styleItem))
                {
                    if (styleItem.Style == null) { continue; }
                    if (styleItem.Style.FamilyStyles != null)
                    {
                        styleItem.SetSelected(styleItem.Style == _selectedFamily || styleItem.Style.FamilyStyles.Any(style => style == _selectedStyle));
                    }
                    else
                    {
                        if (_showAllStylesOption && styleItem.Style.Id == 0)
                        {
                            styleItem.SetSelected(_selectedStyle == null && _selectedFamily == null);
                        }
                        else
                        {
                            styleItem.SetSelected(styleItem.Style == _selectedStyle);
                        }
                    }
                }
            }

            foreach (Transform child in _styleContainer)
            {
                if (child.TryGetComponent<StyleItem>(out var styleItem))
                {
                    styleItem.SetSelected(styleItem.Style == _selectedStyle);
                }
            }
        }

        private void ShowStyleFamilies()
        {
            UpdateSelectionVisual();
            _styleFamilyContainerRoot.gameObject.SetActive(true);
            _styleContainerRoot.gameObject.SetActive(false);
        }

        private void SelectStyle(SkyboxStyle style)
        {
            _selectedStyle = style;
            OnStylePicked?.Invoke(style);
            gameObject.SetActive(false);
        }

        private async void ShowPreview(SkyboxStyle style)
        {
            if (!_showPreview)
            {
                return;
            }

            _previewPanelRoot.SetActive(false);
            _previewStyle = style;

            if (style == null || string.IsNullOrEmpty(style.ImageJpg))
            {
                return;
            }

            _previewText.text = style.Description;

            if (_previewCache.TryGetValue(style.ImageJpg, out var sprite))
            {
                _previewImage.sprite = sprite;
                _previewPanelRoot.SetActive(true);
                return;
            }

            // Ensure we only make one download request.
            _previewCache.Add(style.ImageJpg, null);

            Texture2D texture = null;

            // Download the image
            try
            {
                texture = await Rest.DownloadTextureAsync(style.ImageJpg, cancellationToken: destroyCancellationToken);
            }
            catch (Exception e)
            {
                switch (e)
                {
                    case TaskCanceledException _:
                    case OperationCanceledException _:
                        // ignored
                        return;
                    default:
                        Debug.LogException(e);
                        break;
                }
            }

            if (texture == null)
            {
                return;
            }

            sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
            _previewCache[style.ImageJpg] = sprite;

            // If we're still showing the style preview, update the preview image.
            if (style == _previewStyle)
            {
                _previewImage.sprite = sprite;
                _previewPanelRoot.SetActive(true);
            }
        }
    }
}
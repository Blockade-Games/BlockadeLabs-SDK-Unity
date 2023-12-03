using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
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

        private void OnEnable()
        {
            ShowStyleFamilies();
            _dismissButton.gameObject.SetActive(true);
        }

        private void OnDisable()
        {
            StopAllCoroutines();
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
                if (child != _backButton.transform)
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
                styleItem.Hoverable.OnHover.AddListener(() => ShowPreview(style));
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

        private void ShowPreview(SkyboxStyle style)
        {
            StopAllCoroutines();

            // Doesn't currently work because Unity doesn't support webp files.
            // StartCoroutine(DownloadImage(style.image, sprite =>
            // {
            //     _previewPanelRoot.SetActive(true);
            //     _previewText.text = style.description;
            //     _previewImage.sprite = sprite;
            // }));

            _previewPanelRoot.SetActive(true);
            _previewText.text = style.description;
        }

        private Dictionary<string, Sprite> _imageCache = new Dictionary<string, Sprite>();

        private IEnumerator DownloadImage(string url, Action<Sprite> callback)
        {
            if (_imageCache.TryGetValue(url, out var sprite))
            {
                callback(sprite);
                yield break;
            }

            var req = UnityWebRequest.Get(url);
            req.downloadHandler = new DownloadHandlerTexture();
            yield return req.SendWebRequest();
            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.Log("Download error: " + req.downloadHandler.error);
                yield break;
            }

            var texture = ((DownloadHandlerTexture)req.downloadHandler).texture;
            sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
            _imageCache.Add(url, sprite);
            callback(sprite);
        }
    }
}
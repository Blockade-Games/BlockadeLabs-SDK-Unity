using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BlockadeLabsSDK
{
    public class StyleItem : MonoBehaviour
    {
        [SerializeField]
        private GameObject _newSticker;

        [SerializeField]
        private GameObject _premiumSticker;

        [SerializeField]
        private GameObject _experimentalSticker;

        [SerializeField]
        private TMP_Text _nameText;

        [SerializeField]
        private Button _button;
        public Button Button => _button;

        [SerializeField]
        private Hoverable _hoverable;
        public Hoverable Hoverable => _hoverable;

        [SerializeField]
        private GameObject _nextChevron;

        [SerializeField]
        private GameObject _selectedIndicator;

        private SkyboxStyle _style;
        public SkyboxStyle Style => _style;

        private void Awake()
        {
            var fontColor = _nameText.color;
            var hoverable = GetComponent<Hoverable>();
            hoverable.OnHover.AddListener(() => _nameText.color = Color.white);
            hoverable.OnUnhover.AddListener(() => _nameText.color = fontColor);
        }

        public void SetStyle(SkyboxStyle style)
        {
            _nameText.text = style.name;
            _newSticker.SetActive(style.isNew);
            _premiumSticker.SetActive(style.premium);
            _experimentalSticker.SetActive(style.experimental);
            _nextChevron.SetActive(false);
            _selectedIndicator.SetActive(false);
            _style = style;
        }

        public void SetStyleFamily(SkyboxStyleFamily styleFamily)
        {
            _nameText.text = styleFamily.name;
            _newSticker.SetActive(styleFamily.isNew);
            _premiumSticker.SetActive(styleFamily.premium);
            _experimentalSticker.SetActive(styleFamily.experimental);
            _nextChevron.SetActive(styleFamily.items.Count > 1);
            _selectedIndicator.SetActive(false);
            _style = styleFamily;
        }

        public void SetSelected(bool selected)
        {
            _selectedIndicator.SetActive(selected);
        }
    }
}
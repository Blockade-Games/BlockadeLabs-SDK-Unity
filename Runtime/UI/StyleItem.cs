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
        private GameObject _model3Sticker;

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
            _nameText.text = style.Name;
            _newSticker.SetActive(style.New);
            _premiumSticker.SetActive(style.Premium);
            _experimentalSticker.SetActive(style.Experimental);
            _model3Sticker.SetActive(style.Model == SkyboxModel.Model3);
            _nextChevron.SetActive(style.FamilyStyles != null);
            _selectedIndicator.SetActive(false);
            _style = style;
        }

        public void SetSelected(bool selected)
        {
            _selectedIndicator.SetActive(selected);
        }
    }
}
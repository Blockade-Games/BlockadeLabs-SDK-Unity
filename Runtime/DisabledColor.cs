using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BlockadeLabsSDK
{
    public class DisabledColor : MonoBehaviour
    {
        [SerializeField]
        private Color _color = Color.gray;
        public Color Color
        {
            get { return _color; }
            set
            {
                _color = value;
                UpdateColor();
            }
        }

        [SerializeField]
        private bool _disabled;
        public bool Disabled
        {
            get { return _disabled; }
            set
            {
                _disabled = value;
                UpdateColor();
            }
        }

        private TMP_Text _text;
        private Image _image;
        private Color _enabledColor;

        private void Awake()
        {
            _text = GetComponent<TMP_Text>();
            _image = GetComponent<Image>();
            _enabledColor = _text?.color ?? _image?.color ?? Color.white;
            UpdateColor();
        }

        private void UpdateColor()
        {
            if (_text != null)
            {
                _text.color = _disabled ? _color : _enabledColor;
            }

            if (_image != null)
            {
                _image.color = _disabled ? _color : _enabledColor;
            }
        }
    }
}
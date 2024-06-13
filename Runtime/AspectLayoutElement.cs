using UnityEngine;
using UnityEngine.UI;

namespace BlockadeLabsSDK
{
    public class AspectLayoutElement : MonoBehaviour, ILayoutElement
    {
        [SerializeField]
        private float _minWidth = -1;
        public float minWidth => _minWidth;

        [SerializeField]
        private float _preferredWidth = -1;
        public float preferredWidth => _preferredWidth;

        [SerializeField]
        private float _flexibleWidth = -1;
        public float flexibleWidth => _flexibleWidth;

        [SerializeField]
        private float _heightToWidth = 1;
        public float heightToWidth
        {
            get => _heightToWidth;
            set => _heightToWidth = value;
        }

        [SerializeField]
        private int _layoutPriority = 1;
        public int layoutPriority => _layoutPriority;

        public float minHeight => _minWidth * _heightToWidth;
        public float preferredHeight => GetComponent<RectTransform>().rect.width * _heightToWidth;
        public float flexibleHeight => 0;

        public void CalculateLayoutInputHorizontal() { }
        public void CalculateLayoutInputVertical() { }
    }
}
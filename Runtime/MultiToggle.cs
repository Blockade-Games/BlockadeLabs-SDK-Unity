using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BlockadeLabsSDK
{
    public class MultiToggle : Selectable, IPointerClickHandler
    {
        public List<GameObject> OnObjects;
        public List<GameObject> OffObjects;
        public UnityEvent<bool> OnValueChanged;

        [SerializeField]
        private bool _isOn;
        public bool IsOn
        {
            get { return _isOn; }
            set
            {
                if (_isOn != value)
                {
                    _isOn = value;
                    UpdateVisualState();
                    OnValueChanged.Invoke(_isOn);
                }
            }
        }

        protected override void Awake()
        {
            base.Awake();
            UpdateVisualState();
        }

        private void Toggle()
        {
            IsOn = !IsOn;
        }

        private void UpdateVisualState()
        {
            foreach (var onComponent in OnObjects)
            {
                onComponent.SetActive(IsOn);
            }

            foreach (var offComponent in OffObjects)
            {
                offComponent.SetActive(!IsOn);
            }
        }

        public virtual void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left || !interactable)
            {
                return;
            }

            Toggle();
        }
    }
}
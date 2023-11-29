using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace BlockadeLabsSDK
{
    public class Hoverable : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public bool IsHovered { get; private set; }
        public UnityEvent<bool> OnHoverChanged;

        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            IsHovered = true;
            OnHoverChanged.Invoke(IsHovered);
        }

        public virtual void OnPointerExit(PointerEventData eventData)
        {
            IsHovered = false;
            OnHoverChanged.Invoke(IsHovered);
        }
    }
}
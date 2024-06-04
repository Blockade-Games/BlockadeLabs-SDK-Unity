using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BlockadeLabsSDK
{
    public class PopupButton : Selectable, IPointerClickHandler
    {
        public GameObject Popup;

        public void TogglePopup()
        {
            Popup.SetActive(!Popup.activeSelf);
        }

        public virtual void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }

            TogglePopup();
        }
    }
}
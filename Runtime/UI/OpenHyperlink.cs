#if UNITY_TMPRO

using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

namespace BlockadeLabsSDK
{
    [RequireComponent(typeof(TMP_Text))]
    public class OpenHyperlink : MonoBehaviour, IPointerClickHandler
    {
        private TMP_Text textMeshProText;

        void Start()
        {
            textMeshProText = GetComponent<TMP_Text>();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            int linkIndex = TMP_TextUtilities.FindIntersectingLink(textMeshProText, InputHelper.MousePosition, null);
            if (linkIndex != -1)
            {
                TMP_LinkInfo linkInfo = textMeshProText.textInfo.linkInfo[linkIndex];
                Application.OpenURL(linkInfo.GetLinkID());
            }
        }
    }
}

#endif
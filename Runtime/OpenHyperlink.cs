using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

namespace BlockadeLabsSDK
{
    [RequireComponent(typeof(TMP_Text))]
    public class OpenHyperlink : MonoBehaviour, IPointerClickHandler
    {
        private TMP_Text m_textMeshPro;

        void Start()
        {
            m_textMeshPro = GetComponent<TMP_Text>();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            int linkIndex = TMP_TextUtilities.FindIntersectingLink(m_textMeshPro, Input.mousePosition, null);
            if (linkIndex != -1)
            {
                TMP_LinkInfo linkInfo = m_textMeshPro.textInfo.linkInfo[linkIndex];
                Application.OpenURL(linkInfo.GetLinkID());
            }
        }
    }
}
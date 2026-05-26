using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LayerLab.CasualGame
{
    public class PanelCasualGame : MonoBehaviour
    {
        [SerializeField] private GameObject[] otherPanels;

        public void OnEnable()
        {
            for (int i = 0; i < otherPanels.Length; i++) otherPanels[i].SetActive(true);
            //SafeArea();
        }

        public void OnDisable()
        {
            for (int i = 0; i < otherPanels.Length; i++) otherPanels[i].SetActive(false);
        }

        private void SafeArea()
        {
            var safeArea = Screen.safeArea;
            Vector2 CustomAnchor = safeArea.position;
            CustomAnchor.y = CustomAnchor.y / 2;
            var anchorMin = CustomAnchor;
            var anchorMax = safeArea.position + safeArea.size;

            float gabLeft = anchorMin.x / Screen.width;
            float gabRight = 1 - (anchorMax.x / Screen.width);
            float gab = (gabLeft > gabRight ? gabLeft : gabRight);
            if (gab > 0.045f) gab = 0.045f;

            anchorMin.x = gab;
            anchorMax.x = 1 - gab;
            anchorMin.y = 0;
            anchorMax.y = 1;

            RectTransform trans = gameObject.GetComponent<RectTransform>();
            trans.anchorMin = anchorMin;
            trans.anchorMax = anchorMax;
        }
    }
}

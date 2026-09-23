using UnityEngine;
namespace BaseBall.BallPlay.UGUI
{
    public sealed class GameUIGrid : MonoBehaviour
    {
        public int arrangement, maxPerLine;
        public float cellWidth, cellHeight;
        public bool hideInactive;
        public Transform content;
        private void OnEnable() { Reposition(); }
        public void Reposition()
        {
            var root = content != null ? content : transform;
            int index = 0;
            foreach (Transform child in root)
            {
                if (hideInactive && !child.gameObject.activeSelf) continue;
                int column = maxPerLine > 0 ? index % maxPerLine : index;
                int row = maxPerLine > 0 ? index / maxPerLine : 0;
                child.localPosition = arrangement == 0 ? new Vector3(column * cellWidth, -row * cellHeight, child.localPosition.z)
                    : new Vector3(row * cellWidth, -column * cellHeight, child.localPosition.z);
                index++;
            }
        }
    }
}

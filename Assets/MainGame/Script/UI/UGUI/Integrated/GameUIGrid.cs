using UnityEngine;
using System.Linq;
namespace BaseBall.BallPlay.UGUI
{
    public sealed class GameUIGrid : MonoBehaviour
    {
        public int arrangement, maxPerLine;
        public float cellWidth, cellHeight;
        public bool hideInactive;
        public int pivot, sorting;
        public Transform content;
        private void OnEnable() { Reposition(); }
        public void Reposition()
        {
            var root = content != null ? content : transform;
            int index = 0;
            var children = root.Cast<Transform>().Where(t => t.GetComponent<Canvas>() == null && (!hideInactive || t.gameObject.activeSelf)).ToList();
            if (sorting == 1) children.Sort((a, b) => string.CompareOrdinal(a.name, b.name));
            else if (sorting == 2) children.Sort((a, b) => a.localPosition.x.CompareTo(b.localPosition.x));
            else if (sorting == 3) children.Sort((a, b) => b.localPosition.y.CompareTo(a.localPosition.y));
            foreach (Transform child in children)
            {
                if (hideInactive && !child.gameObject.activeSelf) continue;
                int column = maxPerLine > 0 ? index % maxPerLine : index;
                int row = maxPerLine > 0 ? index / maxPerLine : 0;
                child.localPosition = arrangement == 0 ? new Vector3(column * cellWidth, -row * cellHeight, child.localPosition.z)
                    : new Vector3(row * cellWidth, -column * cellHeight, child.localPosition.z);
                index++;
            }
            if (pivot != 0 && children.Count > 0)
            {
                int columns = maxPerLine > 0 ? Mathf.Min(maxPerLine, children.Count) : children.Count;
                int rows = maxPerLine > 0 ? (children.Count - 1) / maxPerLine + 1 : 1;
                var offset = new Vector3((pivot % 3) * .5f * (arrangement == 0 ? columns - 1 : rows - 1) * cellWidth,
                    -(pivot / 3) * .5f * (arrangement == 0 ? rows - 1 : columns - 1) * cellHeight, 0);
                foreach (var child in children) child.localPosition -= offset;
            }
        }
    }
}

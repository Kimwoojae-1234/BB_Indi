using UnityEngine;
using UnityEngine.UI;

// An open-bottom tab joins the selected slot to the skill panel without a seam.
public class SkillPanelConnector : MaskableGraphic
{
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        Rect r = rectTransform.rect;
        AddTriangle(vh, new Vector2(0, r.yMax), new Vector2(r.xMin, r.yMin), new Vector2(r.xMax, r.yMin), new Color32(0, 43, 80, 255));
        AddTriangle(vh, new Vector2(0, r.yMax - 5), new Vector2(r.xMin + 4, r.yMin), new Vector2(r.xMax - 4, r.yMin), color);
    }
    private static void AddTriangle(VertexHelper vh, Vector2 a, Vector2 b, Vector2 c, Color32 tint)
    {
        int start = vh.currentVertCount;
        vh.AddVert(a, tint, Vector2.zero); vh.AddVert(b, tint, Vector2.zero); vh.AddVert(c, tint, Vector2.zero);
        vh.AddTriangle(start, start + 1, start + 2);
    }
}

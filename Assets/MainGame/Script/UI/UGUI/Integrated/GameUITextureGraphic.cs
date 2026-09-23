using UnityEngine;
using UnityEngine.UI;
namespace BaseBall.BallPlay.UGUI
{
    // Image provides Unity's sliced/tiled geometry while the source can still be a live texture.
    public sealed class GameUITextureGraphic : Image
    {
        public Texture sourceTexture;
        public Rect sourceUV = new Rect(0, 0, 1, 1);
        public override Texture mainTexture => sourceTexture != null ? sourceTexture : base.mainTexture;
        public void SetTexture(Texture texture, Rect uv)
        {
            if (sourceTexture != texture) { sourceTexture = texture; SetMaterialDirty(); }
            if (sourceUV != uv) { sourceUV = uv; SetVerticesDirty(); }
        }
        protected override void OnPopulateMesh(VertexHelper helper)
        {
            base.OnPopulateMesh(helper);
            var vertex = new UIVertex();
            for (int i = 0; i < helper.currentVertCount; i++)
            {
                helper.PopulateUIVertex(ref vertex, i);
                vertex.uv0 = new Vector4(sourceUV.x + vertex.uv0.x * sourceUV.width,
                    sourceUV.y + vertex.uv0.y * sourceUV.height, vertex.uv0.z, vertex.uv0.w);
                helper.SetUIVertex(vertex, i);
            }
        }
    }
}

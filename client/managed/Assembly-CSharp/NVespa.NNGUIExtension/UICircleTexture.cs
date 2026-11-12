using UnityEngine;

namespace NVespa.NNGUIExtension;

public class UICircleTexture : UITexture
{
	public int SplitCount = 8;

	public override void OnFill(RenderingList<Vector3> verts, RenderingList<Vector2> uvs, RenderingList<Color32> cols)
	{
		base.OnFill(verts, uvs, cols);
		UICircleSprite.FillSplit(SplitCount, verts, uvs, cols);
	}
}

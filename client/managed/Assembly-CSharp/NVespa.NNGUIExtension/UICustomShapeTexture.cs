using System.Collections.Generic;
using UnityEngine;

namespace NVespa.NNGUIExtension;

public class UICustomShapeTexture : UITexture
{
	public List<Transform> CustomVertices = new List<Transform>();

	protected override void Awake()
	{
		base.Awake();
		onPostFill = Convert;
	}

	public void Convert(UIWidget widget, int bufferOffset, RenderingList<Vector3> verts, RenderingList<Vector2> uvs, RenderingList<Color32> cols)
	{
		UICustomShapeSprite.Convert(CustomVertices, widget, bufferOffset, verts, uvs, cols);
	}
}

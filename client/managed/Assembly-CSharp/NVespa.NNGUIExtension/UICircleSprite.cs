using System;
using UnityEngine;

namespace NVespa.NNGUIExtension;

[ExecuteInEditMode]
public class UICircleSprite : UISprite
{
	public int SplitCount = 8;

	public override void OnFill(RenderingList<Vector3> verts, RenderingList<Vector2> uvs, RenderingList<Color32> cols)
	{
		base.OnFill(verts, uvs, cols);
		FillSplit(SplitCount, verts, uvs, cols);
	}

	public static void FillSplit(int splitCount, RenderingList<Vector3> verts, RenderingList<Vector2> uvs, RenderingList<Color32> cols)
	{
		if (verts == null || uvs == null || cols == null || verts.size < 4 || uvs.size < 4 || cols.size < 4)
		{
			return;
		}
		Vector3[] array = new Vector3[4]
		{
			verts[0],
			verts[1],
			verts[2],
			verts[3]
		};
		Vector2[] array2 = new Vector2[4]
		{
			uvs[0],
			uvs[1],
			uvs[2],
			uvs[3]
		};
		Vector3 item = (array[0] + array[1] + array[2] + array[3]) / 4f;
		Vector2 item2 = (array2[0] + array2[1] + array2[2] + array2[3]) / 4f;
		verts.Clear();
		uvs.Clear();
		for (int i = 0; i < splitCount; i += 2)
		{
			verts.Add(item);
			uvs.Add(item2);
			for (int j = 0; j < 3; j++)
			{
				int num = Mathf.Min(i + j, splitCount);
				float f = MathF.PI * -2f * (float)num / (float)splitCount;
				float t = Mathf.Cos(f) * 0.5f + 0.5f;
				float t2 = Mathf.Sin(f) * 0.5f + 0.5f;
				Vector3 a = Vector3.Lerp(array[0], array[3], t);
				Vector3 b = Vector3.Lerp(array[1], array[2], t);
				Vector3 item3 = Vector3.Lerp(a, b, t2);
				Vector2 a2 = Vector2.Lerp(array2[0], array2[3], t);
				Vector2 b2 = Vector2.Lerp(array2[1], array2[2], t);
				Vector2 item4 = Vector2.Lerp(a2, b2, t2);
				verts.Add(item3);
				uvs.Add(item4);
			}
		}
		Color32 item5 = cols[0];
		cols.Clear();
		cols.AddCount(item5, verts.size);
	}
}

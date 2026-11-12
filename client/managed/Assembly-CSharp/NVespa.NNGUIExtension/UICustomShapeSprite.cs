using System;
using System.Collections.Generic;
using UnityEngine;

namespace NVespa.NNGUIExtension;

public class UICustomShapeSprite : UISprite
{
	public List<Transform> CustomVertices = new List<Transform>();

	private const int RectVertexCount = 4;

	protected override void Awake()
	{
		base.Awake();
		onPostFill = Convert;
	}

	public void Convert(UIWidget widget, int bufferOffset, RenderingList<Vector3> verts, RenderingList<Vector2> uvs, RenderingList<Color32> cols)
	{
		Convert(CustomVertices, widget, bufferOffset, verts, uvs, cols);
	}

	public static void Convert(IList<Transform> customVertices, UIWidget widget, int bufferOffset, RenderingList<Vector3> verts, RenderingList<Vector2> uvs, RenderingList<Color32> cols)
	{
		List<Vector3> list = new List<Vector3>();
		foreach (Transform customVertex in customVertices)
		{
			list.Add(customVertex.localPosition);
		}
		Convert(list, widget, bufferOffset, verts, uvs, cols);
	}

	public static void Convert(IList<Vector3> customVertices, UIWidget widget, int bufferOffset, RenderingList<Vector3> verts, RenderingList<Vector2> uvs, RenderingList<Color32> cols)
	{
		if (__Extension_Method_ICollection__.COUNT(customVertices) < 4 || __Extension_Method_ICollection__.COUNT(customVertices) % 2 != 0)
		{
			return;
		}
		Vector2 uc = (uvs[0] + uvs[1] + uvs[2] + uvs[3]) / 4f;
		float num = Mathf.Min(verts[0].x, verts[1].x, verts[2].x, verts[3].x);
		float num2 = Mathf.Max(verts[0].x, verts[1].x, verts[2].x, verts[3].x);
		float num3 = Mathf.Min(verts[0].y, verts[1].y, verts[2].y, verts[3].y);
		float num4 = Mathf.Max(verts[0].y, verts[1].y, verts[2].y, verts[3].y);
		float num5 = Mathf.Min(uvs[0].x, uvs[1].x, uvs[2].x, uvs[3].x);
		float num6 = Mathf.Max(uvs[0].x, uvs[1].x, uvs[2].x, uvs[3].x);
		float num7 = Mathf.Min(uvs[0].y, uvs[1].y, uvs[2].y, uvs[3].y);
		float num8 = Mathf.Max(uvs[0].y, uvs[1].y, uvs[2].y, uvs[3].y);
		float num9 = num2 - num;
		float num10 = num4 - num3;
		float num11 = num6 - num5;
		float num12 = num8 - num7;
		float uvWidthRate = num11 / num9;
		float uvHeightRate = num12 / num10;
		Func<Vector3, Vector2> func = delegate(Vector3 vt)
		{
			Vector2 vector2 = vt;
			vector2.x = vt.x * uvWidthRate;
			vector2.y = vt.y * uvHeightRate;
			return uc + vector2;
		};
		int count = customVertices.Count / 2 - 1;
		verts.Clear();
		verts.ReserveCount(count);
		uvs.Clear();
		uvs.ReserveCount(count);
		int num13 = __Extension_Method_ICollection__.COUNT(customVertices);
		int num14 = 1;
		Vector3 vector = customVertices[0];
		do
		{
			verts.AddUnsafe(vector);
			uvs.AddUnsafe(func(vector));
			int num15 = 3;
			for (int num16 = 0; num16 < num15; num16++)
			{
				verts.AddUnsafe(customVertices[num14]);
				uvs.AddUnsafe(func(customVertices[num14]));
				if (num16 < num15 - 1)
				{
					num14++;
				}
			}
		}
		while (num14 < num13 - 1);
		Color32 item = cols[0];
		cols.Clear();
		cols.AddCount(item, verts.size);
	}
}

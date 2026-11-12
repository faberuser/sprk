using UnityEngine;

namespace NVespa.NNGUIExtension;

public class UITileTexture : UITexture
{
	public Vector2 UvMultiply = Vector2.one;

	public Vector2 UvOffset = Vector2.zero;

	public float CornerRoundLength;

	public int CornerRoundSplit = 3;

	public override void OnFill(RenderingList<Vector3> verts, RenderingList<Vector2> uvs, RenderingList<Color32> cols)
	{
		base.OnFill(verts, uvs, cols);
		if (verts == null || uvs == null || cols == null)
		{
			return;
		}
		Texture texture = mainTexture;
		if (!(texture == null))
		{
			int num = texture.width;
			int num2 = texture.height;
			float num3 = (float)base.width / (float)num;
			float num4 = (float)base.height / (float)num2;
			for (int i = 0; i < uvs.Count; i++)
			{
				Vector2 value = uvs[i];
				value.x = value.x * num3 * UvMultiply.x + UvOffset.x;
				value.y = value.y * num4 * UvMultiply.y + UvOffset.y;
				uvs[i] = value;
			}
			if (CornerRoundLength > 1f && type == Type.Simple)
			{
				RoundFill(verts, uvs, cols);
			}
		}
	}

	private void RoundFill(RenderingList<Vector3> verts, RenderingList<Vector2> uvs, RenderingList<Color32> cols)
	{
		if (verts.size >= 4 && uvs.size >= 4 && cols.size >= 4)
		{
			Vector3[] array = new Vector3[4]
			{
				verts[0],
				verts[1],
				verts[2],
				verts[3]
			};
			_ = new Vector2[4]
			{
				uvs[0],
				uvs[1],
				uvs[2],
				uvs[3]
			};
			verts.Clear();
			uvs.Clear();
			Color32 item = cols[0];
			Vector2[] array2 = new Vector2[4]
			{
				Vespa_Unity_Vector3_Extension_Method.XY(array[0]),
				new Vector2(array[0].x + CornerRoundLength, array[0].y + CornerRoundLength),
				new Vector2(array[2].x - CornerRoundLength, array[2].y - CornerRoundLength),
				Vespa_Unity_Vector3_Extension_Method.XY(array[2])
			};
			Vector2 vector = new Vector2(CornerRoundLength / (array2[3].x - array2[0].x), CornerRoundLength / (array2[3].y - array2[0].y));
			Vector2[] array3 = new Vector2[4]
			{
				uvs[0],
				new Vector2(Mathf.Lerp(uvs[0].x, uvs[2].x, vector.x), Mathf.Lerp(uvs[0].y, uvs[2].y, vector.y)),
				new Vector2(Mathf.Lerp(uvs[2].x, uvs[0].x, vector.x), Mathf.Lerp(uvs[2].y, uvs[0].y, vector.y)),
				uvs[2]
			};
			FillRound(new Vector2(array2[1].x, array2[2].y), new Vector2(array3[1].x, array3[2].y), new Vector2(array2[1].x, array2[3].y), new Vector2(array3[1].x, array3[3].y), new Vector2(array2[0].x, array2[2].y), new Vector2(array3[0].x, array3[2].y), verts, uvs);
			FillRect(1, 2, array2, array3, verts, uvs);
			FillRound(new Vector2(array2[2].x, array2[2].y), new Vector2(array3[2].x, array3[2].y), new Vector2(array2[3].x, array2[2].y), new Vector2(array3[3].x, array3[2].y), new Vector2(array2[2].x, array2[3].y), new Vector2(array3[2].x, array3[3].y), verts, uvs);
			FillRect(0, 1, array2, array3, verts, uvs);
			FillRect(1, 1, array2, array3, verts, uvs);
			FillRect(2, 1, array2, array3, verts, uvs);
			FillRound(new Vector2(array2[1].x, array2[1].y), new Vector2(array3[1].x, array3[1].y), new Vector2(array2[0].x, array2[1].y), new Vector2(array3[0].x, array3[1].y), new Vector2(array2[1].x, array2[0].y), new Vector2(array3[1].x, array3[0].y), verts, uvs);
			FillRect(1, 0, array2, array3, verts, uvs);
			FillRound(new Vector2(array2[2].x, array2[1].y), new Vector2(array3[2].x, array3[1].y), new Vector2(array2[2].x, array2[0].y), new Vector2(array3[2].x, array3[0].y), new Vector2(array2[3].x, array2[1].y), new Vector2(array3[3].x, array3[1].y), verts, uvs);
			if (cols.Count < verts.Count)
			{
				cols.AddCount(item, verts.Count - cols.Count);
			}
		}
	}

	private void FillRound(Vector2 center, Vector2 centerUv, Vector2 edge1, Vector2 edgeUv1, Vector2 edge2, Vector2 edgeUv2, RenderingList<Vector3> verts, RenderingList<Vector2> uvs)
	{
		int num = 2;
		int num2 = 1;
		for (int i = 0; i < CornerRoundSplit; i++)
		{
			num += num2;
			num2 *= 2;
		}
		Vector3[] array = new Vector3[num];
		Vector2[] array2 = new Vector2[num];
		array[0] = edge1;
		array[num - 1] = edge2;
		array2[0] = edgeUv1;
		array2[num - 1] = edgeUv2;
		Vector2 i_v = edge1 - center;
		for (int j = 1; j < num - 1; j++)
		{
			float i_degrees = 90f * (float)j / (float)(num - 1);
			array[j] = center + Math.Rotate(i_v, i_degrees);
			float t = Mathf.InverseLerp(edge1.x, edge2.x, array[j].x);
			float t2 = Mathf.InverseLerp(edge1.y, edge2.y, array[j].y);
			array2[j] = new Vector2(Mathf.Lerp(edgeUv1.x, edgeUv2.x, t), Mathf.Lerp(edgeUv1.y, edgeUv2.y, t2));
		}
		for (int k = 0; k < num - 2; k += 2)
		{
			verts.Add(center);
			uvs.Add(centerUv);
			for (int l = 0; l < 3; l++)
			{
				int num3 = Mathf.Min(k + l, num - 1);
				verts.Add(array[num3]);
				uvs.Add(array2[num3]);
			}
		}
	}

	private void FillRect(int xIndex, int yIndex, Vector2[] points, Vector2[] uvPoints, RenderingList<Vector3> verts, RenderingList<Vector2> uvs)
	{
		float x = points[xIndex].x;
		float x2 = points[xIndex + 1].x;
		float y = points[yIndex].y;
		float y2 = points[yIndex + 1].y;
		float x3 = uvPoints[xIndex].x;
		float x4 = uvPoints[xIndex + 1].x;
		float y3 = uvPoints[yIndex].y;
		float y4 = uvPoints[yIndex + 1].y;
		verts.ReserveCount(4);
		verts.AddUnsafe(new Vector3(x, y));
		verts.AddUnsafe(new Vector3(x, y2));
		verts.AddUnsafe(new Vector3(x2, y2));
		verts.AddUnsafe(new Vector3(x2, y));
		uvs.ReserveCount(4);
		uvs.AddUnsafe(new Vector2(x3, y3));
		uvs.AddUnsafe(new Vector2(x3, y4));
		uvs.AddUnsafe(new Vector2(x4, y4));
		uvs.AddUnsafe(new Vector2(x4, y3));
	}
}

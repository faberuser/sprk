using System;
using UnityEngine;

namespace NVespa.NNGUIExtension;

public class UIRoundCornerTexture : UITexture
{
	public float RoundAmount;

	public override void OnFill(RenderingList<Vector3> verts, RenderingList<Vector2> uvs, RenderingList<Color32> cols)
	{
		base.OnFill(verts, uvs, cols);
		FillRound(RoundAmount, verts, uvs, cols);
	}

	public void FillRound(float round, RenderingList<Vector3> verts, RenderingList<Vector2> uvs, RenderingList<Color32> cols)
	{
		Vector4 vector = drawingDimensions;
		Vector4 vector2 = base.drawingUVs;
		Color32 item = base.drawingColor;
		float num = vector.z - vector.x;
		float num2 = vector.w - vector.y;
		if (round <= 0f)
		{
			RoundAmount = 0f;
			return;
		}
		if (round >= num * 0.5f || round >= num2 * 0.5f)
		{
			round = Mathf.Min(num * 0.5f, num2 * 0.5f);
			RoundAmount = round;
		}
		verts.Clear();
		uvs.Clear();
		cols.Clear();
		float num3 = vector2.z - vector2.x;
		float num4 = round * (num3 / num);
		int num5 = 1;
		num5 = Mathf.CeilToInt(round * 0.5f);
		num5 = Mathf.Min(5, num5);
		float[] array = new float[num5 + 1];
		float[] array2 = new float[num5 + 1];
		float num6 = MathF.PI / 2f / (float)num5;
		for (int i = 0; i < num5; i++)
		{
			float num7 = Mathf.Cos(-MathF.PI + num6 * (float)i);
			float num8 = Mathf.Sqrt(1f - num7 * num7);
			array[i] = 1f + num7;
			array2[i] = num8;
		}
		array[num5] = 1f;
		array2[num5] = 1f;
		for (int j = 0; j < num5; j++)
		{
			float num9 = array[j] * round;
			float num10 = array[j + 1] * round;
			float num11 = array[j] * num4;
			float num12 = array[j + 1] * num4;
			float num13 = round - round * array2[j];
			float num14 = round - round * array2[j + 1];
			float num15 = num4 - num4 * array2[j];
			float num16 = num4 - num4 * array2[j + 1];
			verts.Add(new Vector3(vector.x + num9, vector.y + num13));
			verts.Add(new Vector3(vector.x + num9, vector.w - num13));
			verts.Add(new Vector3(vector.x + num10, vector.w - num14));
			verts.Add(new Vector3(vector.x + num10, vector.y + num14));
			uvs.Add(new Vector2(vector2.x + num11, vector2.y + num15));
			uvs.Add(new Vector2(vector2.x + num11, vector2.w - num15));
			uvs.Add(new Vector2(vector2.x + num12, vector2.w - num16));
			uvs.Add(new Vector2(vector2.x + num12, vector2.y + num16));
			verts.Add(new Vector3(vector.z - num9, vector.y + num13));
			verts.Add(new Vector3(vector.z - num9, vector.w - num13));
			verts.Add(new Vector3(vector.z - num10, vector.w - num14));
			verts.Add(new Vector3(vector.z - num10, vector.y + num14));
			uvs.Add(new Vector2(vector2.z - num11, vector2.y + num15));
			uvs.Add(new Vector2(vector2.z - num11, vector2.w - num15));
			uvs.Add(new Vector2(vector2.z - num12, vector2.w - num16));
			uvs.Add(new Vector2(vector2.z - num12, vector2.y + num16));
		}
		verts.Add(new Vector3(vector.x + round, vector.y));
		verts.Add(new Vector3(vector.x + round, vector.w));
		verts.Add(new Vector3(vector.z - round, vector.w));
		verts.Add(new Vector3(vector.z - round, vector.y));
		uvs.Add(new Vector2(vector2.x + num4, vector2.y));
		uvs.Add(new Vector2(vector2.x + num4, vector2.w));
		uvs.Add(new Vector2(vector2.z - num4, vector2.w));
		uvs.Add(new Vector2(vector2.z - num4, vector2.y));
		cols.AddCount(item, verts.size);
	}
}

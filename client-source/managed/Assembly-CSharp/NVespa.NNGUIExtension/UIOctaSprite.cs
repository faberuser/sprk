using UnityEngine;

namespace NVespa.NNGUIExtension;

public class UIOctaSprite : UISprite
{
	protected override void Awake()
	{
		base.Awake();
		onPostFill = ConvertToOcta;
	}

	public static void ConvertToOcta(UIWidget widget, int bufferOffset, RenderingList<Vector3> verts, RenderingList<Vector2> uvs, RenderingList<Color32> cols)
	{
		float num = Mathf.Sqrt(2f);
		Vector3 vector = (verts[3] + verts[0]) / 2f;
		Vector3 vector2 = (verts[0] + verts[1]) / 2f;
		Vector3 vector3 = (verts[1] + verts[2]) / 2f;
		Vector3 vector4 = (verts[2] + verts[3]) / 2f;
		Vector3 item = (vector + vector2) / num;
		Vector3 item2 = (vector2 + vector3) / num;
		Vector3 item3 = (vector3 + vector4) / num;
		Vector3 item4 = (vector4 + vector) / num;
		verts.Clear();
		verts.ReserveCount(12);
		verts.AddUnsafe(vector);
		verts.AddUnsafe(item);
		verts.AddUnsafe(vector2);
		verts.AddUnsafe(item2);
		verts.AddUnsafe(vector);
		verts.AddUnsafe(item2);
		verts.AddUnsafe(vector3);
		verts.AddUnsafe(item3);
		verts.AddUnsafe(vector);
		verts.AddUnsafe(item3);
		verts.AddUnsafe(vector4);
		verts.AddUnsafe(item4);
		Vector2 vector5 = (uvs[0] + uvs[1] + uvs[2] + uvs[3]) / 4f;
		Vector2 vector6 = uvs[0] - vector5;
		Vector2 vector7 = uvs[1] - vector5;
		Vector2 vector8 = uvs[2] - vector5;
		Vector2 vector9 = uvs[3] - vector5;
		Vector2 vector10 = (vector9 + vector6) / 2f;
		Vector2 vector11 = (vector6 + vector7) / 2f;
		Vector2 vector12 = (vector7 + vector8) / 2f;
		Vector2 vector13 = (vector8 + vector9) / 2f;
		Vector2 vector14 = (vector10 + vector11) / num;
		Vector2 vector15 = (vector11 + vector12) / num;
		Vector2 vector16 = (vector12 + vector13) / num;
		Vector2 vector17 = (vector13 + vector10) / num;
		uvs.Clear();
		uvs.ReserveCount(12);
		uvs.AddUnsafe(vector5 + vector10);
		uvs.AddUnsafe(vector5 + vector14);
		uvs.AddUnsafe(vector5 + vector11);
		uvs.AddUnsafe(vector5 + vector15);
		uvs.AddUnsafe(vector5 + vector10);
		uvs.AddUnsafe(vector5 + vector15);
		uvs.AddUnsafe(vector5 + vector12);
		uvs.AddUnsafe(vector5 + vector16);
		uvs.AddUnsafe(vector5 + vector10);
		uvs.AddUnsafe(vector5 + vector16);
		uvs.AddUnsafe(vector5 + vector13);
		uvs.AddUnsafe(vector5 + vector17);
		Color32 item5 = cols[0];
		cols.Clear();
		cols.AddCount(item5, verts.size);
	}
}

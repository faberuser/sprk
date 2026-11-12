using System;
using UnityEngine;

public static class Vespa_Unity_Vector2_Extension_Method
{
	public static float MaxComponent(this Vector2 i_this)
	{
		return Mathf.Max(i_this.x, i_this.y);
	}

	public static VectorXZ ToVecXZ(this Vector2 i_this)
	{
		return new VectorXZ(i_this.x, i_this.y);
	}

	public static Vector3 ToVec3(this Vector2 i_this)
	{
		return new Vector3(i_this.x, i_this.y, 0f);
	}

	public static Vector3 ToVec3(this Vector2 i_this, float zValue)
	{
		return new Vector3(i_this.x, i_this.y, zValue);
	}

	public static Vector2 Rotate(this Vector2 i_this, float i_angle)
	{
		float num = Mathf.Sin(i_angle * (MathF.PI / 180f));
		float num2 = Mathf.Cos(i_angle * (MathF.PI / 180f));
		float x = i_this.x;
		float y = i_this.y;
		i_this.x = num2 * x - num * y;
		i_this.y = num * x + num2 * y;
		return i_this;
	}
}

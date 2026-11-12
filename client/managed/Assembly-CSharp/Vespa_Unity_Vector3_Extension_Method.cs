using UnityEngine;

public static class Vespa_Unity_Vector3_Extension_Method
{
	public static bool IsUnitVector(this Vector3 This)
	{
		if (Mathf.Approximately(This.x, This.y))
		{
			return Mathf.Approximately(This.x, This.z);
		}
		return false;
	}

	public static Vector3 Abs(this Vector3 i_this)
	{
		return new Vector3(Mathf.Abs(i_this.x), Mathf.Abs(i_this.y), Mathf.Abs(i_this.z));
	}

	public static float MaxComponent(this Vector3 i_this)
	{
		return Mathf.Max(i_this.x, Mathf.Max(i_this.y, i_this.z));
	}

	public static Vector2 XY(this Vector3 i_src)
	{
		return new Vector2(i_src.x, i_src.y);
	}

	public static Vector2 XZ(this Vector3 i_src)
	{
		return new Vector2(i_src.x, i_src.z);
	}

	public static VectorXZ VecXZ(this Vector3 i_src)
	{
		return new VectorXZ(i_src.x, i_src.z);
	}

	public static Vector2 YZ(this Vector3 i_src)
	{
		return new Vector2(i_src.y, i_src.z);
	}

	public static Vector3 ReplaceX(this Vector3 i_src, float i_value)
	{
		return new Vector3(i_value, i_src.y, i_src.z);
	}

	public static Vector3 ReplaceY(this Vector3 i_src, float i_value)
	{
		return new Vector3(i_src.x, i_value, i_src.z);
	}

	public static Vector3 ReplaceZ(this Vector3 i_src, float i_value)
	{
		return new Vector3(i_src.x, i_src.y, i_value);
	}

	public static Vector3 ReplaceXY(this Vector3 i_src, float i_x, float i_y)
	{
		return new Vector3(i_x, i_y, i_src.z);
	}

	public static Vector3 ReplaceYZ(this Vector3 i_src, float i_y, float i_z)
	{
		return new Vector3(i_src.x, i_y, i_z);
	}

	public static Vector3 ReplaceXZ(this Vector3 i_src, float i_x, float i_z)
	{
		return new Vector3(i_x, i_src.y, i_z);
	}

	public static Vector3 Round(this Vector3 i_value)
	{
		Vector3 result = default(Vector3);
		result.x = Mathf.Round(i_value.x);
		result.y = Mathf.Round(i_value.y);
		result.z = Mathf.Round(i_value.z);
		return result;
	}
}

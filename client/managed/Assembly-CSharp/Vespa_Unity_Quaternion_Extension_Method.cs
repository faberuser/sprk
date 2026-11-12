using UnityEngine;

public static class Vespa_Unity_Quaternion_Extension_Method
{
	public static float Distance(this Quaternion i_this, Quaternion i_rhs)
	{
		float num = i_this.x - i_rhs.x;
		float num2 = i_this.y - i_rhs.y;
		float num3 = i_this.z - i_rhs.z;
		float num4 = i_this.w - i_rhs.w;
		return Mathf.Sqrt(num * num + num2 * num2 + num3 * num3 + num4 * num4);
	}
}

using UnityEngine;

public static class Vespa_Unity_Bounds_Extension_Method
{
	public static Bounds AddMinMax(this Bounds This, Vector3 min, Vector3 max)
	{
		Bounds result = This;
		result.center += (min + max) * 0.5f;
		result.size += max - min;
		return result;
	}
}

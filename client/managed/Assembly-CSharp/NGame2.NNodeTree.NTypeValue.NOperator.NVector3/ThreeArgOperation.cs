using UnityEngine;

namespace NGame2.NNodeTree.NTypeValue.NOperator.NVector3;

public static class ThreeArgOperation
{
	public static Vector3 Rotate(Vector3 dir, Vector3 axis, float angle)
	{
		return Quaternion.AngleAxis(angle, axis) * dir;
	}

	public static Vector3 FromXYZ(float x, float y, float z)
	{
		return new Vector3(x, y, z);
	}
}

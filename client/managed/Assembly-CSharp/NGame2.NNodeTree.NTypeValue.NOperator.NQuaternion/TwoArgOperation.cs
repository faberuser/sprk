using UnityEngine;

namespace NGame2.NNodeTree.NTypeValue.NOperator.NQuaternion;

public static class TwoArgOperation
{
	public static Quaternion Multiply(Quaternion parent, Quaternion child)
	{
		return parent * child;
	}

	public static Quaternion FromToRotation(Vector3 from, Vector3 to)
	{
		return Quaternion.FromToRotation(from, to);
	}
}

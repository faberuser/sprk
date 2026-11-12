using NVespa;
using UnityEngine;

namespace NGame2.NNodeTree.NTypeValue.NOperator.NVector3;

public static class TwoArgOperation
{
	public static Vector3 Add(Vector3 a, Vector3 b)
	{
		return a + b;
	}

	public static Vector3 Sub(Vector3 a, Vector3 b)
	{
		return a - b;
	}

	public static Vector3 Multiply(Vector3 a, float b)
	{
		return a * b;
	}

	public static Vector3 Divide(Vector3 a, float b)
	{
		return a / Vespa_Float_Extension_Method.NoneZero(b);
	}

	public static Vector3 ReplaceX(Vector3 a, float b)
	{
		return new Vector3(b, a.y, a.z);
	}

	public static Vector3 ReplaceY(Vector3 a, float b)
	{
		return new Vector3(a.x, b, a.z);
	}

	public static Vector3 ReplaceZ(Vector3 a, float b)
	{
		return new Vector3(a.x, a.y, b);
	}

	public static Vector3 Random(Vector3 a, Vector3 b)
	{
		return Math.RandomVec3(a, b);
	}

	public static Vector3 LookAtDirection(Vector3 from, Vector3 to)
	{
		return (to - from).normalized;
	}
}

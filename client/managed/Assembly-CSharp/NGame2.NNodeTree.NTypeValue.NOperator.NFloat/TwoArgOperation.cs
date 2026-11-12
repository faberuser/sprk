using System;
using UnityEngine;

namespace NGame2.NNodeTree.NTypeValue.NOperator.NFloat;

public static class TwoArgOperation
{
	public static float Add(float a, float b)
	{
		return a + b;
	}

	public static float Sub(float a, float b)
	{
		return a - b;
	}

	public static float Multiply(float a, float b)
	{
		return a * b;
	}

	public static float Divide(float a, float b)
	{
		try
		{
			return a / b;
		}
		catch (Exception)
		{
			return a;
		}
	}

	public static float Distance(Vector3 a, Vector3 b)
	{
		return Vector3.Distance(a, b);
	}
}

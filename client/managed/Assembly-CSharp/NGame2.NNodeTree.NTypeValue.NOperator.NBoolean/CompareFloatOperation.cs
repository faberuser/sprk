using UnityEngine;

namespace NGame2.NNodeTree.NTypeValue.NOperator.NBoolean;

public static class CompareFloatOperation
{
	public static bool Equal(float a, float b)
	{
		return Mathf.Approximately(a, b);
	}

	public static bool Less(float a, float b)
	{
		if (Mathf.Approximately(a, b))
		{
			return false;
		}
		return a < b;
	}

	public static bool Great(float a, float b)
	{
		if (Mathf.Approximately(a, b))
		{
			return false;
		}
		return a > b;
	}

	public static bool LessEqual(float a, float b)
	{
		if (Mathf.Approximately(a, b))
		{
			return true;
		}
		return a < b;
	}

	public static bool GreatEqual(float a, float b)
	{
		if (Mathf.Approximately(a, b))
		{
			return true;
		}
		return a > b;
	}
}

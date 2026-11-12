namespace NGame2.NNodeTree.NTypeValue.NOperator.NBoolean;

public static class BooleanOperation
{
	public static bool Equal(bool a, bool b)
	{
		return a == b;
	}

	public static bool NotEqual(bool a, bool b)
	{
		return a != b;
	}

	public static bool And(bool a, bool b)
	{
		return a && b;
	}

	public static bool Or(bool a, bool b)
	{
		return a || b;
	}

	public static bool Inverse(bool a)
	{
		return !a;
	}
}

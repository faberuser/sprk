using NShared;

public static class HashString_Extension_Method
{
	public static int TOINT(this HashString This, int i_default)
	{
		if (!int.TryParse(This, out var result))
		{
			return i_default;
		}
		return result;
	}

	public static bool IsNullOrEmpty(this HashString This)
	{
		if (!(This == null))
		{
			return This.Length == 0;
		}
		return true;
	}

	public static bool TryToInt(this HashString This, out int o_output)
	{
		return int.TryParse(This, out o_output);
	}

	public static string[] ToArray(this HashString[] This)
	{
		if (This == null)
		{
			return null;
		}
		string[] array = new string[This.Length];
		for (int i = 0; i < This.Length; i++)
		{
			array[i] = This[i];
		}
		return array;
	}
}

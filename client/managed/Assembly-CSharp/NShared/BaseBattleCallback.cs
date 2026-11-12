namespace NShared;

public abstract class BaseBattleCallback
{
	public long GlobalTimestamp { get; set; }

	public abstract bool IsEquals(BaseBattleCallback callback);

	public static bool ArrayIsEquals(int[] lhs, int[] rhs)
	{
		int num = ((lhs != null) ? lhs.Length : 0);
		int num2 = ((rhs != null) ? rhs.Length : 0);
		if (num != num2)
		{
			return false;
		}
		if (num == 0)
		{
			return true;
		}
		for (int i = 0; i < num; i++)
		{
			if (lhs[i] != rhs[i])
			{
				return false;
			}
		}
		return true;
	}

	public static bool ArrayIsEquals(long[] lhs, long[] rhs)
	{
		int num = ((lhs != null) ? lhs.Length : 0);
		int num2 = ((rhs != null) ? rhs.Length : 0);
		if (num != num2)
		{
			return false;
		}
		if (num == 0)
		{
			return true;
		}
		for (int i = 0; i < num; i++)
		{
			if (lhs[i] != rhs[i])
			{
				return false;
			}
		}
		return true;
	}

	public static bool ArrayIsEquals(string[] lhs, string[] rhs)
	{
		int num = ((lhs != null) ? lhs.Length : 0);
		int num2 = ((rhs != null) ? rhs.Length : 0);
		if (num != num2)
		{
			return false;
		}
		if (num == 0)
		{
			return true;
		}
		for (int i = 0; i < num; i++)
		{
			if (lhs[i] != rhs[i])
			{
				return false;
			}
		}
		return true;
	}
}

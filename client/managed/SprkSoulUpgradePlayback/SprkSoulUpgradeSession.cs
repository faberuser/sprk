using System;
using System.Collections;

public sealed class SprkSoulUpgradeSession
{
	public int Generation { get; private set; }

	public bool Active { get; private set; }

	public void Begin()
	{
		Generation++;
		Active = true;
	}

	public void End()
	{
		Generation++;
		Active = false;
	}

	public IEnumerator Guard(IEnumerator source, int token, Func<bool> visible)
	{
		try
		{
			while (Active && Generation == token && visible() && source.MoveNext())
			{
				yield return source.Current;
			}
		}
		finally
		{
			if (source is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}
	}

	public static bool IsUpgrade(string pack, string tree)
	{
		if (pack == "Common/Cinema")
		{
			if (!(tree == "SoulWeapon_RankUP_1"))
			{
				return tree == "SoulWeapon_RankUP_2";
			}
			return true;
		}
		return false;
	}
}

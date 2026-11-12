using System;

namespace NShared;

public class ValueConfuserFactory
{
	private static Random _random = new Random(DateTime.UtcNow.Millisecond);

	private static bool _disable = false;

	public static void DisableConfuser(bool disable)
	{
		_disable = disable;
	}

	public static IValueConfuser<T> Create<T>()
	{
		if (_disable)
		{
			return null;
		}
		Type typeFromHandle = typeof(T);
		if (typeFromHandle == typeof(long))
		{
			return new LongValueConfuser(_random.Next()) as IValueConfuser<T>;
		}
		if (typeFromHandle == typeof(int))
		{
			return new IntValueConfuser(_random.Next()) as IValueConfuser<T>;
		}
		return null;
	}
}

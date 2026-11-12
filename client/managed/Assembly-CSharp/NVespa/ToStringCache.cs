namespace NVespa;

public class ToStringCache
{
	private const long CacheCount = 1000L;

	private static string[] _array;

	static ToStringCache()
	{
		_array = new string[1000];
		for (long num = 0L; num < 1000; num++)
		{
			_array[num] = num.ToString();
		}
	}

	public static string IntToString(int value)
	{
		if (value >= 0 && (long)value < 1000L)
		{
			return _array[value];
		}
		return value.ToString();
	}

	public static string LongToString(long value)
	{
		if (value >= 0 && value < 1000)
		{
			return _array[value];
		}
		return value.ToString();
	}
}

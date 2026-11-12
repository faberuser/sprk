using NVespa;

public static class Vespa_ThreadSafeDic_ExtensionMethod
{
	public static TValue GETVALUE<TKey, TValue>(this ThreadSafeDic<TKey, TValue> This, TKey key, TValue defaultValue)
	{
		if (This == null)
		{
			return defaultValue;
		}
		if (!This.TryGetValue(key, out var o_value))
		{
			return defaultValue;
		}
		return o_value;
	}

	public static bool IsNullOrEmpty<TKey, TValue>(this ThreadSafeDic<TKey, TValue> This)
	{
		if (This == null)
		{
			return true;
		}
		return This.Count == 0;
	}

	public static int COUNT<TKey, TValue>(this ThreadSafeDic<TKey, TValue> This)
	{
		return This?.Count ?? 0;
	}
}

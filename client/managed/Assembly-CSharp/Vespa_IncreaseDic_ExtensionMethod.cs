using System.Collections.Generic;
using NVespa;

public static class Vespa_IncreaseDic_ExtensionMethod
{
	public static TValue GETVALUE<TKey, TValue>(this IncreaseDic<TKey, TValue> This, TKey key, TValue defaultValue)
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

	public static bool IsValidIndex<TKey, TValue>(this IncreaseDic<TKey, TValue> This, int index)
	{
		if (This == null)
		{
			return false;
		}
		if (index < 0 || index >= This.Count)
		{
			return false;
		}
		return true;
	}

	public static bool IsNullOrEmpty<TKey, TValue>(this IncreaseDic<TKey, TValue> This)
	{
		if (This == null)
		{
			return true;
		}
		return This.Count == 0;
	}

	public static int GetNumberOfPartialKeys<TKey, TValue>(this IncreaseDic<TKey, TValue> This, string partialKey)
	{
		int count = 0;
		This._list.ForEach(delegate(KeyValuePair<TKey, TValue> e)
		{
			if (e.Key is string text && text.Contains(partialKey))
			{
				int num = count + 1;
				count = num;
			}
		});
		return count;
	}
}

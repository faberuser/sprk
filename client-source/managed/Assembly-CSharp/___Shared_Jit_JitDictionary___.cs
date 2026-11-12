using System;
using System.Collections.Generic;
using NShared.NJit;

public static class ___Shared_Jit_JitDictionary___
{
	public static int COUNT<K, V>(this IJitDictionaryRead<K, V> This)
	{
		return This?.Count ?? 0;
	}

	public static V GetOrDefault<K, V>(this IJitDictionaryRead<K, V> This, K key)
	{
		return This.GetOrDefault(key, default(V));
	}

	public static V GetOrDefault<K, V>(this IJitDictionaryRead<K, V> This, K key, V defaultV)
	{
		if (This == null)
		{
			return defaultV;
		}
		if (This.TryGetValue(key, out var value))
		{
			return value;
		}
		return defaultV;
	}

	public static V JsonLoad_ValueGetOrNew<K, V>(this JitDictionary<K, V> dic, K key) where V : class, new()
	{
		if (dic.JsonLoad_Get().TryGetValue(key, out var value))
		{
			return value;
		}
		value = new V();
		dic.JsonLoad_Get().Add(key, value);
		return value;
	}

	public static void JsonLoad_AddIfNotExist<K, V>(this JitDictionary<K, V> dic, K key, V value)
	{
		if (!dic.JsonLoad_Get().ContainsKey(key))
		{
			dic.JsonLoad_Get().Add(key, value);
		}
	}

	public static void JsonLoad_AddToListValue<K, V>(this JitDictionary<K, List<V>> dic, K key, V value)
	{
		dic.JsonLoad_ValueGetOrNew(key).Add(value);
	}

	public static void JsonLoad_SortListValue<K, V>(this JitDictionary<K, List<V>> dic, Comparison<V> comparison)
	{
		foreach (KeyValuePair<K, List<V>> item in dic.JsonLoad_Get())
		{
			item.Value.Sort(comparison);
		}
	}

	public static void JsonLoad_AddToHashSetValue<K, V>(this JitDictionary<K, HashSet<V>> dic, K key, V value)
	{
		HashSet<V> hashSet = dic.JsonLoad_ValueGetOrNew(key);
		if (!hashSet.Contains(value))
		{
			hashSet.Add(value);
		}
	}
}

using System;
using System.Collections.Generic;

public static class Vespa_Generic_IDictionary_Extension_Method
{
	public static int GetNumberOfPartialKeys<TKey, TValue>(this IDictionary<TKey, TValue> This, string partialKey)
	{
		int num = 0;
		foreach (TKey key in This.Keys)
		{
			if (key is string text && text.Contains(partialKey))
			{
				num++;
			}
		}
		return num;
	}

	public static TValue ADDVALUE<TKey, TValue>(this IDictionary<TKey, TValue> i_this, TKey i_key, TValue value, TValue i_default = default(TValue)) where TValue : struct
	{
		TValue value2 = i_default;
		if (!i_this.TryGetValue(i_key, out value2))
		{
			i_this.Add(i_key, value2);
		}
		return i_this[i_key] = (TValue)Convert.ChangeType(Convert.ToDecimal(value2) + Convert.ToDecimal(value), typeof(TValue));
	}

	public static TValue UPDATEVALUE<TKey, TValue>(this IDictionary<TKey, TValue> i_this, TKey i_key, TValue value) where TValue : struct
	{
		if (!i_this.TryGetValue(i_key, out var value2))
		{
			i_this.Add(i_key, value2);
		}
		return i_this[i_key] = (TValue)Convert.ChangeType(value, typeof(TValue));
	}

	public static bool MAXVALUE<TKey, TValue>(this IDictionary<TKey, TValue> i_this, TKey i_key, TValue value, TValue i_default = default(TValue)) where TValue : struct
	{
		TValue value2 = i_default;
		if (!i_this.TryGetValue(i_key, out value2))
		{
			i_this.Add(i_key, value2);
		}
		TValue value3 = (TValue)Convert.ChangeType(Math.Max(Convert.ToDecimal(value2), Convert.ToDecimal(value)), typeof(TValue));
		i_this[i_key] = value3;
		return Convert.ToDecimal(value2) <= Convert.ToDecimal(value);
	}

	public static TValue GETVALUE<TKey, TValue>(this IDictionary<TKey, TValue> i_this, TKey i_key, TValue i_default)
	{
		if (i_this == null)
		{
			return i_default;
		}
		if (i_this.TryGetValue(i_key, out var value))
		{
			return value;
		}
		return i_default;
	}

	public static void ConditionRemove<TKey, TValue>(this IDictionary<TKey, TValue> i_this, Func<TKey, TValue, bool> i_conditionFunc)
	{
		if (i_this == null)
		{
			return;
		}
		List<TKey> list = new List<TKey>();
		foreach (KeyValuePair<TKey, TValue> i_thi in i_this)
		{
			if (Vespa_System_Action_Extension_Method.fire(i_conditionFunc, i_thi.Key, i_thi.Value))
			{
				list.Add(i_thi.Key);
			}
		}
		for (int i = 0; i < list.Count; i++)
		{
			i_this.Remove(list[i]);
		}
		list.Clear();
	}

	public static void RemoveList<TKey, TValue>(this IDictionary<TKey, TValue> This, IList<TKey> keyList)
	{
		if (This != null)
		{
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(keyList); i++)
			{
				This.Remove(keyList[i]);
			}
		}
	}

	public static void AddListValue<TKey, TValue>(this IDictionary<TKey, List<TValue>> This, TKey key, TValue value, bool isAllowDuplicate = true)
	{
		if (This != null)
		{
			List<TValue> list = null;
			if (!This.ContainsKey(key))
			{
				list = new List<TValue>();
				This.Add(key, list);
			}
			else
			{
				list = This[key];
			}
			if (list != null && (isAllowDuplicate || !list.Contains(value)))
			{
				list.Add(value);
			}
		}
	}
}

using System;
using System.Collections.Generic;

namespace NVespa;

public class WeakDictionary<TKey, TValue> where TKey : class
{
	private Dictionary<int, List<WeakPair<TKey, TValue>>> _dic = new Dictionary<int, List<WeakPair<TKey, TValue>>>();

	public TValue this[TKey key]
	{
		get
		{
			return GetWeakPair(key, i_autoCreate: true)._value;
		}
		set
		{
			GetWeakPair(key, i_autoCreate: true)._value = value;
		}
	}

	public void DumpSummary()
	{
		foreach (KeyValuePair<int, List<WeakPair<TKey, TValue>>> item in _dic)
		{
			_ = item;
		}
	}

	public bool Add(TKey i_key, TValue i_value)
	{
		if (GetWeakPair(i_key, i_autoCreate: false) != null)
		{
			return false;
		}
		GetWeakPair(i_key, i_autoCreate: true)._value = i_value;
		return true;
	}

	public void Clear()
	{
		_dic.Clear();
	}

	public bool Remove(TKey i_key)
	{
		if (i_key == null)
		{
			return false;
		}
		List<WeakPair<TKey, TValue>> vALUE = Vespa_Generic_IDictionary_Extension_Method.GETVALUE(_dic, i_key.GetHashCode(), null);
		if (vALUE == null)
		{
			return false;
		}
		bool result = false;
		for (int i = 0; i < vALUE.Count; i++)
		{
			WeakPair<TKey, TValue> weakPair = vALUE[i];
			if (!weakPair._key.IsAlive)
			{
				vALUE.RemoveAt(i);
				i--;
			}
			else if (Vespa_Common_Class_Extension_Method.EQUALS(weakPair._key.Target, i_key))
			{
				vALUE.RemoveAt(i);
				result = true;
				break;
			}
		}
		if (__Extension_Method_ICollection__.IsNullOrEmpty(vALUE))
		{
			_dic.Remove(i_key.GetHashCode());
		}
		return result;
	}

	public bool TryGetValue(TKey i_key, out TValue o_value)
	{
		WeakPair<TKey, TValue> weakPair = GetWeakPair(i_key, i_autoCreate: false);
		if (weakPair == null)
		{
			o_value = default(TValue);
			return false;
		}
		o_value = weakPair._value;
		return true;
	}

	public void Iterate(Func<TKey, TValue, bool> i_iterFunc)
	{
		foreach (KeyValuePair<int, List<WeakPair<TKey, TValue>>> item in _dic)
		{
			for (int i = 0; i < item.Value.Count; i++)
			{
				WeakPair<TKey, TValue> weakPair = item.Value[i];
				if (!weakPair._key.IsAlive)
				{
					item.Value.RemoveAt(i);
					i--;
				}
				else if (Vespa_System_Action_Extension_Method.fire(i_iterFunc, weakPair.Key, weakPair.Value))
				{
					return;
				}
			}
		}
	}

	private WeakPair<TKey, TValue> GetWeakPair(TKey i_key, bool i_autoCreate)
	{
		if (i_key == null)
		{
			return null;
		}
		List<WeakPair<TKey, TValue>> list = Vespa_Generic_IDictionary_Extension_Method.GETVALUE(_dic, i_key.GetHashCode(), null);
		if (list == null)
		{
			if (!i_autoCreate)
			{
				return null;
			}
			list = new List<WeakPair<TKey, TValue>>();
			_dic.Add(i_key.GetHashCode(), list);
		}
		for (int i = 0; i < list.Count; i++)
		{
			WeakPair<TKey, TValue> weakPair = list[i];
			if (!weakPair._key.IsAlive)
			{
				list.RemoveAt(i);
				i--;
			}
			else if (Vespa_Common_Class_Extension_Method.EQUALS(weakPair._key.Target, i_key))
			{
				return weakPair;
			}
		}
		if (!i_autoCreate)
		{
			return null;
		}
		WeakPair<TKey, TValue> weakPair2 = new WeakPair<TKey, TValue>();
		weakPair2._key = new WeakReference(i_key);
		list.Add(weakPair2);
		return weakPair2;
	}
}

using System;
using System.Collections;
using System.Collections.Generic;

namespace NVespa;

public class IncreaseDic<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable, ICollection<KeyValuePair<TKey, TValue>>
{
	internal Dictionary<TKey, TValue> _dic;

	internal List<KeyValuePair<TKey, TValue>> _list;

	public Dictionary<TKey, TValue> RawDic => _dic;

	public int Count => _dic.Count;

	public bool IsReadOnly => false;

	public static IncreaseDic<TKey, TValue> CreateFromReservePool()
	{
		return SingletonAuto<ReservePool<IncreaseDic<TKey, TValue>>>.instance.Create();
	}

	public static IncreaseDic<TKey, TValue> CreateWithComparer()
	{
		return new IncreaseDic<TKey, TValue>(SingletonAuto<EqualityComparerCache>.instance.Get<TKey>());
	}

	public static IncreaseDic<TKey, TValue> CreateWithComparer(int capacity)
	{
		return new IncreaseDic<TKey, TValue>(capacity, SingletonAuto<EqualityComparerCache>.instance.Get<TKey>());
	}

	public IncreaseDic()
	{
		if (typeof(TKey).IsEnum)
		{
			_dic = new Dictionary<TKey, TValue>(SingletonAuto<EqualityComparerCache>.instance.Get<TKey>());
		}
		else
		{
			_dic = new Dictionary<TKey, TValue>();
		}
		_list = new List<KeyValuePair<TKey, TValue>>();
	}

	public IncreaseDic(int capacity)
	{
		if (typeof(TKey).IsEnum)
		{
			_dic = new Dictionary<TKey, TValue>(capacity, SingletonAuto<EqualityComparerCache>.instance.Get<TKey>());
		}
		else
		{
			_dic = new Dictionary<TKey, TValue>(capacity);
		}
		_list = new List<KeyValuePair<TKey, TValue>>(capacity);
	}

	public IncreaseDic(IEqualityComparer<TKey> comparer)
	{
		_dic = new Dictionary<TKey, TValue>(comparer);
		_list = new List<KeyValuePair<TKey, TValue>>();
	}

	public IncreaseDic(int capacity, IEqualityComparer<TKey> comparer)
	{
		_dic = new Dictionary<TKey, TValue>(capacity, comparer);
		_list = new List<KeyValuePair<TKey, TValue>>(capacity);
	}

	public bool Add(TKey key, TValue value)
	{
		if (_dic.ContainsKey(key))
		{
			return false;
		}
		_dic.Add(key, value);
		_list.Add(new KeyValuePair<TKey, TValue>(key, value));
		return true;
	}

	public void AddAndOverride(TKey key, TValue value)
	{
		if (!_dic.ContainsKey(key))
		{
			_dic.Add(key, value);
			_list.Add(new KeyValuePair<TKey, TValue>(key, value));
			return;
		}
		for (int i = 0; i < _list.Count; i++)
		{
			if (_list[i].Key.Equals(key))
			{
				KeyValuePair<TKey, TValue> keyValuePair = _list[i];
				_list[i] = new KeyValuePair<TKey, TValue>(keyValuePair.Key, value);
				_dic[key] = value;
				break;
			}
		}
	}

	public void RemoveByIndex(int index)
	{
		TKey key = _list[index].Key;
		_list.RemoveAt(index);
		_dic.Remove(key);
	}

	public bool RemoveSlow(TKey key)
	{
		if (!_dic.Remove(key))
		{
			return false;
		}
		for (int i = 0; i < _list.Count; i++)
		{
			if (_list[i].Key.Equals(key))
			{
				_list.RemoveAt(i);
				break;
			}
		}
		return true;
	}

	public void Clear()
	{
		_dic.Clear();
		_list.Clear();
	}

	public void SortByKey(Comparison<TKey> comparison)
	{
		_list.Sort((KeyValuePair<TKey, TValue> lhs, KeyValuePair<TKey, TValue> rhs) => comparison(lhs.Key, rhs.Key));
	}

	public void SortByValue(Comparison<TValue> comparison)
	{
		_list.Sort((KeyValuePair<TKey, TValue> lhs, KeyValuePair<TKey, TValue> rhs) => comparison(lhs.Value, rhs.Value));
	}

	public void Reverse()
	{
		_list.Reverse();
	}

	public int IndexOfKey(TKey key)
	{
		for (int i = 0; i < _list.Count; i++)
		{
			if (_dic.Comparer.Equals(key, _list[i].Key))
			{
				return i;
			}
		}
		return -1;
	}

	public bool ContainsKey(TKey key)
	{
		return _dic.ContainsKey(key);
	}

	public bool ContainsValue(TValue value)
	{
		return _dic.ContainsValue(value);
	}

	public bool TryGetValue(TKey key, out TValue o_value)
	{
		return _dic.TryGetValue(key, out o_value);
	}

	public TKey GetKeyByIndex(int index)
	{
		return _list[index].Key;
	}

	public TValue GetValueByIndex(int index)
	{
		return _list[index].Value;
	}

	public KeyValuePair<TKey, TValue> GetKeyValuePairByIndex(int index)
	{
		return _list[index];
	}

	public TValue[] GetAllValues()
	{
		TValue[] array = new TValue[_list.Count];
		for (int i = 0; i < _list.Count; i++)
		{
			array[i] = _list[i].Value;
		}
		return array;
	}

	public void SetValueByIndex(int index, TValue value)
	{
		KeyValuePair<TKey, TValue> keyValuePair = _list[index];
		_list[index] = new KeyValuePair<TKey, TValue>(keyValuePair.Key, value);
		_dic[keyValuePair.Key] = value;
	}

	public IEnumerable GetEnumerable()
	{
		yield return _list.GetEnumerator();
	}

	IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
	{
		return _list.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return _list.GetEnumerator();
	}

	void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> pair)
	{
		Add(pair.Key, pair.Value);
	}

	bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> pair)
	{
		return Vespa_Common_Class_Extension_Method.Interface((ICollection<KeyValuePair<TKey, TValue>>)_dic).Contains(pair);
	}

	void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
	{
		Vespa_Common_Class_Extension_Method.Interface((ICollection<KeyValuePair<TKey, TValue>>)_dic).CopyTo(array, arrayIndex);
	}

	bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> pair)
	{
		return RemoveSlow(pair.Key);
	}
}

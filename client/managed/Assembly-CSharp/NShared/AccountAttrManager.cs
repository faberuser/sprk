using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NShared;

public class AccountAttrManager<TKey, TValue> : IEnumerable<TValue>, IEnumerable where TValue : IAccountAttrKey<TKey>
{
	private readonly Dictionary<TKey, TValue> _attrDictionary = new Dictionary<TKey, TValue>();

	public void Add(TValue value)
	{
		if (value != null)
		{
			OnAddData(value.Key, value);
		}
	}

	protected virtual void OnAddData(TKey key, TValue value)
	{
		_attrDictionary.Remove(key);
		_attrDictionary.Add(key, value);
	}

	public virtual bool Remove(TKey key)
	{
		return _attrDictionary.Remove(key);
	}

	public virtual bool Remove(TValue value)
	{
		if (value == null)
		{
			return false;
		}
		return _attrDictionary.Remove(value.Key);
	}

	public virtual void RemoveAll()
	{
		_attrDictionary.Clear();
	}

	public TValue Get(TKey key)
	{
		_attrDictionary.TryGetValue(key, out var value);
		return value;
	}

	public TValue[] GetValues()
	{
		return _attrDictionary.Values.ToArray();
	}

	public virtual void Reset(TValue[] values)
	{
		RemoveAll();
		Set(values);
	}

	public virtual void Set(TValue[] values)
	{
		if (values != null)
		{
			foreach (TValue value in values)
			{
				Add(value);
			}
		}
	}

	public void Unset(TValue[] values)
	{
		if (values != null)
		{
			foreach (TValue value in values)
			{
				Remove(value);
			}
		}
	}

	public IEnumerator<TValue> GetEnumerator()
	{
		return _attrDictionary.Values.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public TValue[] ToArray()
	{
		return _attrDictionary.Values.ToArray();
	}

	public List<TValue> ToList()
	{
		return _attrDictionary.Values.ToList();
	}

	public int Count()
	{
		return _attrDictionary.Count;
	}

	public bool ContainKey(TKey key)
	{
		return _attrDictionary.ContainsKey(key);
	}

	public bool ContainValue(TValue value)
	{
		return _attrDictionary.ContainsValue(value);
	}
}

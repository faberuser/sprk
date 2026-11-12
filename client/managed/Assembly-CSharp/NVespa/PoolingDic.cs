using System.Collections.Generic;

namespace NVespa;

public struct PoolingDic<TKey, TValue>
{
	private Dictionary<TKey, TValue> _dic;

	public Dictionary<TKey, TValue>.ValueCollection Values
	{
		get
		{
			if (_dic != null)
			{
				return _dic.Values;
			}
			return NVespa.EmptyDicHolder<TKey, TValue>.Empty.Values;
		}
	}

	private Dictionary<TKey, TValue> _safeDic
	{
		get
		{
			if (_dic == null)
			{
				_dic = CreateDic();
			}
			return _dic;
		}
	}

	public int Count => __Extension_Method_ICollection__.COUNT(_dic);

	private static Dictionary<TKey, TValue> CreateDic()
	{
		return SingletonAuto<ReservePool<Dictionary<TKey, TValue>>>.instance.Create();
	}

	private static void ReleaseDic(Dictionary<TKey, TValue> dic)
	{
		ReservePoolUtil.ReleaseDictionary(dic);
	}

	public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
	{
		if (_dic != null)
		{
			return _dic.GetEnumerator();
		}
		return NVespa.EmptyDicHolder<TKey, TValue>.Empty.GetEnumerator();
	}

	public bool IsNullOrEmpty()
	{
		return __Extension_Method_ICollection__.IsNullOrEmpty(_dic);
	}

	public void Clear()
	{
		if (_dic != null)
		{
			_dic.Clear();
			ReleaseDic(_dic);
			_dic = null;
		}
	}

	public void Add(TKey key, TValue value)
	{
		_safeDic.Add(key, value);
	}

	public void Update(TKey key, TValue value)
	{
		if (key != null)
		{
			_safeDic[key] = value;
		}
	}

	public TValue GETVALUE(TKey key, TValue defaultValue)
	{
		return Vespa_Generic_IDictionary_Extension_Method.GETVALUE(_dic, key, defaultValue);
	}
}

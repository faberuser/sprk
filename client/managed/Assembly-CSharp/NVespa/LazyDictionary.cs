using System.Collections.Generic;
using JsonBin;
using LitJson;

namespace NVespa;

public struct LazyDictionary<TKey, TValue> where TKey : new() where TValue : new()
{
	private IEqualityComparer<TKey> _comparer;

	private Dictionary<TKey, TValue> _dic;

	private Dictionary<TKey, TValue> _safeDic
	{
		get
		{
			if (_dic == null)
			{
				if (_comparer != null)
				{
					_dic = new Dictionary<TKey, TValue>(_comparer);
				}
				else
				{
					_dic = new Dictionary<TKey, TValue>();
				}
			}
			return _dic;
		}
	}

	public int Count => __Extension_Method_ICollection__.COUNT(_dic);

	public LazyDictionary(IEqualityComparer<TKey> comparer)
	{
		_comparer = comparer;
		_dic = null;
	}

	public bool IsNullOrEmpty()
	{
		return __Extension_Method_ICollection__.IsNullOrEmpty(_dic);
	}

	public void Add(TKey key, TValue value)
	{
		_safeDic.Add(key, value);
	}

	public bool TryGetValue(TKey key, out TValue o_value)
	{
		if (_dic == null)
		{
			o_value = default(TValue);
			return false;
		}
		return _dic.TryGetValue(key, out o_value);
	}

	public TValue GETVALUE(TKey key, TValue defaultValue)
	{
		return Vespa_Generic_IDictionary_Extension_Method.GETVALUE(_dic, key, defaultValue);
	}

	public bool LoadFromJson(JsonRead json)
	{
		if (!json.IsArray())
		{
			return false;
		}
		if (json.Count == 0)
		{
			return false;
		}
		return _safeDic.LoadDictionaryFromJson(json);
	}

	public JsonData MakeJson()
	{
		return Json_Extension_Method_IDictionary.MakeJson(_dic);
	}
}

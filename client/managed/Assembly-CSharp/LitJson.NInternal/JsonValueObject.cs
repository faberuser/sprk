using System.Collections.Generic;
using NVespa;

namespace LitJson.NInternal;

public sealed class JsonValueObject : JsonData.IJsonValue
{
	private static class EmptyHolder
	{
		internal static IncreaseDic<string, JsonData> Empty;

		static EmptyHolder()
		{
			Empty = new IncreaseDic<string, JsonData>();
		}
	}

	private IncreaseDic<string, JsonData> _value;

	public int Count => __Extension_Method_ICollection__.COUNT(_value);

	private IncreaseDic<string, JsonData> _safeValue
	{
		get
		{
			if (_value == null)
			{
				_value = IncreaseDic<string, JsonData>.CreateFromReservePool();
			}
			return _value;
		}
	}

	public JsonType GetJsonType()
	{
		return JsonType.Object;
	}

	public bool EqualsJsonValue(JsonData.IJsonValue rhsValue)
	{
		JsonValueObject jsonValueObject = rhsValue as JsonValueObject;
		if (rhsValue == null)
		{
			return false;
		}
		bool flag = _value == null;
		bool flag2 = jsonValueObject._value == null;
		if (flag != flag2)
		{
			return false;
		}
		if (flag)
		{
			return true;
		}
		return _value.RawDic.Equals(jsonValueObject._value.RawDic);
	}

	public void ReleaseSelf()
	{
		Clear();
		SingletonAuto<JsonPool>.instance.EnqueueJsonValue(this);
	}

	private void Clear()
	{
		if (_value == null)
		{
			return;
		}
		for (int i = 0; i < _value.Count; i++)
		{
			JsonData valueByIndex = _value.GetValueByIndex(i);
			if (valueByIndex != null)
			{
				SingletonAuto<JsonPool>.instance.ReleaseJsonData(valueByIndex);
			}
		}
		_value.Clear();
		ReservePoolUtil.Release(_value);
		_value = null;
	}

	public void Add(string key, JsonData value)
	{
		_safeValue.AddAndOverride(key, value);
	}

	public void ToDictionary(Dictionary<string, JsonData> dic)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_value); i++)
		{
			KeyValuePair<string, JsonData> keyValuePairByIndex = _value.GetKeyValuePairByIndex(i);
			dic.Add(keyValuePairByIndex.Key, keyValuePairByIndex.Value);
		}
	}

	public bool ContainsKey(string key)
	{
		if (_value == null)
		{
			return false;
		}
		return _value.ContainsKey(key);
	}

	public JsonData GetValue(string key)
	{
		if (_value == null)
		{
			return null;
		}
		return _value.GETVALUE(key, null);
	}

	public IEnumerator<KeyValuePair<string, JsonData>> GetEnumerator()
	{
		IEnumerable<KeyValuePair<string, JsonData>> enumerable = ((_value == null) ? EmptyHolder.Empty : _value);
		return enumerable.GetEnumerator();
	}
}

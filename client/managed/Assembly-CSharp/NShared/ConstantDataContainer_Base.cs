using NShared.NJit;
using NShared.NUtil;

namespace NShared;

public abstract class ConstantDataContainer_Base : JitDataContainerDefault<string, ConstantData>
{
	private IndexableDictionay<string, int> _intCache = new IndexableDictionay<string, int>(128);

	private IndexableDictionay<string, bool> _boolCache = new IndexableDictionay<string, bool>(128);

	public string GetValue(string key, string defaultValue = "")
	{
		ConstantData data = GetData(key);
		if (data != null)
		{
			return data.Value;
		}
		return defaultValue;
	}

	public int GetValueInt(string key, int defaultValue = 0)
	{
		lock (_intCache)
		{
			if (_intCache.TryGetValue(key, out var value))
			{
				return value;
			}
			if (!int.TryParse(GetValue(key), out value))
			{
				value = defaultValue;
			}
			_intCache.Add(key, value);
			return value;
		}
	}

	public bool GetValueBool(string key)
	{
		lock (_boolCache)
		{
			if (_boolCache.TryGetValue(key, out var value))
			{
				return value;
			}
			value = ParseValueBool(key);
			_boolCache.Add(key, value);
			return value;
		}
	}

	private bool ParseValueBool(string key)
	{
		string value = GetValue(key);
		if (string.IsNullOrEmpty(value))
		{
			return false;
		}
		if (value == "0")
		{
			return false;
		}
		if (value.ToLower() == "false")
		{
			return false;
		}
		return true;
	}
}

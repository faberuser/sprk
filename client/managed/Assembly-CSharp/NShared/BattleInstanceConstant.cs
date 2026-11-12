using System;
using System.Collections.Generic;

namespace NShared;

public class BattleInstanceConstant
{
	private string[] _values;

	private Dictionary<string, string> _valueDictionary;

	public int MpRegenM => GetValueInt("MpRegenM");

	public int HpRegenM => GetValueInt("HpRegenM");

	public bool InfiniteTime => GetValueBool("InfiniteTime");

	public void Init(string[] values)
	{
		_values = values;
		_valueDictionary = ParseValue(values);
	}

	public string[] GetValues()
	{
		return _values;
	}

	private static Dictionary<string, string> ParseValue(string[] values)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		if (values == null)
		{
			return dictionary;
		}
		foreach (string text in values)
		{
			if (string.IsNullOrEmpty(text))
			{
				continue;
			}
			int num = text.IndexOf('=');
			if (num < 0)
			{
				continue;
			}
			string text2 = text.Substring(0, num).Trim();
			if (!string.IsNullOrEmpty(text2))
			{
				string value = text.Substring(num + 1).Trim();
				if (!string.IsNullOrEmpty(value))
				{
					dictionary.Remove(text2);
					dictionary.Add(text2, value);
				}
			}
		}
		return dictionary;
	}

	private string GetValue(string key)
	{
		if (_valueDictionary == null)
		{
			return null;
		}
		_valueDictionary.TryGetValue(key, out var value);
		return value;
	}

	private int GetValueInt(string key)
	{
		string value = GetValue(key);
		int result = 0;
		if (!string.IsNullOrEmpty(value))
		{
			int.TryParse(value, out result);
		}
		return result;
	}

	private bool GetValueBool(string key)
	{
		return string.Equals(GetValue(key), "TRUE", StringComparison.OrdinalIgnoreCase);
	}
}

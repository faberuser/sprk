using NVespa;

namespace NShared;

[JsonSerializable(Javascript = true)]
public class ContentsValueInfo : IAccountAttrKey<string>
{
	string IAccountAttrKey<string>.Key => Key;

	public string Key { get; set; }

	public string Value { get; set; }

	public int GetIntValue(int defaultValue = 0)
	{
		int result = 0;
		if (!int.TryParse(Value.Replace("\"", ""), out result))
		{
			return defaultValue;
		}
		return result;
	}

	public bool GetBoolValue(bool defaultValue = false)
	{
		bool result = false;
		if (!bool.TryParse(Value.Replace("\"", ""), out result))
		{
			return defaultValue;
		}
		return result;
	}

	public T GetEnumValue<T>(T defaultValue)
	{
		if (!typeof(T).IsEnum)
		{
			return defaultValue;
		}
		if (!EnumLookup<T>.TryParse(Value.Replace("\"", ""), out var o_value))
		{
			return defaultValue;
		}
		return o_value;
	}

	public string[] GetStringArray()
	{
		if (string.IsNullOrEmpty(Value))
		{
			return new string[0];
		}
		return Value.Split(',');
	}
}

using System;
using System.Collections.Generic;

namespace NShared;

public class ArgParser
{
	public static bool HasValue<TValue>(IList<TValue> values, int index)
	{
		if (values != null)
		{
			return index < values.Count;
		}
		return false;
	}

	public static int GetValueInt<TValue>(IList<TValue> values, int index, int defaultValue = 0)
	{
		if (typeof(TValue) == typeof(int))
		{
			if (values == null || index >= values.Count)
			{
				return defaultValue;
			}
			return (int)Convert.ChangeType(values[index], typeof(int));
		}
		string valueString = GetValueString(values, index);
		int result = 0;
		if (int.TryParse(valueString, out result))
		{
			return result;
		}
		return defaultValue;
	}

	public static long GetValueLong<TValue>(IList<TValue> values, int index, long defaultValue = 0L)
	{
		if (typeof(TValue) == typeof(long))
		{
			if (values == null || index >= values.Count)
			{
				return defaultValue;
			}
			return (long)Convert.ChangeType(values[index], typeof(long));
		}
		string valueString = GetValueString(values, index);
		long result = 0L;
		if (long.TryParse(valueString, out result))
		{
			return result;
		}
		return defaultValue;
	}

	public static bool HasValueInt<TValue>(IList<TValue> values, int index, int value)
	{
		return HasValueInt(GetValueString(values, index), value);
	}

	public static bool HasValueInt(string stringValue, int value)
	{
		if (string.IsNullOrEmpty(stringValue))
		{
			return false;
		}
		string[] array = stringValue.Split(new char[1] { ':' });
		if (array == null || array.Length == 0)
		{
			return false;
		}
		string text = value.ToString();
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] == text)
			{
				return true;
			}
		}
		return false;
	}

	public static string GetValueString<TValue>(IList<TValue> values, int index)
	{
		if (values == null || index >= values.Count || values[index] == null)
		{
			return string.Empty;
		}
		return values[index].ToString().Trim();
	}

	public static bool GetValueBool<TValue>(IList<TValue> values, int index, bool defaultValue = false)
	{
		string valueString = GetValueString(values, index);
		if (string.IsNullOrEmpty(valueString))
		{
			return defaultValue;
		}
		return string.Equals(valueString, "true", StringComparison.OrdinalIgnoreCase);
	}

	public static TEnum GetValueEnum<TValue, TEnum>(IList<TValue> values, int index, TEnum defaultValue = default(TEnum)) where TEnum : struct
	{
		string valueString = GetValueString(values, index);
		if (string.IsNullOrEmpty(valueString))
		{
			return defaultValue;
		}
		try
		{
			return (TEnum)Enum.Parse(typeof(TEnum), valueString, ignoreCase: true);
		}
		catch (Exception)
		{
		}
		return default(TEnum);
	}

	public static TEnum GetValueEnum<TEnum>(IList<string> values, int index, TEnum defaultValue = default(TEnum)) where TEnum : struct
	{
		return ArgParser.GetValueEnum<string, TEnum>(values, index, defaultValue);
	}

	public static TEnum GetValueEnum<TEnum>(IList<HashString> values, int index, TEnum defaultValue = default(TEnum)) where TEnum : struct
	{
		return ArgParser.GetValueEnum<HashString, TEnum>(values, index, defaultValue);
	}

	public static TEnum[] GetValueEnums<TEnum>(IList<string> values, int index) where TEnum : struct
	{
		string valueString = GetValueString(values, index);
		if (string.IsNullOrEmpty(valueString))
		{
			return null;
		}
		string[] array = valueString.Split(new char[1] { '|' });
		TEnum[] array2 = new TEnum[array.Length];
		for (int i = 0; i < array.Length; i++)
		{
			try
			{
				array2[i] = EnumParser.Parse<TEnum>(array[i]);
			}
			catch (Exception)
			{
			}
		}
		return array2;
	}

	public static TFlag GetValueFlag<TFlag>(IList<string> values, int index, char parser = '|') where TFlag : struct
	{
		string valueString = GetValueString(values, index);
		if (string.IsNullOrEmpty(valueString))
		{
			return default(TFlag);
		}
		List<TFlag> list = new List<TFlag>();
		string[] array = valueString.Split(new char[1] { parser });
		for (int i = 0; i < array.Length; i++)
		{
			try
			{
				list.Add(EnumParser.Parse<TFlag>(array[i]));
			}
			catch (Exception)
			{
			}
		}
		if (list.Count == 0)
		{
			return default(TFlag);
		}
		int num = 0;
		for (int j = 0; j < list.Count; j++)
		{
			num |= (int)(object)list[j];
		}
		return (TFlag)(object)num;
	}
}

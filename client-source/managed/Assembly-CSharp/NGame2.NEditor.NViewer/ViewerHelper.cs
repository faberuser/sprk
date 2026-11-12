using System;
using System.Collections;
using System.Collections.Generic;

namespace NGame2.NEditor.NViewer;

public static class ViewerHelper
{
	private static Dictionary<Type, Func<object, object, int>> _dicComparer;

	public static object BaseConvert(string strValue, Type tType, object defaultValue)
	{
		if (string.IsNullOrEmpty(strValue))
		{
			return defaultValue;
		}
		if (tType.IsEnum)
		{
			return Enum.Parse(tType, strValue);
		}
		if (tType == typeof(string))
		{
			return strValue;
		}
		if (tType == typeof(DateTime))
		{
			if (DateTime.TryParse(strValue, out var result))
			{
				return result;
			}
		}
		else if (tType.IsValueType)
		{
			long result6;
			if (tType == typeof(bool))
			{
				if (bool.TryParse(strValue, out var result2))
				{
					return result2;
				}
			}
			else if (tType == typeof(char))
			{
				if (char.TryParse(strValue, out var result3))
				{
					return result3;
				}
			}
			else if (tType == typeof(float) || tType == typeof(double))
			{
				if (double.TryParse(strValue, out var result4))
				{
					return result4;
				}
			}
			else if (tType == typeof(byte) || tType == typeof(ushort) || tType == typeof(uint) || tType == typeof(ulong))
			{
				if (ulong.TryParse(strValue, out var result5))
				{
					return result5;
				}
			}
			else if ((tType == typeof(sbyte) || tType == typeof(short) || tType == typeof(int) || tType == typeof(long)) && long.TryParse(strValue, out result6))
			{
				return result6;
			}
		}
		return defaultValue;
	}

	private static object BaseConvert<T>(string strValue, T defaultValue = default(T))
	{
		Type typeFromHandle = typeof(T);
		return BaseConvert(strValue, typeFromHandle, defaultValue);
	}

	public static T GenericConver<T>(string strValue, T defaultValue = default(T))
	{
		return (T)Convert.ChangeType(BaseConvert(strValue, defaultValue), typeof(T));
	}

	public static int GenericCompareTo(object lhs, object rhs, Type type)
	{
		if (type.IsArray || typeof(IList).IsAssignableFrom(type) || typeof(IDictionary).IsAssignableFrom(type))
		{
			return 0;
		}
		return CompareTo(lhs, rhs, type);
	}

	private static int CompareTo(object lhs, object rhs, Type type)
	{
		Func<object, object, int> compareTo = GetCompareTo(type);
		if (compareTo == null)
		{
			return 0;
		}
		return Vespa_System_Action_Extension_Method.fire(compareTo, lhs, rhs);
	}

	private static Func<object, object, int> GetCompareTo(Type type)
	{
		InitDicCompareTo();
		_dicComparer.TryGetValue(type, out var value);
		return value;
	}

	private static void InitDicCompareTo()
	{
		if (_dicComparer == null)
		{
			_dicComparer = new Dictionary<Type, Func<object, object, int>>();
			RegistCompareToTValue<sbyte>(_dicComparer);
			RegistCompareToTValue<byte>(_dicComparer);
			RegistCompareToTValue<short>(_dicComparer);
			RegistCompareToTValue<ushort>(_dicComparer);
			RegistCompareToTValue<int>(_dicComparer);
			RegistCompareToTValue<uint>(_dicComparer);
			RegistCompareToTValue<long>(_dicComparer);
			RegistCompareToTValue<ulong>(_dicComparer);
			RegistCompareToTValue<float>(_dicComparer);
			RegistCompareToTValue<double>(_dicComparer);
			RegistCompareToTValue<string>(_dicComparer);
			RegistCompareToTValue<char>(_dicComparer);
			RegistCompareToTValue<DateTime>(_dicComparer);
			RegistCompareToObject<Enum>(_dicComparer);
		}
	}

	private static void RegistCompareToTValue<T>(Dictionary<Type, Func<object, object, int>> dic) where T : IComparable<T>
	{
		if (dic != null)
		{
			Type typeFromHandle = typeof(T);
			_dicComparer.Remove(typeFromHandle);
			_dicComparer.Add(typeFromHandle, CompareToTValue<T>);
		}
	}

	private static void RegistCompareToObject<T>(Dictionary<Type, Func<object, object, int>> dic) where T : IComparable
	{
		if (dic != null)
		{
			Type typeFromHandle = typeof(T);
			_dicComparer.Remove(typeFromHandle);
			_dicComparer.Add(typeFromHandle, CompareToObject<T>);
		}
	}

	private static int CompareToTValue<T>(object lhs, object rhs) where T : IComparable<T>
	{
		T lhs2 = (T)Convert.ChangeType(lhs, typeof(T));
		T val = (T)Convert.ChangeType(rhs, typeof(T));
		int num = CheckNullCompareTo(lhs2, val);
		if (num != 2)
		{
			return num;
		}
		return lhs2.CompareTo(val);
	}

	private static int CompareToObject<T>(object lhs, object rhs) where T : IComparable
	{
		T lhs2 = (T)Convert.ChangeType(lhs, typeof(T));
		T val = (T)Convert.ChangeType(rhs, typeof(T));
		int num = CheckNullCompareTo(lhs2, val);
		if (num != 2)
		{
			return num;
		}
		return lhs2.CompareTo(val);
	}

	private static int CheckNullCompareTo<T>(T lhs, T rhs)
	{
		if (lhs == null && rhs == null)
		{
			return 0;
		}
		if (lhs == null && rhs != null)
		{
			return 1;
		}
		if (rhs == null && lhs != null)
		{
			return -1;
		}
		if (lhs.Equals(rhs))
		{
			return 0;
		}
		return 2;
	}

	public static bool SearchEqual(object value, string searchStr)
	{
		if (value == null)
		{
			return false;
		}
		if (String_Extension_Method.IsNullOrEmpty(searchStr))
		{
			return true;
		}
		return string.Equals(value.ToString(), searchStr, StringComparison.OrdinalIgnoreCase);
	}

	public static bool SearchIndexOf(object value, string searchStr)
	{
		if (value == null)
		{
			return false;
		}
		if (String_Extension_Method.IsNullOrEmpty(searchStr))
		{
			return true;
		}
		return value.ToString().IndexOf(searchStr, StringComparison.OrdinalIgnoreCase) >= 0;
	}
}

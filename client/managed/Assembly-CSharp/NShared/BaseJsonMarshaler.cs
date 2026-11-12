using System;
using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NVespa;

namespace NShared;

public class BaseJsonMarshaler
{
	public static class TypeCaster<T>
	{
		private enum ECastType
		{
			IsString,
			IsClass,
			IsEnum,
			IsBool,
			Etc
		}

		private static ECastType _castType;

		static TypeCaster()
		{
			if (typeof(T) == typeof(string))
			{
				_castType = ECastType.IsString;
			}
			else if (typeof(T).IsClass || typeof(T).IsInterface)
			{
				_castType = ECastType.IsClass;
			}
			else if (typeof(T).IsEnum)
			{
				_castType = ECastType.IsEnum;
			}
			else if (typeof(T) == typeof(bool))
			{
				_castType = ECastType.IsBool;
			}
			else
			{
				_castType = ECastType.Etc;
			}
		}

		public static T To(object value)
		{
			if (value == null)
			{
				return default(T);
			}
			try
			{
				if (_castType == ECastType.IsString)
				{
					return (T)Convert.ChangeType(CastToString(value), typeof(T));
				}
				if (_castType == ECastType.IsClass)
				{
					return (T)value;
				}
				if (_castType == ECastType.IsEnum)
				{
					if (value == null)
					{
						return default(T);
					}
					string text = CastToString(value);
					text = text.Trim();
					if (string.IsNullOrEmpty(text) || string.Equals(text, default(T).ToString(), StringComparison.OrdinalIgnoreCase))
					{
						return default(T);
					}
					try
					{
						return EnumParser.Parse<T>(text);
					}
					catch (Exception)
					{
						return default(T);
					}
				}
				if (_castType == ECastType.IsBool)
				{
					return (T)Convert.ChangeType(value != null && string.Equals(CastToString(value), "TRUE", StringComparison.OrdinalIgnoreCase), typeof(T));
				}
				return (T)Convert.ChangeType(value, typeof(T));
			}
			catch (Exception)
			{
				return default(T);
			}
		}
	}

	public delegate T ParserDelegate<T>(IDictionary dictionary);

	public delegate Dictionary<string, object> EncoderDelegate<T>(T value);

	public static bool IsNumericType(Type type)
	{
		if (type == null)
		{
			return false;
		}
		switch (Type.GetTypeCode(type))
		{
		case TypeCode.SByte:
		case TypeCode.Byte:
		case TypeCode.Int16:
		case TypeCode.UInt16:
		case TypeCode.Int32:
		case TypeCode.UInt32:
		case TypeCode.Int64:
		case TypeCode.UInt64:
		case TypeCode.Single:
		case TypeCode.Double:
		case TypeCode.Decimal:
			return true;
		case TypeCode.Object:
			if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
			{
				return IsNumericType(Nullable.GetUnderlyingType(type));
			}
			return false;
		default:
			return false;
		}
	}

	public static int ToInt(string value)
	{
		int.TryParse(value, out var result);
		return result;
	}

	public static long ToLong(string value)
	{
		long.TryParse(value, out var result);
		return result;
	}

	public static short ToShort(string value)
	{
		short.TryParse(value, out var result);
		return result;
	}

	public static T To<T>(object value)
	{
		if (value == null)
		{
			return default(T);
		}
		try
		{
			if (typeof(T) == typeof(string))
			{
				return (T)Convert.ChangeType(CastToString(value), typeof(T));
			}
			if (typeof(T).IsClass || typeof(T).IsInterface)
			{
				return (T)value;
			}
			if (typeof(T).IsEnum)
			{
				if (value == null)
				{
					return default(T);
				}
				string text = CastToString(value);
				text = text.Trim();
				if (string.IsNullOrEmpty(text) || string.Equals(text, default(T).ToString(), StringComparison.OrdinalIgnoreCase))
				{
					return default(T);
				}
				try
				{
					return EnumParser.Parse<T>(text);
				}
				catch (Exception)
				{
					return default(T);
				}
			}
			if (typeof(T) == typeof(bool))
			{
				return (T)Convert.ChangeType(value != null && string.Equals(CastToString(value), "TRUE", StringComparison.OrdinalIgnoreCase), typeof(T));
			}
			return (T)Convert.ChangeType(value, typeof(T));
		}
		catch (Exception)
		{
			return default(T);
		}
	}

	public static IList ParseList(IDictionary dictionary, string key)
	{
		if (dictionary != null && dictionary.Contains(key))
		{
			return dictionary[key] as IList;
		}
		return null;
	}

	public static T[] ParseArray<T>(IDictionary dictionary, string key, ParserDelegate<T> parser) where T : class
	{
		IList list = ParseList(dictionary, key);
		if (list == null)
		{
			return null;
		}
		int count = list.Count;
		int num = 0;
		for (int i = 0; i < count; i++)
		{
			if (list[i] is IDictionary)
			{
				num++;
			}
		}
		T[] array = new T[num];
		int num2 = 0;
		for (int j = 0; j < count; j++)
		{
			if (list[j] is IDictionary dictionary2)
			{
				array[num2] = parser(dictionary2);
				num2++;
			}
		}
		return array;
	}

	public static T[] ParseArray2<T>(IDictionary dictionary, string key, ParserDelegate<T> parser) where T : class
	{
		List<T> list = new List<T>();
		IList list2 = ParseList(dictionary, key);
		if (list2 == null)
		{
			return null;
		}
		foreach (object item2 in list2)
		{
			if (item2 is IDictionary dictionary2)
			{
				T item = parser(dictionary2);
				list.Add(item);
			}
		}
		return list.ToArray();
	}

	public static T[] ParseArrayCast<T>(IDictionary dictionary, string key, Func<object, T> parser)
	{
		IList list = ParseList(dictionary, key);
		if (list == null)
		{
			return null;
		}
		int count = list.Count;
		T[] array = new T[count];
		for (int i = 0; i < count; i++)
		{
			T val = parser(list[i]);
			array[i] = val;
		}
		return array;
	}

	public static T[] ParseArray<T>(IDictionary dictionary, string key)
	{
		IList list = ParseList(dictionary, key);
		if (list == null)
		{
			return null;
		}
		int count = list.Count;
		T[] array = new T[count];
		for (int i = 0; i < count; i++)
		{
			T val = To<T>(list[i]);
			array[i] = val;
		}
		return array;
	}

	public static T[] ParseArray<T>(string value, Func<IDictionary, T> parser) where T : class
	{
		try
		{
			if (!(Json.Deserialize(value.Trim()) is List<object> list))
			{
				return new T[0];
			}
			List<T> list2 = new List<T>();
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i] is IDictionary arg)
				{
					T val = parser(arg);
					if (val != null)
					{
						list2.Add(val);
					}
				}
			}
			return list2.ToArray();
		}
		catch (Exception)
		{
		}
		return new T[0];
	}

	public static Dictionary<string, object> ParseDictionaryInternal(IDictionary dictionary, string key)
	{
		if (dictionary != null && dictionary.Contains(key))
		{
			return dictionary[key] as Dictionary<string, object>;
		}
		return null;
	}

	public static D ParseDictionary<K, V, D>(IDictionary dictionary, string key, ParserDelegate<V> parser) where V : class where D : IDictionary, new()
	{
		D result = new D();
		Dictionary<string, object> dictionary2 = ParseDictionaryInternal(dictionary, key);
		if (dictionary2 == null)
		{
			return default(D);
		}
		foreach (KeyValuePair<string, object> item in dictionary2)
		{
			K val = To<K>(item.Key);
			V val2 = parser(item.Value as IDictionary);
			if (val != null && val2 != null)
			{
				object key2 = val;
				result.Remove(key2);
				object key3 = val;
				result.Add(key3, val2);
			}
		}
		return result;
	}

	public static Dictionary<K, V> ParseDictionary<K, V>(IDictionary dictionary, string key)
	{
		Dictionary<K, V> dictionary2 = new Dictionary<K, V>();
		Dictionary<string, object> dictionary3 = ParseDictionaryInternal(dictionary, key);
		if (dictionary3 == null)
		{
			return null;
		}
		foreach (KeyValuePair<string, object> item in dictionary3)
		{
			K val = To<K>(item.Key);
			V val2 = To<V>(item.Value);
			if (val != null && val2 != null)
			{
				dictionary2.Remove(val);
				dictionary2.Add(val, val2);
			}
		}
		return dictionary2;
	}

	public static List<object> ParseListInternal(IDictionary dictionary, string key)
	{
		if (dictionary != null && dictionary.Contains(key))
		{
			return dictionary[key] as List<object>;
		}
		return null;
	}

	public static D ParseList<V, D>(IDictionary dictionary, string key, ParserDelegate<V> parser) where V : class where D : IList, new()
	{
		D result = new D();
		List<object> list = ParseListInternal(dictionary, key);
		if (list == null)
		{
			return default(D);
		}
		foreach (object item in list)
		{
			V val = parser(item as IDictionary);
			if (val != null)
			{
				result.Add(val);
			}
		}
		return result;
	}

	public static List<V> ParseList<K, V>(IDictionary dictionary, string key)
	{
		List<V> list = new List<V>();
		List<object> list2 = ParseListInternal(dictionary, key);
		if (list2 == null)
		{
			return null;
		}
		foreach (object item in list2)
		{
			V val = To<V>(item);
			if (val != null)
			{
				list.Add(val);
			}
		}
		return list;
	}

	public static string CastToString(object value)
	{
		if (value == null)
		{
			return null;
		}
		if (value is string)
		{
			return value as string;
		}
		return value.ToString();
	}

	public static HashString CastToHashString(object value)
	{
		return CastToString(value);
	}

	public static long CastToLong(object value)
	{
		if (value == null)
		{
			return 0L;
		}
		if (value is long)
		{
			return (long)value;
		}
		if (value is double)
		{
			return (long)(double)value;
		}
		if (value is string)
		{
			long result = 0L;
			long.TryParse(value as string, out result);
			return result;
		}
		VespaDebug.LogError("CastToLong : type=" + value.GetType());
		return 0L;
	}

	public static int CastToInt(object value)
	{
		return (int)CastToLong(value);
	}

	public static uint CastToUInt(object value)
	{
		return (uint)CastToLong(value);
	}

	public static short CastToShort(object value)
	{
		return (short)CastToLong(value);
	}

	public static ushort CastToUShort(object value)
	{
		return (ushort)CastToLong(value);
	}

	public static byte CastToByte(object value)
	{
		return (byte)CastToLong(value);
	}

	public static sbyte CastToSByte(object value)
	{
		return (sbyte)CastToLong(value);
	}

	public static bool CastToBool(object value)
	{
		if (value == null)
		{
			return false;
		}
		if (value is bool)
		{
			return (bool)value;
		}
		string empty = string.Empty;
		empty = ((!(value is string)) ? value.ToString() : (value as string));
		return string.Equals(empty, "true", StringComparison.OrdinalIgnoreCase);
	}

	public static float CastToFloat(object value)
	{
		if (value == null)
		{
			return 0f;
		}
		if (value is float)
		{
			return (float)value;
		}
		if (value is double)
		{
			return (float)(double)value;
		}
		if (value is long)
		{
			return (long)value;
		}
		if (value is string)
		{
			double result = 0.0;
			double.TryParse(value as string, out result);
			return (float)result;
		}
		VespaDebug.LogError("CastToFloat : type=" + value.GetType());
		return 0f;
	}

	public static T ParseObject<T>(IDictionary dictionary, string key, ParserDelegate<T> parser)
	{
		if (dictionary != null && dictionary.Contains(key))
		{
			if (!(dictionary[key] is IDictionary dictionary2))
			{
				return default(T);
			}
			return parser(dictionary2);
		}
		return default(T);
	}

	public static T ParseValue<T>(IDictionary dictionary, string key)
	{
		if (dictionary != null && dictionary.Contains(key))
		{
			return To<T>(dictionary[key]);
		}
		return default(T);
	}

	public static string ParseValueString(IDictionary dictionary, string key)
	{
		if (dictionary != null && dictionary.Contains(key))
		{
			return CastToString(dictionary[key]);
		}
		return null;
	}

	public static HashString ParseValueHashString(IDictionary dictionary, string key)
	{
		if (dictionary != null && dictionary.Contains(key))
		{
			return CastToString(dictionary[key]);
		}
		return null;
	}

	public static long ParseValueLong(IDictionary dictionary, string key)
	{
		if (dictionary != null && dictionary.Contains(key))
		{
			return CastToLong(dictionary[key]);
		}
		return 0L;
	}

	public static int ParseValueInt(IDictionary dictionary, string key)
	{
		if (dictionary != null && dictionary.Contains(key))
		{
			return CastToInt(dictionary[key]);
		}
		return 0;
	}

	public static uint ParseValueUInt(IDictionary dictionary, string key)
	{
		if (dictionary != null && dictionary.Contains(key))
		{
			return CastToUInt(dictionary[key]);
		}
		return 0u;
	}

	public static short ParseValueShort(IDictionary dictionary, string key)
	{
		if (dictionary != null && dictionary.Contains(key))
		{
			return CastToShort(dictionary[key]);
		}
		return 0;
	}

	public static ushort ParseValueUShort(IDictionary dictionary, string key)
	{
		if (dictionary != null && dictionary.Contains(key))
		{
			return CastToUShort(dictionary[key]);
		}
		return 0;
	}

	public static byte ParseValueByte(IDictionary dictionary, string key)
	{
		if (dictionary != null && dictionary.Contains(key))
		{
			return CastToByte(dictionary[key]);
		}
		return 0;
	}

	public static sbyte ParseValueSByte(IDictionary dictionary, string key)
	{
		if (dictionary != null && dictionary.Contains(key))
		{
			return CastToSByte(dictionary[key]);
		}
		return 0;
	}

	public static bool ParseValueBool(IDictionary dictionary, string key)
	{
		if (dictionary != null && dictionary.Contains(key))
		{
			return CastToBool(dictionary[key]);
		}
		return false;
	}

	public static T ParseEnum<T>(IDictionary dictionary, string key) where T : struct
	{
		if (dictionary != null && dictionary.Contains(key))
		{
			string text = dictionary[key] as string;
			if (!string.IsNullOrEmpty(text))
			{
				try
				{
					return EnumParser.Parse<T>(text.Replace('|', ','));
				}
				catch (Exception)
				{
					return default(T);
				}
			}
		}
		return default(T);
	}

	public static float ParseValueFloat(IDictionary dictionary, string key)
	{
		if (dictionary != null && dictionary.Contains(key))
		{
			return CastToFloat(dictionary[key]);
		}
		return 0f;
	}

	public static List<object> EncodeArray<T>(IList<T> array)
	{
		List<object> list = JsonPool.instance.CreateList();
		if (array != null && array.Count > 0)
		{
			foreach (T item in array)
			{
				list.Add(item.ToString());
			}
		}
		return list;
	}

	public static List<object> EncodeArray<T>(IList<T> array, EncoderDelegate<T> encoder)
	{
		List<object> list = JsonPool.instance.CreateList();
		if (array != null)
		{
			foreach (T item in array)
			{
				list.Add(encoder(item));
			}
		}
		return list;
	}

	public static Dictionary<string, object> EncodeDictionary<K, V>(Dictionary<K, V> dictionary)
	{
		Dictionary<string, object> dictionary2 = new Dictionary<string, object>();
		if (dictionary != null)
		{
			foreach (KeyValuePair<K, V> item in dictionary)
			{
				dictionary2.Add(item.Key.ToString(), item.Value);
			}
		}
		return dictionary2;
	}

	public static Dictionary<string, object> EncodeDictionary<K, V>(IDictionary<K, V> dictionary, EncoderDelegate<V> encoder)
	{
		Dictionary<string, object> dictionary2 = new Dictionary<string, object>();
		if (dictionary != null)
		{
			foreach (KeyValuePair<K, V> item in dictionary)
			{
				dictionary2.Add(item.Key.ToString(), encoder(item.Value));
			}
		}
		return dictionary2;
	}
}

using System;
using LitJson;

public static class LitJson_Extension_Method
{
	public static JsonData GetJsonAny(this JsonData This, string arg0, string arg1 = null, string arg2 = null)
	{
		if (This == null)
		{
			return null;
		}
		JsonData jsonData = This[arg0];
		if (jsonData != null)
		{
			return jsonData;
		}
		jsonData = This[arg1];
		if (jsonData != null)
		{
			return jsonData;
		}
		jsonData = This[arg2];
		if (jsonData != null)
		{
			return jsonData;
		}
		return null;
	}

	public static string ToJson(this JsonData i_json, bool i_prettyPrint = false)
	{
		return i_json.ToJson(0, i_prettyPrint);
	}

	public static string ToJson(this JsonData i_json, int capacity, bool i_prettyPrint = false)
	{
		if (i_json == null)
		{
			return new JsonData().internalToJson(capacity, i_prettyPrint);
		}
		return i_json.internalToJson(capacity, i_prettyPrint);
	}

	public static bool isNone(this JsonData i_json)
	{
		if (i_json == null)
		{
			return true;
		}
		if (i_json.GetJsonType() == JsonType.None)
		{
			return true;
		}
		return false;
	}

	public static bool IsArray(this JsonData i_json)
	{
		return IsJsonType(i_json, JsonType.Array);
	}

	public static bool IsBoolean(this JsonData i_json)
	{
		return IsJsonType(i_json, JsonType.Boolean);
	}

	public static bool IsDouble(this JsonData i_json)
	{
		return IsJsonType(i_json, JsonType.Double);
	}

	public static bool IsLong(this JsonData i_json)
	{
		return IsJsonType(i_json, JsonType.Long);
	}

	public static bool IsObject(this JsonData i_json)
	{
		return IsJsonType(i_json, JsonType.Object);
	}

	public static bool IsString(this JsonData i_json)
	{
		return IsJsonType(i_json, JsonType.String);
	}

	public static bool TryGetBoolean(this JsonData i_json, out bool o_value)
	{
		if (i_json.IsBoolean())
		{
			o_value = i_json.valueBool;
			return true;
		}
		if (i_json.IsString())
		{
			return bool.TryParse(i_json.valueStr, out o_value);
		}
		o_value = false;
		return false;
	}

	public static bool GETBOOLEAN(this JsonData i_json, bool i_default)
	{
		if (!i_json.TryGetBoolean(out var o_value))
		{
			return i_default;
		}
		return o_value;
	}

	public static bool TryGetFloat(this JsonData i_json, out float o_value)
	{
		if (i_json.IsDouble())
		{
			o_value = (float)i_json.valueDouble;
			return true;
		}
		if (i_json.IsString())
		{
			return float.TryParse(i_json.valueStr, out o_value);
		}
		o_value = 0f;
		return false;
	}

	public static float GETFLOAT(this JsonData i_json, float i_default)
	{
		if (!i_json.TryGetFloat(out var o_value))
		{
			return i_default;
		}
		return o_value;
	}

	public static bool TryGetDouble(this JsonData i_json, out double o_value)
	{
		if (i_json.IsDouble())
		{
			o_value = i_json.valueDouble;
			return true;
		}
		if (i_json.IsString())
		{
			return double.TryParse(i_json.valueStr, out o_value);
		}
		o_value = 0.0;
		return false;
	}

	public static double GETDOUBLE(this JsonData i_json, double i_default)
	{
		if (!i_json.TryGetDouble(out var o_value))
		{
			return i_default;
		}
		return o_value;
	}

	public static bool TryGetInt(this JsonData i_json, out int o_value)
	{
		if (i_json.IsLong())
		{
			o_value = i_json.valueInt;
			return true;
		}
		if (i_json.IsString())
		{
			return int.TryParse(i_json.valueStr, out o_value);
		}
		o_value = 0;
		return false;
	}

	public static int GETINT(this JsonData i_json, int i_default)
	{
		if (!i_json.TryGetInt(out var o_value))
		{
			return i_default;
		}
		return o_value;
	}

	public static bool TryGetLong(this JsonData i_json, out long o_value)
	{
		if (i_json.IsLong())
		{
			o_value = i_json.valueLong;
			return true;
		}
		if (i_json.IsString())
		{
			return long.TryParse(i_json.valueStr, out o_value);
		}
		o_value = 0L;
		return false;
	}

	public static long GETLONG(this JsonData i_json, long i_default)
	{
		if (!i_json.TryGetLong(out var o_value))
		{
			return i_default;
		}
		return o_value;
	}

	public static ulong GETULONG(this JsonData i_json, ulong i_default)
	{
		if (!i_json.TryGetString(out var o_value))
		{
			return i_default;
		}
		return Vespa_String_Extension_Method.TOULONG(o_value, i_default);
	}

	public static bool TryGetString(this JsonData i_json, out string o_value)
	{
		if (!i_json.IsString())
		{
			o_value = string.Empty;
			return false;
		}
		o_value = i_json.valueStr;
		return true;
	}

	public static string GETSTRING(this JsonData i_json, string i_default)
	{
		if (!i_json.TryGetString(out var o_value))
		{
			return i_default;
		}
		return o_value;
	}

	public static bool TryGetEnum<EnumType>(this JsonData i_json, out EnumType o_value)
	{
		if (!i_json.IsString())
		{
			o_value = default(EnumType);
			return false;
		}
		return Vespa_String_Extension_Method.TryToEnum<EnumType>(i_json.valueStr, out o_value);
	}

	public static EnumType GETENUM<EnumType>(this JsonData i_json, EnumType i_default)
	{
		if (!i_json.TryGetEnum<EnumType>(out var o_value))
		{
			return i_default;
		}
		return o_value;
	}

	public static bool TryGetDateTime(this JsonData i_json, out DateTime o_value)
	{
		if (!i_json.IsString())
		{
			o_value = DateTime.MinValue;
			return false;
		}
		return DateTime.TryParse(i_json.valueStr, out o_value);
	}

	public static DateTime GETDATETIME(this JsonData i_json, DateTime i_default)
	{
		if (!i_json.TryGetDateTime(out var o_value))
		{
			return i_default;
		}
		return o_value;
	}

	private static bool IsJsonType(JsonData i_json, JsonType i_type)
	{
		if (i_json == null)
		{
			return false;
		}
		if (i_json.GetJsonType() == i_type)
		{
			return true;
		}
		return false;
	}
}

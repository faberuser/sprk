using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_LuaPointResultInfo
{
	public static LuaPointResultInfo Parse(string s)
	{
		return Parse(s, new LuaPointResultInfo());
	}

	public static LuaPointResultInfo Parse(string s, LuaPointResultInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static LuaPointResultInfo Parse(IDictionary d)
	{
		return Parse(d, new LuaPointResultInfo());
	}

	public static LuaPointResultInfo Parse(IDictionary d, LuaPointResultInfo o)
	{
		o.AddValue = BaseJsonMarshaler.ParseValueLong(d, "AddValue");
		o.NewValue = BaseJsonMarshaler.ParseValueLong(d, "NewValue");
		return o;
	}

	public static string EncodeJsonString(LuaPointResultInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(LuaPointResultInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("AddValue", o.AddValue.ToString());
		dictionary.Add("NewValue", o.NewValue.ToString());
		return dictionary;
	}
}

using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_OrdealArenaStatDataBase
{
	public static OrdealArenaStatDataBase Parse(string s)
	{
		return Parse(s, new OrdealArenaStatDataBase());
	}

	public static OrdealArenaStatDataBase Parse(string s, OrdealArenaStatDataBase o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static OrdealArenaStatDataBase Parse(IDictionary d)
	{
		return Parse(d, new OrdealArenaStatDataBase());
	}

	public static OrdealArenaStatDataBase Parse(IDictionary d, OrdealArenaStatDataBase o)
	{
		o.OptionType = BaseJsonMarshaler.ParseEnum<OperationType>(d, "OptionType");
		o.OptionValue = BaseJsonMarshaler.ParseValueInt(d, "OptionValue");
		o.Index = BaseJsonMarshaler.ParseValueInt(d, "Index");
		return o;
	}

	public static string EncodeJsonString(OrdealArenaStatDataBase o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(OrdealArenaStatDataBase o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("OptionType", o.OptionType.ToString());
		dictionary.Add("OptionValue", o.OptionValue.ToString());
		dictionary.Add("Index", o.Index.ToString());
		return dictionary;
	}
}

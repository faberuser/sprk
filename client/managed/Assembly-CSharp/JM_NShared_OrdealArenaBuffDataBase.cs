using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_OrdealArenaBuffDataBase
{
	public static OrdealArenaBuffDataBase Parse(string s)
	{
		return Parse(s, new OrdealArenaBuffDataBase());
	}

	public static OrdealArenaBuffDataBase Parse(string s, OrdealArenaBuffDataBase o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static OrdealArenaBuffDataBase Parse(IDictionary d)
	{
		return Parse(d, new OrdealArenaBuffDataBase());
	}

	public static OrdealArenaBuffDataBase Parse(IDictionary d, OrdealArenaBuffDataBase o)
	{
		o.Index = BaseJsonMarshaler.ParseValueInt(d, "Index");
		return o;
	}

	public static string EncodeJsonString(OrdealArenaBuffDataBase o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(OrdealArenaBuffDataBase o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Index", o.Index.ToString());
		return dictionary;
	}
}

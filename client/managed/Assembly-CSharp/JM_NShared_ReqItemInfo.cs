using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_ReqItemInfo
{
	public static ReqItemInfo Parse(string s)
	{
		return Parse(s, new ReqItemInfo());
	}

	public static ReqItemInfo Parse(string s, ReqItemInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static ReqItemInfo Parse(IDictionary d)
	{
		return Parse(d, new ReqItemInfo());
	}

	public static ReqItemInfo Parse(IDictionary d, ReqItemInfo o)
	{
		o.Code = BaseJsonMarshaler.ParseValueString(d, "Code");
		o.Count = BaseJsonMarshaler.ParseValueInt(d, "Count");
		return o;
	}

	public static string EncodeJsonString(ReqItemInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(ReqItemInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.Code != null)
		{
			dictionary.Add("Code", o.Code);
		}
		dictionary.Add("Count", o.Count.ToString());
		return dictionary;
	}
}

using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.ExtendRunePage;

public static class JM_NShared_ExtendRunePage_Request
{
	public static NShared.ExtendRunePage.Request Parse(string s)
	{
		return Parse(s, new NShared.ExtendRunePage.Request());
	}

	public static NShared.ExtendRunePage.Request Parse(string s, NShared.ExtendRunePage.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.ExtendRunePage.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.ExtendRunePage.Request());
	}

	public static NShared.ExtendRunePage.Request Parse(IDictionary d, NShared.ExtendRunePage.Request o)
	{
		o.HeroIndex = BaseJsonMarshaler.ParseValueInt(d, "HeroIndex");
		o.RunePage = BaseJsonMarshaler.ParseValueInt(d, "RunePage");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.ExtendRunePage.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.ExtendRunePage.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("HeroIndex", o.HeroIndex.ToString());
		dictionary.Add("RunePage", o.RunePage.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}

using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.GetGlobalMailList;

public static class JM_NShared_GetGlobalMailList_Request
{
	public static NShared.GetGlobalMailList.Request Parse(string s)
	{
		return Parse(s, new NShared.GetGlobalMailList.Request());
	}

	public static NShared.GetGlobalMailList.Request Parse(string s, NShared.GetGlobalMailList.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.GetGlobalMailList.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.GetGlobalMailList.Request());
	}

	public static NShared.GetGlobalMailList.Request Parse(IDictionary d, NShared.GetGlobalMailList.Request o)
	{
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.GetGlobalMailList.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.GetGlobalMailList.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}

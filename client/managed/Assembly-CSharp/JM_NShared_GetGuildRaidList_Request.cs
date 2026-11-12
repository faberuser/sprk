using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.GetGuildRaidList;

public static class JM_NShared_GetGuildRaidList_Request
{
	public static NShared.GetGuildRaidList.Request Parse(string s)
	{
		return Parse(s, new NShared.GetGuildRaidList.Request());
	}

	public static NShared.GetGuildRaidList.Request Parse(string s, NShared.GetGuildRaidList.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.GetGuildRaidList.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.GetGuildRaidList.Request());
	}

	public static NShared.GetGuildRaidList.Request Parse(IDictionary d, NShared.GetGuildRaidList.Request o)
	{
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.GetGuildRaidList.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.GetGuildRaidList.Request o)
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

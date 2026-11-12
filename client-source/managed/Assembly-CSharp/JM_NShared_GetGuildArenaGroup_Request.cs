using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.GetGuildArenaGroup;

public static class JM_NShared_GetGuildArenaGroup_Request
{
	public static NShared.GetGuildArenaGroup.Request Parse(string s)
	{
		return Parse(s, new NShared.GetGuildArenaGroup.Request());
	}

	public static NShared.GetGuildArenaGroup.Request Parse(string s, NShared.GetGuildArenaGroup.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.GetGuildArenaGroup.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.GetGuildArenaGroup.Request());
	}

	public static NShared.GetGuildArenaGroup.Request Parse(IDictionary d, NShared.GetGuildArenaGroup.Request o)
	{
		o.SessionIndex = BaseJsonMarshaler.ParseValueLong(d, "SessionIndex");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.GetGuildArenaGroup.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.GetGuildArenaGroup.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("SessionIndex", o.SessionIndex.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}

using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.GetGuildRaidMemberScoreList;

public static class JM_NShared_GetGuildRaidMemberScoreList_Request
{
	public static NShared.GetGuildRaidMemberScoreList.Request Parse(string s)
	{
		return Parse(s, new NShared.GetGuildRaidMemberScoreList.Request());
	}

	public static NShared.GetGuildRaidMemberScoreList.Request Parse(string s, NShared.GetGuildRaidMemberScoreList.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.GetGuildRaidMemberScoreList.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.GetGuildRaidMemberScoreList.Request());
	}

	public static NShared.GetGuildRaidMemberScoreList.Request Parse(IDictionary d, NShared.GetGuildRaidMemberScoreList.Request o)
	{
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.GetGuildRaidMemberScoreList.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.GetGuildRaidMemberScoreList.Request o)
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

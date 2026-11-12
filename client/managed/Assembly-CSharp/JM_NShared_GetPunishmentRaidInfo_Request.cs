using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.GetPunishmentRaidInfo;

public static class JM_NShared_GetPunishmentRaidInfo_Request
{
	public static NShared.GetPunishmentRaidInfo.Request Parse(string s)
	{
		return Parse(s, new NShared.GetPunishmentRaidInfo.Request());
	}

	public static NShared.GetPunishmentRaidInfo.Request Parse(string s, NShared.GetPunishmentRaidInfo.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.GetPunishmentRaidInfo.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.GetPunishmentRaidInfo.Request());
	}

	public static NShared.GetPunishmentRaidInfo.Request Parse(IDictionary d, NShared.GetPunishmentRaidInfo.Request o)
	{
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.GetPunishmentRaidInfo.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.GetPunishmentRaidInfo.Request o)
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

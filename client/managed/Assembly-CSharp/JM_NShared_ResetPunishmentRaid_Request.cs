using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.ResetPunishmentRaid;

public static class JM_NShared_ResetPunishmentRaid_Request
{
	public static NShared.ResetPunishmentRaid.Request Parse(string s)
	{
		return Parse(s, new NShared.ResetPunishmentRaid.Request());
	}

	public static NShared.ResetPunishmentRaid.Request Parse(string s, NShared.ResetPunishmentRaid.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.ResetPunishmentRaid.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.ResetPunishmentRaid.Request());
	}

	public static NShared.ResetPunishmentRaid.Request Parse(IDictionary d, NShared.ResetPunishmentRaid.Request o)
	{
		o.GroupIndex = BaseJsonMarshaler.ParseValueInt(d, "GroupIndex");
		o.DungeonType = BaseJsonMarshaler.ParseValueInt(d, "DungeonType");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.ResetPunishmentRaid.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.ResetPunishmentRaid.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("GroupIndex", o.GroupIndex.ToString());
		dictionary.Add("DungeonType", o.DungeonType.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}

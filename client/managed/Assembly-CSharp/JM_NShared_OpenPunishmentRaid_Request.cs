using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.OpenPunishmentRaid;

public static class JM_NShared_OpenPunishmentRaid_Request
{
	public static NShared.OpenPunishmentRaid.Request Parse(string s)
	{
		return Parse(s, new NShared.OpenPunishmentRaid.Request());
	}

	public static NShared.OpenPunishmentRaid.Request Parse(string s, NShared.OpenPunishmentRaid.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.OpenPunishmentRaid.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.OpenPunishmentRaid.Request());
	}

	public static NShared.OpenPunishmentRaid.Request Parse(IDictionary d, NShared.OpenPunishmentRaid.Request o)
	{
		o.GroupIndex = BaseJsonMarshaler.ParseValueInt(d, "GroupIndex");
		o.DungeonType = BaseJsonMarshaler.ParseValueInt(d, "DungeonType");
		o.Level = BaseJsonMarshaler.ParseValueInt(d, "Level");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.OpenPunishmentRaid.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.OpenPunishmentRaid.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("GroupIndex", o.GroupIndex.ToString());
		dictionary.Add("DungeonType", o.DungeonType.ToString());
		dictionary.Add("Level", o.Level.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}

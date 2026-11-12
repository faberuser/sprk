using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_OpenPunishmentRaidInfo
{
	public static OpenPunishmentRaidInfo Parse(string s)
	{
		return Parse(s, new OpenPunishmentRaidInfo());
	}

	public static OpenPunishmentRaidInfo Parse(string s, OpenPunishmentRaidInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static OpenPunishmentRaidInfo Parse(IDictionary d)
	{
		return Parse(d, new OpenPunishmentRaidInfo());
	}

	public static OpenPunishmentRaidInfo Parse(IDictionary d, OpenPunishmentRaidInfo o)
	{
		o.GroupIndex = BaseJsonMarshaler.ParseValueInt(d, "GroupIndex");
		o.DungeonType = BaseJsonMarshaler.ParseValueInt(d, "DungeonType");
		o.IsOpen = BaseJsonMarshaler.ParseValueInt(d, "IsOpen");
		o.OpenedTime = BaseJsonMarshaler.ParseValueString(d, "OpenedTime");
		o.ExpireTime = BaseJsonMarshaler.ParseValueString(d, "ExpireTime");
		o.OpenLevel = BaseJsonMarshaler.ParseValueInt(d, "OpenLevel");
		o.ClearLevel = BaseJsonMarshaler.ParseValueInt(d, "ClearLevel");
		o.ClearCount = BaseJsonMarshaler.ParseValueInt(d, "ClearCount");
		return o;
	}

	public static string EncodeJsonString(OpenPunishmentRaidInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(OpenPunishmentRaidInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("GroupIndex", o.GroupIndex.ToString());
		dictionary.Add("DungeonType", o.DungeonType.ToString());
		dictionary.Add("IsOpen", o.IsOpen.ToString());
		if (o.OpenedTime != null)
		{
			dictionary.Add("OpenedTime", o.OpenedTime);
		}
		if (o.ExpireTime != null)
		{
			dictionary.Add("ExpireTime", o.ExpireTime);
		}
		dictionary.Add("OpenLevel", o.OpenLevel.ToString());
		dictionary.Add("ClearLevel", o.ClearLevel.ToString());
		dictionary.Add("ClearCount", o.ClearCount.ToString());
		return dictionary;
	}
}

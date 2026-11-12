using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_PunishmentRaidInfo
{
	public static PunishmentRaidInfo Parse(string s)
	{
		return Parse(s, new PunishmentRaidInfo());
	}

	public static PunishmentRaidInfo Parse(string s, PunishmentRaidInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static PunishmentRaidInfo Parse(IDictionary d)
	{
		return Parse(d, new PunishmentRaidInfo());
	}

	public static PunishmentRaidInfo Parse(IDictionary d, PunishmentRaidInfo o)
	{
		o.RaidIndex = BaseJsonMarshaler.ParseValueInt(d, "RaidIndex");
		o.RaidLevel = BaseJsonMarshaler.ParseValueInt(d, "RaidLevel");
		o.ClearCheckIndex = BaseJsonMarshaler.ParseValueInt(d, "ClearCheckIndex");
		o.ClearedTime = BaseJsonMarshaler.ParseValueString(d, "ClearedTime");
		return o;
	}

	public static string EncodeJsonString(PunishmentRaidInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(PunishmentRaidInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("RaidIndex", o.RaidIndex.ToString());
		dictionary.Add("RaidLevel", o.RaidLevel.ToString());
		dictionary.Add("ClearCheckIndex", o.ClearCheckIndex.ToString());
		if (o.ClearedTime != null)
		{
			dictionary.Add("ClearedTime", o.ClearedTime);
		}
		return dictionary;
	}
}

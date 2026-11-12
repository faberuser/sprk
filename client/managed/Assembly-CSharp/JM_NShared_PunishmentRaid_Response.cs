using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.PunishmentRaid;

public static class JM_NShared_PunishmentRaid_Response
{
	public static NShared.PunishmentRaid.Response Parse(string s)
	{
		return Parse(s, new NShared.PunishmentRaid.Response());
	}

	public static NShared.PunishmentRaid.Response Parse(string s, NShared.PunishmentRaid.Response o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.PunishmentRaid.Response Parse(IDictionary d)
	{
		return Parse(d, new NShared.PunishmentRaid.Response());
	}

	public static NShared.PunishmentRaid.Response Parse(IDictionary d, NShared.PunishmentRaid.Response o)
	{
		o.GroupIndex = BaseJsonMarshaler.ParseValueInt(d, "GroupIndex");
		return o;
	}

	public static string EncodeJsonString(NShared.PunishmentRaid.Response o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.PunishmentRaid.Response o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("GroupIndex", o.GroupIndex.ToString());
		return dictionary;
	}
}

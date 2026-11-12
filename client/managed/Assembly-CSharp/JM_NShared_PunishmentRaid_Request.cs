using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.PunishmentRaid;

public static class JM_NShared_PunishmentRaid_Request
{
	public static NShared.PunishmentRaid.Request Parse(string s)
	{
		return Parse(s, new NShared.PunishmentRaid.Request());
	}

	public static NShared.PunishmentRaid.Request Parse(string s, NShared.PunishmentRaid.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.PunishmentRaid.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.PunishmentRaid.Request());
	}

	public static NShared.PunishmentRaid.Request Parse(IDictionary d, NShared.PunishmentRaid.Request o)
	{
		o.GroupIndex = BaseJsonMarshaler.ParseValueInt(d, "GroupIndex");
		o.DungeonType = BaseJsonMarshaler.ParseEnum<EPunishmentDungeonType>(d, "DungeonType");
		o.FlaskItemIndex = BaseJsonMarshaler.ParseValueInt(d, "FlaskItemIndex");
		o.FlaskItemCount = BaseJsonMarshaler.ParseValueInt(d, "FlaskItemCount");
		o.AffixIndices = BaseJsonMarshaler.ParseArrayCast(d, "AffixIndices", BaseJsonMarshaler.CastToInt);
		o.Level = BaseJsonMarshaler.ParseValueInt(d, "Level");
		return o;
	}

	public static string EncodeJsonString(NShared.PunishmentRaid.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.PunishmentRaid.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("GroupIndex", o.GroupIndex.ToString());
		dictionary.Add("DungeonType", o.DungeonType.ToString());
		dictionary.Add("FlaskItemIndex", o.FlaskItemIndex.ToString());
		dictionary.Add("FlaskItemCount", o.FlaskItemCount.ToString());
		dictionary.Add("AffixIndices", BaseJsonMarshaler.EncodeArray(o.AffixIndices));
		dictionary.Add("Level", o.Level.ToString());
		return dictionary;
	}
}

using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_GuildRaidBootyEquipItemInfo
{
	public static GuildRaidBootyEquipItemInfo Parse(string s)
	{
		return Parse(s, new GuildRaidBootyEquipItemInfo());
	}

	public static GuildRaidBootyEquipItemInfo Parse(string s, GuildRaidBootyEquipItemInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static GuildRaidBootyEquipItemInfo Parse(IDictionary d)
	{
		return Parse(d, new GuildRaidBootyEquipItemInfo());
	}

	public static GuildRaidBootyEquipItemInfo Parse(IDictionary d, GuildRaidBootyEquipItemInfo o)
	{
		o.Id = BaseJsonMarshaler.ParseValueLong(d, "Id");
		o.CreatedTime = BaseJsonMarshaler.ParseValueString(d, "CreatedTime");
		o.ItemIndex = BaseJsonMarshaler.ParseValueInt(d, "ItemIndex");
		o.Star = BaseJsonMarshaler.ParseValueInt(d, "Star");
		return o;
	}

	public static string EncodeJsonString(GuildRaidBootyEquipItemInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(GuildRaidBootyEquipItemInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Id", o.Id.ToString());
		if (o.CreatedTime != null)
		{
			dictionary.Add("CreatedTime", o.CreatedTime);
		}
		dictionary.Add("ItemIndex", o.ItemIndex.ToString());
		dictionary.Add("Star", o.Star.ToString());
		return dictionary;
	}
}

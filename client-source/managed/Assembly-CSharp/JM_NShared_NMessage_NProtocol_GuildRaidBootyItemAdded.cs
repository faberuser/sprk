using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NMessage.NProtocol;

public static class JM_NShared_NMessage_NProtocol_GuildRaidBootyItemAdded
{
	public static GuildRaidBootyItemAdded Parse(string s)
	{
		return Parse(s, new GuildRaidBootyItemAdded());
	}

	public static GuildRaidBootyItemAdded Parse(string s, GuildRaidBootyItemAdded o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static GuildRaidBootyItemAdded Parse(IDictionary d)
	{
		return Parse(d, new GuildRaidBootyItemAdded());
	}

	public static GuildRaidBootyItemAdded Parse(IDictionary d, GuildRaidBootyItemAdded o)
	{
		o.EquipItemInfos = BaseJsonMarshaler.ParseArray(d, "EquipItemInfos", JM_NShared_GuildRaidBootyEquipItemInfo.Parse);
		o.ItemInfos = BaseJsonMarshaler.ParseArray(d, "ItemInfos", JM_NShared_GuildRaidBootyItemInfo.Parse);
		return o;
	}

	public static string EncodeJsonString(GuildRaidBootyItemAdded o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(GuildRaidBootyItemAdded o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("EquipItemInfos", BaseJsonMarshaler.EncodeArray(o.EquipItemInfos, JM_NShared_GuildRaidBootyEquipItemInfo.EncodeJsonObject));
		dictionary.Add("ItemInfos", BaseJsonMarshaler.EncodeArray(o.ItemInfos, JM_NShared_GuildRaidBootyItemInfo.EncodeJsonObject));
		return dictionary;
	}
}

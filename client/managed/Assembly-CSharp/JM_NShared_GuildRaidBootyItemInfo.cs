using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_GuildRaidBootyItemInfo
{
	public static GuildRaidBootyItemInfo Parse(string s)
	{
		return Parse(s, new GuildRaidBootyItemInfo());
	}

	public static GuildRaidBootyItemInfo Parse(string s, GuildRaidBootyItemInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static GuildRaidBootyItemInfo Parse(IDictionary d)
	{
		return Parse(d, new GuildRaidBootyItemInfo());
	}

	public static GuildRaidBootyItemInfo Parse(IDictionary d, GuildRaidBootyItemInfo o)
	{
		o.Id = BaseJsonMarshaler.ParseValueLong(d, "Id");
		o.CreatedTime = BaseJsonMarshaler.ParseValueString(d, "CreatedTime");
		o.ItemIndex = BaseJsonMarshaler.ParseValueInt(d, "ItemIndex");
		o.Count = BaseJsonMarshaler.ParseValueInt(d, "Count");
		return o;
	}

	public static string EncodeJsonString(GuildRaidBootyItemInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(GuildRaidBootyItemInfo o)
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
		dictionary.Add("Count", o.Count.ToString());
		return dictionary;
	}
}

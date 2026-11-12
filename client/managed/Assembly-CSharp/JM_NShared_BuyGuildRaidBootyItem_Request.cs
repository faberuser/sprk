using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.BuyGuildRaidBootyItem;

public static class JM_NShared_BuyGuildRaidBootyItem_Request
{
	public static NShared.BuyGuildRaidBootyItem.Request Parse(string s)
	{
		return Parse(s, new NShared.BuyGuildRaidBootyItem.Request());
	}

	public static NShared.BuyGuildRaidBootyItem.Request Parse(string s, NShared.BuyGuildRaidBootyItem.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.BuyGuildRaidBootyItem.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.BuyGuildRaidBootyItem.Request());
	}

	public static NShared.BuyGuildRaidBootyItem.Request Parse(IDictionary d, NShared.BuyGuildRaidBootyItem.Request o)
	{
		o.ShopIndex = BaseJsonMarshaler.ParseValueInt(d, "ShopIndex");
		o.Id = BaseJsonMarshaler.ParseValueLong(d, "Id");
		o.ItemIndex = BaseJsonMarshaler.ParseValueInt(d, "ItemIndex");
		o.Count = BaseJsonMarshaler.ParseValueInt(d, "Count");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.BuyGuildRaidBootyItem.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.BuyGuildRaidBootyItem.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ShopIndex", o.ShopIndex.ToString());
		dictionary.Add("Id", o.Id.ToString());
		dictionary.Add("ItemIndex", o.ItemIndex.ToString());
		dictionary.Add("Count", o.Count.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}

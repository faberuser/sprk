using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.BuyPurchaseDungeon;

public static class JM_NShared_BuyPurchaseDungeon_Request
{
	public static NShared.BuyPurchaseDungeon.Request Parse(string s)
	{
		return Parse(s, new NShared.BuyPurchaseDungeon.Request());
	}

	public static NShared.BuyPurchaseDungeon.Request Parse(string s, NShared.BuyPurchaseDungeon.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.BuyPurchaseDungeon.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.BuyPurchaseDungeon.Request());
	}

	public static NShared.BuyPurchaseDungeon.Request Parse(IDictionary d, NShared.BuyPurchaseDungeon.Request o)
	{
		o.ChapterIndex = BaseJsonMarshaler.ParseValueInt(d, "ChapterIndex");
		o.DungeonIndex = BaseJsonMarshaler.ParseValueInt(d, "DungeonIndex");
		o.Price = BaseJsonMarshaler.ParseValueInt(d, "Price");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.BuyPurchaseDungeon.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.BuyPurchaseDungeon.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ChapterIndex", o.ChapterIndex.ToString());
		dictionary.Add("DungeonIndex", o.DungeonIndex.ToString());
		dictionary.Add("Price", o.Price.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}

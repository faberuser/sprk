using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.UsePotionItem;

public static class JM_NShared_UsePotionItem_Request
{
	public static NShared.UsePotionItem.Request Parse(string s)
	{
		return Parse(s, new NShared.UsePotionItem.Request());
	}

	public static NShared.UsePotionItem.Request Parse(string s, NShared.UsePotionItem.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.UsePotionItem.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.UsePotionItem.Request());
	}

	public static NShared.UsePotionItem.Request Parse(IDictionary d, NShared.UsePotionItem.Request o)
	{
		o.ItemIndex = BaseJsonMarshaler.ParseValueInt(d, "ItemIndex");
		o.HeroIndex = BaseJsonMarshaler.ParseValueInt(d, "HeroIndex");
		o.ItemCount = BaseJsonMarshaler.ParseValueInt(d, "ItemCount");
		o.ChapterIndex = BaseJsonMarshaler.ParseValueInt(d, "ChapterIndex");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.UsePotionItem.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.UsePotionItem.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ItemIndex", o.ItemIndex.ToString());
		dictionary.Add("HeroIndex", o.HeroIndex.ToString());
		dictionary.Add("ItemCount", o.ItemCount.ToString());
		dictionary.Add("ChapterIndex", o.ChapterIndex.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return SprkInventoryTickets.AddParameters(dictionary, o);
	}
}

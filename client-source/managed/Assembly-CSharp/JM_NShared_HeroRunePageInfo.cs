using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_HeroRunePageInfo
{
	public static HeroRunePageInfo Parse(string s)
	{
		return Parse(s, new HeroRunePageInfo());
	}

	public static HeroRunePageInfo Parse(string s, HeroRunePageInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static HeroRunePageInfo Parse(IDictionary d)
	{
		return Parse(d, new HeroRunePageInfo());
	}

	public static HeroRunePageInfo Parse(IDictionary d, HeroRunePageInfo o)
	{
		o.HeroIndex = BaseJsonMarshaler.ParseValueInt(d, "HeroIndex");
		o.RunePage = BaseJsonMarshaler.ParseValueInt(d, "RunePage");
		o.SlotNum = BaseJsonMarshaler.ParseValueInt(d, "SlotNum");
		o.EquipItemSlotIndex = BaseJsonMarshaler.ParseValueInt(d, "EquipItemSlotIndex");
		o.ItemIndex = BaseJsonMarshaler.ParseValueInt(d, "ItemIndex");
		return o;
	}

	public static string EncodeJsonString(HeroRunePageInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(HeroRunePageInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("HeroIndex", o.HeroIndex.ToString());
		dictionary.Add("RunePage", o.RunePage.ToString());
		dictionary.Add("SlotNum", o.SlotNum.ToString());
		dictionary.Add("EquipItemSlotIndex", o.EquipItemSlotIndex.ToString());
		dictionary.Add("ItemIndex", o.ItemIndex.ToString());
		return dictionary;
	}
}

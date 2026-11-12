using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_PayShopProductEquipItemInfo
{
	public static PayShopProductEquipItemInfo Parse(string s)
	{
		return Parse(s, new PayShopProductEquipItemInfo());
	}

	public static PayShopProductEquipItemInfo Parse(string s, PayShopProductEquipItemInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static PayShopProductEquipItemInfo Parse(IDictionary d)
	{
		return Parse(d, new PayShopProductEquipItemInfo());
	}

	public static PayShopProductEquipItemInfo Parse(IDictionary d, PayShopProductEquipItemInfo o)
	{
		o.ItemIndex = BaseJsonMarshaler.ParseValueInt(d, "ItemIndex");
		o.Star = BaseJsonMarshaler.ParseValueInt(d, "Star");
		o.Level = BaseJsonMarshaler.ParseValueInt(d, "Level");
		return o;
	}

	public static string EncodeJsonString(PayShopProductEquipItemInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(PayShopProductEquipItemInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ItemIndex", o.ItemIndex.ToString());
		dictionary.Add("Star", o.Star.ToString());
		dictionary.Add("Level", o.Level.ToString());
		return dictionary;
	}
}

using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.BuyShopItem;

public static class JM_NShared_BuyShopItem_Request
{
	public static NShared.BuyShopItem.Request Parse(string s)
	{
		return Parse(s, new NShared.BuyShopItem.Request());
	}

	public static NShared.BuyShopItem.Request Parse(string s, NShared.BuyShopItem.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.BuyShopItem.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.BuyShopItem.Request());
	}

	public static NShared.BuyShopItem.Request Parse(IDictionary d, NShared.BuyShopItem.Request o)
	{
		o.ShopIndex = BaseJsonMarshaler.ParseValueInt(d, "ShopIndex");
		o.ListNo = BaseJsonMarshaler.ParseValueInt(d, "ListNo");
		o.ShopItemIndex = BaseJsonMarshaler.ParseValueInt(d, "ShopItemIndex");
		o.ShopItemPurchaseCount = BaseJsonMarshaler.ParseValueInt(d, "ShopItemPurchaseCount");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.BuyShopItem.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.BuyShopItem.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ShopIndex", o.ShopIndex.ToString());
		dictionary.Add("ListNo", o.ListNo.ToString());
		dictionary.Add("ShopItemIndex", o.ShopItemIndex.ToString());
		dictionary.Add("ShopItemPurchaseCount", o.ShopItemPurchaseCount.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}

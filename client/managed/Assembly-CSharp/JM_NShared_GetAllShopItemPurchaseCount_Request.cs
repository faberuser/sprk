using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.GetAllShopItemPurchaseCount;

public static class JM_NShared_GetAllShopItemPurchaseCount_Request
{
	public static NShared.GetAllShopItemPurchaseCount.Request Parse(string s)
	{
		return Parse(s, new NShared.GetAllShopItemPurchaseCount.Request());
	}

	public static NShared.GetAllShopItemPurchaseCount.Request Parse(string s, NShared.GetAllShopItemPurchaseCount.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.GetAllShopItemPurchaseCount.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.GetAllShopItemPurchaseCount.Request());
	}

	public static NShared.GetAllShopItemPurchaseCount.Request Parse(IDictionary d, NShared.GetAllShopItemPurchaseCount.Request o)
	{
		o.ShopIndex = BaseJsonMarshaler.ParseValueInt(d, "ShopIndex");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.GetAllShopItemPurchaseCount.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.GetAllShopItemPurchaseCount.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ShopIndex", o.ShopIndex.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}

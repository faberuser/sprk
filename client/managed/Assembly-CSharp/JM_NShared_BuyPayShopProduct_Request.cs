using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.BuyPayShopProduct;

public static class JM_NShared_BuyPayShopProduct_Request
{
	public static NShared.BuyPayShopProduct.Request Parse(string s)
	{
		return Parse(s, new NShared.BuyPayShopProduct.Request());
	}

	public static NShared.BuyPayShopProduct.Request Parse(string s, NShared.BuyPayShopProduct.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.BuyPayShopProduct.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.BuyPayShopProduct.Request());
	}

	public static NShared.BuyPayShopProduct.Request Parse(IDictionary d, NShared.BuyPayShopProduct.Request o)
	{
		o.Index = BaseJsonMarshaler.ParseValueLong(d, "Index");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.BuyPayShopProduct.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.BuyPayShopProduct.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return SprkSpecialShop.Encode(dictionary, o.SprkQuantity);
		}
		dictionary.Add("Index", o.Index.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return SprkSpecialShop.Encode(dictionary, o.SprkQuantity);
	}
}

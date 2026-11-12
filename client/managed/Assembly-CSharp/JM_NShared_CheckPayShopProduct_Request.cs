using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.CheckPayShopProduct;

public static class JM_NShared_CheckPayShopProduct_Request
{
	public static NShared.CheckPayShopProduct.Request Parse(string s)
	{
		return Parse(s, new NShared.CheckPayShopProduct.Request());
	}

	public static NShared.CheckPayShopProduct.Request Parse(string s, NShared.CheckPayShopProduct.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.CheckPayShopProduct.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.CheckPayShopProduct.Request());
	}

	public static NShared.CheckPayShopProduct.Request Parse(IDictionary d, NShared.CheckPayShopProduct.Request o)
	{
		o.Platform = BaseJsonMarshaler.ParseEnum<EPurchasePlatform>(d, "Platform");
		o.ProductID = BaseJsonMarshaler.ParseValueString(d, "ProductID");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.CheckPayShopProduct.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.CheckPayShopProduct.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Platform", o.Platform.ToString());
		if (o.ProductID != null)
		{
			dictionary.Add("ProductID", o.ProductID);
		}
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}

using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.GetPayShopProducts;

public static class JM_NShared_GetPayShopProducts_Request
{
	public static NShared.GetPayShopProducts.Request Parse(string s)
	{
		return Parse(s, new NShared.GetPayShopProducts.Request());
	}

	public static NShared.GetPayShopProducts.Request Parse(string s, NShared.GetPayShopProducts.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.GetPayShopProducts.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.GetPayShopProducts.Request());
	}

	public static NShared.GetPayShopProducts.Request Parse(IDictionary d, NShared.GetPayShopProducts.Request o)
	{
		o.CategoryType = BaseJsonMarshaler.ParseEnum<PayShopCategory>(d, "CategoryType");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.GetPayShopProducts.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.GetPayShopProducts.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("CategoryType", o.CategoryType.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}

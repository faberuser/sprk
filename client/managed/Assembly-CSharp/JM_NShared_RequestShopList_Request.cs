using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.RequestShopList;

public static class JM_NShared_RequestShopList_Request
{
	public static NShared.RequestShopList.Request Parse(string s)
	{
		return Parse(s, new NShared.RequestShopList.Request());
	}

	public static NShared.RequestShopList.Request Parse(string s, NShared.RequestShopList.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.RequestShopList.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.RequestShopList.Request());
	}

	public static NShared.RequestShopList.Request Parse(IDictionary d, NShared.RequestShopList.Request o)
	{
		o.ShopIndex = BaseJsonMarshaler.ParseValueInt(d, "ShopIndex");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.RequestShopList.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.RequestShopList.Request o)
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

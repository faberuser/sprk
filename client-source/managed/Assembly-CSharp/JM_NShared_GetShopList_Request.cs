using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.GetShopList;

public static class JM_NShared_GetShopList_Request
{
	public static NShared.GetShopList.Request Parse(string s)
	{
		return Parse(s, new NShared.GetShopList.Request());
	}

	public static NShared.GetShopList.Request Parse(string s, NShared.GetShopList.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.GetShopList.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.GetShopList.Request());
	}

	public static NShared.GetShopList.Request Parse(IDictionary d, NShared.GetShopList.Request o)
	{
		o.ShopIndex = BaseJsonMarshaler.ParseValueInt(d, "ShopIndex");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.GetShopList.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.GetShopList.Request o)
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

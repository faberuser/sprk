using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_ShopRestockTimeInfo
{
	public static ShopRestockTimeInfo Parse(string s)
	{
		return Parse(s, new ShopRestockTimeInfo());
	}

	public static ShopRestockTimeInfo Parse(string s, ShopRestockTimeInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static ShopRestockTimeInfo Parse(IDictionary d)
	{
		return Parse(d, new ShopRestockTimeInfo());
	}

	public static ShopRestockTimeInfo Parse(IDictionary d, ShopRestockTimeInfo o)
	{
		o.ShopIndex = BaseJsonMarshaler.ParseValueInt(d, "ShopIndex");
		o.ShopItemListIndex = BaseJsonMarshaler.ParseValueInt(d, "ShopItemListIndex");
		o.RestockTime = BaseJsonMarshaler.ParseValueString(d, "RestockTime");
		o.RestockCount = BaseJsonMarshaler.ParseValueInt(d, "RestockCount");
		o.RestockCountResetTime = BaseJsonMarshaler.ParseValueString(d, "RestockCountResetTime");
		return o;
	}

	public static string EncodeJsonString(ShopRestockTimeInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(ShopRestockTimeInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ShopIndex", o.ShopIndex.ToString());
		dictionary.Add("ShopItemListIndex", o.ShopItemListIndex.ToString());
		if (o.RestockTime != null)
		{
			dictionary.Add("RestockTime", o.RestockTime);
		}
		dictionary.Add("RestockCount", o.RestockCount.ToString());
		if (o.RestockCountResetTime != null)
		{
			dictionary.Add("RestockCountResetTime", o.RestockCountResetTime);
		}
		return dictionary;
	}
}

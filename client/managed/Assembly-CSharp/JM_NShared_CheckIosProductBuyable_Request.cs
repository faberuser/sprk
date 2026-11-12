using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.CheckIosProductBuyable;

public static class JM_NShared_CheckIosProductBuyable_Request
{
	public static NShared.CheckIosProductBuyable.Request Parse(string s)
	{
		return Parse(s, new NShared.CheckIosProductBuyable.Request());
	}

	public static NShared.CheckIosProductBuyable.Request Parse(string s, NShared.CheckIosProductBuyable.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.CheckIosProductBuyable.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.CheckIosProductBuyable.Request());
	}

	public static NShared.CheckIosProductBuyable.Request Parse(IDictionary d, NShared.CheckIosProductBuyable.Request o)
	{
		o.ProductID = BaseJsonMarshaler.ParseValueString(d, "ProductID");
		o.ProductIndex = BaseJsonMarshaler.ParseValueInt(d, "ProductIndex");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.CheckIosProductBuyable.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.CheckIosProductBuyable.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.ProductID != null)
		{
			dictionary.Add("ProductID", o.ProductID);
		}
		dictionary.Add("ProductIndex", o.ProductIndex.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}

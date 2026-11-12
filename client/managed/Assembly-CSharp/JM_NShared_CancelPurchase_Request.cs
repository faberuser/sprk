using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.CancelPurchase;

public static class JM_NShared_CancelPurchase_Request
{
	public static NShared.CancelPurchase.Request Parse(string s)
	{
		return Parse(s, new NShared.CancelPurchase.Request());
	}

	public static NShared.CancelPurchase.Request Parse(string s, NShared.CancelPurchase.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.CancelPurchase.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.CancelPurchase.Request());
	}

	public static NShared.CancelPurchase.Request Parse(IDictionary d, NShared.CancelPurchase.Request o)
	{
		o.ProductId = BaseJsonMarshaler.ParseValueString(d, "ProductId");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.CancelPurchase.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.CancelPurchase.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.ProductId != null)
		{
			dictionary.Add("ProductId", o.ProductId);
		}
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}

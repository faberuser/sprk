using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.RefreshPayshopDiscountInfo;

public static class JM_NShared_RefreshPayshopDiscountInfo_Request
{
	public static NShared.RefreshPayshopDiscountInfo.Request Parse(string s)
	{
		return Parse(s, new NShared.RefreshPayshopDiscountInfo.Request());
	}

	public static NShared.RefreshPayshopDiscountInfo.Request Parse(string s, NShared.RefreshPayshopDiscountInfo.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.RefreshPayshopDiscountInfo.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.RefreshPayshopDiscountInfo.Request());
	}

	public static NShared.RefreshPayshopDiscountInfo.Request Parse(IDictionary d, NShared.RefreshPayshopDiscountInfo.Request o)
	{
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.RefreshPayshopDiscountInfo.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.RefreshPayshopDiscountInfo.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}

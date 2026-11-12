using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.GetActiveProductInfo;

public static class JM_NShared_GetActiveProductInfo_Request
{
	public static NShared.GetActiveProductInfo.Request Parse(string s)
	{
		return Parse(s, new NShared.GetActiveProductInfo.Request());
	}

	public static NShared.GetActiveProductInfo.Request Parse(string s, NShared.GetActiveProductInfo.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.GetActiveProductInfo.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.GetActiveProductInfo.Request());
	}

	public static NShared.GetActiveProductInfo.Request Parse(IDictionary d, NShared.GetActiveProductInfo.Request o)
	{
		o.ProductIndex = BaseJsonMarshaler.ParseValueInt(d, "ProductIndex");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.GetActiveProductInfo.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.GetActiveProductInfo.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ProductIndex", o.ProductIndex.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}

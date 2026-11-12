using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.GetRightAwayProductInfo;

public static class JM_NShared_GetRightAwayProductInfo_Request
{
	public static NShared.GetRightAwayProductInfo.Request Parse(string s)
	{
		return Parse(s, new NShared.GetRightAwayProductInfo.Request());
	}

	public static NShared.GetRightAwayProductInfo.Request Parse(string s, NShared.GetRightAwayProductInfo.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.GetRightAwayProductInfo.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.GetRightAwayProductInfo.Request());
	}

	public static NShared.GetRightAwayProductInfo.Request Parse(IDictionary d, NShared.GetRightAwayProductInfo.Request o)
	{
		o.ProductIndex = BaseJsonMarshaler.ParseValueInt(d, "ProductIndex");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.GetRightAwayProductInfo.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.GetRightAwayProductInfo.Request o)
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

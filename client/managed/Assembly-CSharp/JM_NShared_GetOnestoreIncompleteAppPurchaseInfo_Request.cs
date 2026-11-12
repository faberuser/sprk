using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.GetOnestoreIncompleteAppPurchaseInfo;

public static class JM_NShared_GetOnestoreIncompleteAppPurchaseInfo_Request
{
	public static NShared.GetOnestoreIncompleteAppPurchaseInfo.Request Parse(string s)
	{
		return Parse(s, new NShared.GetOnestoreIncompleteAppPurchaseInfo.Request());
	}

	public static NShared.GetOnestoreIncompleteAppPurchaseInfo.Request Parse(string s, NShared.GetOnestoreIncompleteAppPurchaseInfo.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.GetOnestoreIncompleteAppPurchaseInfo.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.GetOnestoreIncompleteAppPurchaseInfo.Request());
	}

	public static NShared.GetOnestoreIncompleteAppPurchaseInfo.Request Parse(IDictionary d, NShared.GetOnestoreIncompleteAppPurchaseInfo.Request o)
	{
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.GetOnestoreIncompleteAppPurchaseInfo.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.GetOnestoreIncompleteAppPurchaseInfo.Request o)
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

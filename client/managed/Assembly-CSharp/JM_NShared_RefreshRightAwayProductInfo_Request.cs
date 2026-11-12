using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.RefreshRightAwayProductInfo;

public static class JM_NShared_RefreshRightAwayProductInfo_Request
{
	public static NShared.RefreshRightAwayProductInfo.Request Parse(string s)
	{
		return Parse(s, new NShared.RefreshRightAwayProductInfo.Request());
	}

	public static NShared.RefreshRightAwayProductInfo.Request Parse(string s, NShared.RefreshRightAwayProductInfo.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.RefreshRightAwayProductInfo.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.RefreshRightAwayProductInfo.Request());
	}

	public static NShared.RefreshRightAwayProductInfo.Request Parse(IDictionary d, NShared.RefreshRightAwayProductInfo.Request o)
	{
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.RefreshRightAwayProductInfo.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.RefreshRightAwayProductInfo.Request o)
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

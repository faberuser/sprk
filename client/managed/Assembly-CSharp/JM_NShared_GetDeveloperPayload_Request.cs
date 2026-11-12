using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.GetDeveloperPayload;

public static class JM_NShared_GetDeveloperPayload_Request
{
	public static NShared.GetDeveloperPayload.Request Parse(string s)
	{
		return Parse(s, new NShared.GetDeveloperPayload.Request());
	}

	public static NShared.GetDeveloperPayload.Request Parse(string s, NShared.GetDeveloperPayload.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.GetDeveloperPayload.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.GetDeveloperPayload.Request());
	}

	public static NShared.GetDeveloperPayload.Request Parse(IDictionary d, NShared.GetDeveloperPayload.Request o)
	{
		o.Platform = BaseJsonMarshaler.ParseEnum<EPurchasePlatform>(d, "Platform");
		o.ProductID = BaseJsonMarshaler.ParseValueString(d, "ProductID");
		o.ProductIndex = BaseJsonMarshaler.ParseValueInt(d, "ProductIndex");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.GetDeveloperPayload.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.GetDeveloperPayload.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Platform", o.Platform.ToString());
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

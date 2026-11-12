using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.GetRecommendPopupInfo;

public static class JM_NShared_GetRecommendPopupInfo_Request
{
	public static NShared.GetRecommendPopupInfo.Request Parse(string s)
	{
		return Parse(s, new NShared.GetRecommendPopupInfo.Request());
	}

	public static NShared.GetRecommendPopupInfo.Request Parse(string s, NShared.GetRecommendPopupInfo.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.GetRecommendPopupInfo.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.GetRecommendPopupInfo.Request());
	}

	public static NShared.GetRecommendPopupInfo.Request Parse(IDictionary d, NShared.GetRecommendPopupInfo.Request o)
	{
		o.Language = BaseJsonMarshaler.ParseValueString(d, "Language");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.GetRecommendPopupInfo.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.GetRecommendPopupInfo.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.Language != null)
		{
			dictionary.Add("Language", o.Language);
		}
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}

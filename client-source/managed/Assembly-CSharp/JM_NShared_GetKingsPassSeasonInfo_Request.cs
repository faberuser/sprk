using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.GetKingsPassSeasonInfo;

public static class JM_NShared_GetKingsPassSeasonInfo_Request
{
	public static NShared.GetKingsPassSeasonInfo.Request Parse(string s)
	{
		return Parse(s, new NShared.GetKingsPassSeasonInfo.Request());
	}

	public static NShared.GetKingsPassSeasonInfo.Request Parse(string s, NShared.GetKingsPassSeasonInfo.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.GetKingsPassSeasonInfo.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.GetKingsPassSeasonInfo.Request());
	}

	public static NShared.GetKingsPassSeasonInfo.Request Parse(IDictionary d, NShared.GetKingsPassSeasonInfo.Request o)
	{
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.GetKingsPassSeasonInfo.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.GetKingsPassSeasonInfo.Request o)
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

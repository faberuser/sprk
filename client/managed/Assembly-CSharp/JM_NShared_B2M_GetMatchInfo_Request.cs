using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.B2M_GetMatchInfo;

public static class JM_NShared_B2M_GetMatchInfo_Request
{
	public static NShared.B2M_GetMatchInfo.Request Parse(string s)
	{
		return Parse(s, new NShared.B2M_GetMatchInfo.Request());
	}

	public static NShared.B2M_GetMatchInfo.Request Parse(string s, NShared.B2M_GetMatchInfo.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.B2M_GetMatchInfo.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.B2M_GetMatchInfo.Request());
	}

	public static NShared.B2M_GetMatchInfo.Request Parse(IDictionary d, NShared.B2M_GetMatchInfo.Request o)
	{
		o.MatchUid = BaseJsonMarshaler.ParseValueLong(d, "MatchUid");
		return o;
	}

	public static string EncodeJsonString(NShared.B2M_GetMatchInfo.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.B2M_GetMatchInfo.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("MatchUid", o.MatchUid.ToString());
		return dictionary;
	}
}

using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.GetSeasonInfo;

public static class JM_NShared_GetSeasonInfo_Request
{
	public static NShared.GetSeasonInfo.Request Parse(string s)
	{
		return Parse(s, new NShared.GetSeasonInfo.Request());
	}

	public static NShared.GetSeasonInfo.Request Parse(string s, NShared.GetSeasonInfo.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.GetSeasonInfo.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.GetSeasonInfo.Request());
	}

	public static NShared.GetSeasonInfo.Request Parse(IDictionary d, NShared.GetSeasonInfo.Request o)
	{
		o.ArenaType = BaseJsonMarshaler.ParseEnum<MatchArenaType>(d, "ArenaType");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.GetSeasonInfo.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.GetSeasonInfo.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ArenaType", o.ArenaType.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}

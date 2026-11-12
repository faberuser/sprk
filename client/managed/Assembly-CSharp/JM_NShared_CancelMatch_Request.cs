using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.CancelMatch;

public static class JM_NShared_CancelMatch_Request
{
	public static NShared.CancelMatch.Request Parse(string s)
	{
		return Parse(s, new NShared.CancelMatch.Request());
	}

	public static NShared.CancelMatch.Request Parse(string s, NShared.CancelMatch.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.CancelMatch.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.CancelMatch.Request());
	}

	public static NShared.CancelMatch.Request Parse(IDictionary d, NShared.CancelMatch.Request o)
	{
		o.ArenaType = BaseJsonMarshaler.ParseEnum<MatchArenaType>(d, "ArenaType");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.CancelMatch.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.CancelMatch.Request o)
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

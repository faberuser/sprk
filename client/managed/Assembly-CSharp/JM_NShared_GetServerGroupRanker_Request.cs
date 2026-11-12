using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.GetServerGroupRanker;

public static class JM_NShared_GetServerGroupRanker_Request
{
	public static NShared.GetServerGroupRanker.Request Parse(string s)
	{
		return Parse(s, new NShared.GetServerGroupRanker.Request());
	}

	public static NShared.GetServerGroupRanker.Request Parse(string s, NShared.GetServerGroupRanker.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.GetServerGroupRanker.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.GetServerGroupRanker.Request());
	}

	public static NShared.GetServerGroupRanker.Request Parse(IDictionary d, NShared.GetServerGroupRanker.Request o)
	{
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.GetServerGroupRanker.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.GetServerGroupRanker.Request o)
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

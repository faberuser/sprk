using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.GetKingsPassAllRewards;

public static class JM_NShared_GetKingsPassAllRewards_Request
{
	public static NShared.GetKingsPassAllRewards.Request Parse(string s)
	{
		return Parse(s, new NShared.GetKingsPassAllRewards.Request());
	}

	public static NShared.GetKingsPassAllRewards.Request Parse(string s, NShared.GetKingsPassAllRewards.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.GetKingsPassAllRewards.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.GetKingsPassAllRewards.Request());
	}

	public static NShared.GetKingsPassAllRewards.Request Parse(IDictionary d, NShared.GetKingsPassAllRewards.Request o)
	{
		o.SeasonIndex = BaseJsonMarshaler.ParseValueInt(d, "SeasonIndex");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.GetKingsPassAllRewards.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.GetKingsPassAllRewards.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("SeasonIndex", o.SeasonIndex.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}

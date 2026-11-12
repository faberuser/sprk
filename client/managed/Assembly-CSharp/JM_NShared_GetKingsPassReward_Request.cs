using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.GetKingsPassReward;

public static class JM_NShared_GetKingsPassReward_Request
{
	public static NShared.GetKingsPassReward.Request Parse(string s)
	{
		return Parse(s, new NShared.GetKingsPassReward.Request());
	}

	public static NShared.GetKingsPassReward.Request Parse(string s, NShared.GetKingsPassReward.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.GetKingsPassReward.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.GetKingsPassReward.Request());
	}

	public static NShared.GetKingsPassReward.Request Parse(IDictionary d, NShared.GetKingsPassReward.Request o)
	{
		o.SeasonIndex = BaseJsonMarshaler.ParseValueInt(d, "SeasonIndex");
		o.MissionIndex = BaseJsonMarshaler.ParseValueInt(d, "MissionIndex");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.GetKingsPassReward.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.GetKingsPassReward.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("SeasonIndex", o.SeasonIndex.ToString());
		dictionary.Add("MissionIndex", o.MissionIndex.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}

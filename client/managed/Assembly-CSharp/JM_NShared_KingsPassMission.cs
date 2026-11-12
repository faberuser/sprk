using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_KingsPassMission
{
	public static KingsPassMission Parse(string s)
	{
		return Parse(s, new KingsPassMission());
	}

	public static KingsPassMission Parse(string s, KingsPassMission o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static KingsPassMission Parse(IDictionary d)
	{
		return Parse(d, new KingsPassMission());
	}

	public static KingsPassMission Parse(IDictionary d, KingsPassMission o)
	{
		o.SeasonIndex = BaseJsonMarshaler.ParseValueInt(d, "SeasonIndex");
		o.MissionIndex = BaseJsonMarshaler.ParseValueInt(d, "MissionIndex");
		o.LastStep = BaseJsonMarshaler.ParseValueInt(d, "LastStep");
		o.Progress = BaseJsonMarshaler.ParseValueInt(d, "Progress");
		o.RewardedTime = BaseJsonMarshaler.ParseValueString(d, "RewardedTime");
		return o;
	}

	public static string EncodeJsonString(KingsPassMission o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(KingsPassMission o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("SeasonIndex", o.SeasonIndex.ToString());
		dictionary.Add("MissionIndex", o.MissionIndex.ToString());
		dictionary.Add("LastStep", o.LastStep.ToString());
		dictionary.Add("Progress", o.Progress.ToString());
		if (o.RewardedTime != null)
		{
			dictionary.Add("RewardedTime", o.RewardedTime);
		}
		return dictionary;
	}
}

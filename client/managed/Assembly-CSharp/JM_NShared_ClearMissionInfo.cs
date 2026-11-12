using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_ClearMissionInfo
{
	public static ClearMissionInfo Parse(string s)
	{
		return Parse(s, new ClearMissionInfo());
	}

	public static ClearMissionInfo Parse(string s, ClearMissionInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static ClearMissionInfo Parse(IDictionary d)
	{
		return Parse(d, new ClearMissionInfo());
	}

	public static ClearMissionInfo Parse(IDictionary d, ClearMissionInfo o)
	{
		o.MissionIndex = BaseJsonMarshaler.ParseValueInt(d, "MissionIndex");
		o.LastStep = BaseJsonMarshaler.ParseValueInt(d, "LastStep");
		o.Progress = BaseJsonMarshaler.ParseValueLong(d, "Progress");
		o.RewardedTime = BaseJsonMarshaler.ParseValueString(d, "RewardedTime");
		return o;
	}

	public static string EncodeJsonString(ClearMissionInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(ClearMissionInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
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

using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.RewardAchievement;

public static class JM_NShared_RewardAchievement_Request
{
	public static NShared.RewardAchievement.Request Parse(string s)
	{
		return Parse(s, new NShared.RewardAchievement.Request());
	}

	public static NShared.RewardAchievement.Request Parse(string s, NShared.RewardAchievement.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.RewardAchievement.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.RewardAchievement.Request());
	}

	public static NShared.RewardAchievement.Request Parse(IDictionary d, NShared.RewardAchievement.Request o)
	{
		o.AchievementIndices = BaseJsonMarshaler.ParseArrayCast(d, "AchievementIndices", BaseJsonMarshaler.CastToInt);
		o.Steps = BaseJsonMarshaler.ParseArrayCast(d, "Steps", BaseJsonMarshaler.CastToInt);
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.RewardAchievement.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.RewardAchievement.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("AchievementIndices", BaseJsonMarshaler.EncodeArray(o.AchievementIndices));
		dictionary.Add("Steps", BaseJsonMarshaler.EncodeArray(o.Steps));
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}

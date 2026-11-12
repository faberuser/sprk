using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_ChapterClearRewardData
{
	public static ChapterClearRewardData Parse(string s)
	{
		return Parse(s, new ChapterClearRewardData());
	}

	public static ChapterClearRewardData Parse(string s, ChapterClearRewardData o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static ChapterClearRewardData Parse(IDictionary d)
	{
		return Parse(d, new ChapterClearRewardData());
	}

	public static ChapterClearRewardData Parse(IDictionary d, ChapterClearRewardData o)
	{
		o.ChapterIndex = BaseJsonMarshaler.ParseValueInt(d, "ChapterIndex");
		o.StepIndex = BaseJsonMarshaler.ParseValueInt(d, "StepIndex");
		o.RewardType = BaseJsonMarshaler.ParseEnum<RewardType>(d, "RewardType");
		o.RewardCount = BaseJsonMarshaler.ParseValueInt(d, "RewardCount");
		o.RewardItemCode = BaseJsonMarshaler.ParseValueString(d, "RewardItemCode");
		o.RewardReqStar = BaseJsonMarshaler.ParseValueInt(d, "RewardReqStar");
		return o;
	}

	public static string EncodeJsonString(ChapterClearRewardData o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(ChapterClearRewardData o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ChapterIndex", o.ChapterIndex.ToString());
		dictionary.Add("StepIndex", o.StepIndex.ToString());
		dictionary.Add("RewardType", o.RewardType.ToString());
		dictionary.Add("RewardCount", o.RewardCount.ToString());
		if (o.RewardItemCode != null)
		{
			dictionary.Add("RewardItemCode", o.RewardItemCode);
		}
		dictionary.Add("RewardReqStar", o.RewardReqStar.ToString());
		return dictionary;
	}
}

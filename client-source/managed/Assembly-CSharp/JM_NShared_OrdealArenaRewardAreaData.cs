using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_OrdealArenaRewardAreaData
{
	public static OrdealArenaRewardAreaData Parse(string s)
	{
		return Parse(s, new OrdealArenaRewardAreaData());
	}

	public static OrdealArenaRewardAreaData Parse(string s, OrdealArenaRewardAreaData o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static OrdealArenaRewardAreaData Parse(IDictionary d)
	{
		return Parse(d, new OrdealArenaRewardAreaData());
	}

	public static OrdealArenaRewardAreaData Parse(IDictionary d, OrdealArenaRewardAreaData o)
	{
		o.Index = BaseJsonMarshaler.ParseValueInt(d, "Index");
		o.TierType = BaseJsonMarshaler.ParseEnum<MatchTierType>(d, "TierType");
		o.RewardIndex = BaseJsonMarshaler.ParseValueInt(d, "RewardIndex");
		return o;
	}

	public static string EncodeJsonString(OrdealArenaRewardAreaData o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(OrdealArenaRewardAreaData o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Index", o.Index.ToString());
		dictionary.Add("TierType", o.TierType.ToString());
		dictionary.Add("RewardIndex", o.RewardIndex.ToString());
		return dictionary;
	}
}

using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_InfinityRewardGroupData
{
	public static InfinityRewardGroupData Parse(string s)
	{
		return Parse(s, new InfinityRewardGroupData());
	}

	public static InfinityRewardGroupData Parse(string s, InfinityRewardGroupData o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static InfinityRewardGroupData Parse(IDictionary d)
	{
		return Parse(d, new InfinityRewardGroupData());
	}

	public static InfinityRewardGroupData Parse(IDictionary d, InfinityRewardGroupData o)
	{
		o.Index = BaseJsonMarshaler.ParseValueInt(d, "Index");
		o.RewardIndices = BaseJsonMarshaler.ParseArrayCast(d, "RewardIndices", BaseJsonMarshaler.CastToInt);
		return o;
	}

	public static string EncodeJsonString(InfinityRewardGroupData o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(InfinityRewardGroupData o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Index", o.Index.ToString());
		dictionary.Add("RewardIndices", BaseJsonMarshaler.EncodeArray(o.RewardIndices));
		return dictionary;
	}
}

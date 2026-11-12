using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_PetExploreInfo
{
	public static PetExploreInfo Parse(string s)
	{
		return Parse(s, new PetExploreInfo());
	}

	public static PetExploreInfo Parse(string s, PetExploreInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static PetExploreInfo Parse(IDictionary d)
	{
		return Parse(d, new PetExploreInfo());
	}

	public static PetExploreInfo Parse(IDictionary d, PetExploreInfo o)
	{
		o.ExploreIndex = BaseJsonMarshaler.ParseValueInt(d, "ExploreIndex");
		o.DeckIndex = BaseJsonMarshaler.ParseValueInt(d, "DeckIndex");
		o.PetIndices = BaseJsonMarshaler.ParseValueString(d, "PetIndices");
		o.BeginTime = BaseJsonMarshaler.ParseValueString(d, "BeginTime");
		o.EndTime = BaseJsonMarshaler.ParseValueString(d, "EndTime");
		o.RewardCount = BaseJsonMarshaler.ParseValueInt(d, "RewardCount");
		o.RewardRate = BaseJsonMarshaler.ParseValueInt(d, "RewardRate");
		o.RewardGrade = BaseJsonMarshaler.ParseValueInt(d, "RewardGrade");
		return o;
	}

	public static string EncodeJsonString(PetExploreInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(PetExploreInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ExploreIndex", o.ExploreIndex.ToString());
		dictionary.Add("DeckIndex", o.DeckIndex.ToString());
		if (o.PetIndices != null)
		{
			dictionary.Add("PetIndices", o.PetIndices);
		}
		if (o.BeginTime != null)
		{
			dictionary.Add("BeginTime", o.BeginTime);
		}
		if (o.EndTime != null)
		{
			dictionary.Add("EndTime", o.EndTime);
		}
		dictionary.Add("RewardCount", o.RewardCount.ToString());
		dictionary.Add("RewardRate", o.RewardRate.ToString());
		dictionary.Add("RewardGrade", o.RewardGrade.ToString());
		return dictionary;
	}
}

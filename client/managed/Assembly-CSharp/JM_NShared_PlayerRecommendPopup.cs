using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_PlayerRecommendPopup
{
	public static PlayerRecommendPopup Parse(string s)
	{
		return Parse(s, new PlayerRecommendPopup());
	}

	public static PlayerRecommendPopup Parse(string s, PlayerRecommendPopup o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static PlayerRecommendPopup Parse(IDictionary d)
	{
		return Parse(d, new PlayerRecommendPopup());
	}

	public static PlayerRecommendPopup Parse(IDictionary d, PlayerRecommendPopup o)
	{
		o.RecommendId = BaseJsonMarshaler.ParseValueInt(d, "RecommendId");
		o.PeriodCount = BaseJsonMarshaler.ParseValueInt(d, "PeriodCount");
		return o;
	}

	public static string EncodeJsonString(PlayerRecommendPopup o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(PlayerRecommendPopup o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("RecommendId", o.RecommendId.ToString());
		dictionary.Add("PeriodCount", o.PeriodCount.ToString());
		return dictionary;
	}
}

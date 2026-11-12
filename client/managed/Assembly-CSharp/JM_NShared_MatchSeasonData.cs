using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_MatchSeasonData
{
	public static MatchSeasonData Parse(string s)
	{
		return Parse(s, new MatchSeasonData());
	}

	public static MatchSeasonData Parse(string s, MatchSeasonData o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static MatchSeasonData Parse(IDictionary d)
	{
		return Parse(d, new MatchSeasonData());
	}

	public static MatchSeasonData Parse(IDictionary d, MatchSeasonData o)
	{
		o.Index = BaseJsonMarshaler.ParseValueInt(d, "Index");
		o.Name = BaseJsonMarshaler.ParseValueString(d, "Name");
		o.Begin = BaseJsonMarshaler.ParseValueString(d, "Begin");
		o.End = BaseJsonMarshaler.ParseValueString(d, "End");
		o.Description = BaseJsonMarshaler.ParseValueString(d, "Description");
		o.SeasonIndex = BaseJsonMarshaler.ParseValueInt(d, "SeasonIndex");
		o.ArenaType = BaseJsonMarshaler.ParseEnum<MatchArenaType>(d, "ArenaType");
		o.MatchSeasonType = BaseJsonMarshaler.ParseEnum<MatchSeasonType>(d, "MatchSeasonType");
		o.IsTakeReward = BaseJsonMarshaler.ParseValueBool(d, "IsTakeReward");
		o.PlayTimeBegin = BaseJsonMarshaler.ParseValueString(d, "PlayTimeBegin");
		o.PlayTimeEnd = BaseJsonMarshaler.ParseValueString(d, "PlayTimeEnd");
		o.LimitedHeroStar = BaseJsonMarshaler.ParseValueInt(d, "LimitedHeroStar");
		o.LimitedHeroLevel = BaseJsonMarshaler.ParseValueInt(d, "LimitedHeroLevel");
		o.LimitedHeroCount = BaseJsonMarshaler.ParseValueInt(d, "LimitedHeroCount");
		o.ActiveTier = BaseJsonMarshaler.ParseValueBool(d, "ActiveTier");
		o.ShowSeasonIndex = BaseJsonMarshaler.ParseValueInt(d, "ShowSeasonIndex");
		return o;
	}

	public static string EncodeJsonString(MatchSeasonData o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(MatchSeasonData o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Index", o.Index.ToString());
		if (o.Name != null)
		{
			dictionary.Add("Name", o.Name);
		}
		if (o.Begin != null)
		{
			dictionary.Add("Begin", o.Begin);
		}
		if (o.End != null)
		{
			dictionary.Add("End", o.End);
		}
		if (o.Description != null)
		{
			dictionary.Add("Description", o.Description);
		}
		dictionary.Add("SeasonIndex", o.SeasonIndex.ToString());
		dictionary.Add("ArenaType", o.ArenaType.ToString());
		dictionary.Add("MatchSeasonType", o.MatchSeasonType.ToString());
		dictionary.Add("IsTakeReward", o.IsTakeReward.ToString());
		if (o.PlayTimeBegin != null)
		{
			dictionary.Add("PlayTimeBegin", o.PlayTimeBegin);
		}
		if (o.PlayTimeEnd != null)
		{
			dictionary.Add("PlayTimeEnd", o.PlayTimeEnd);
		}
		dictionary.Add("LimitedHeroStar", o.LimitedHeroStar.ToString());
		dictionary.Add("LimitedHeroLevel", o.LimitedHeroLevel.ToString());
		dictionary.Add("LimitedHeroCount", o.LimitedHeroCount.ToString());
		dictionary.Add("ActiveTier", o.ActiveTier.ToString());
		dictionary.Add("ShowSeasonIndex", o.ShowSeasonIndex.ToString());
		return dictionary;
	}
}

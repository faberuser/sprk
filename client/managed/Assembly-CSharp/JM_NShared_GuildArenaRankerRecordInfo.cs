using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_GuildArenaRankerRecordInfo
{
	public static GuildArenaRankerRecordInfo Parse(string s)
	{
		return Parse(s, new GuildArenaRankerRecordInfo());
	}

	public static GuildArenaRankerRecordInfo Parse(string s, GuildArenaRankerRecordInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static GuildArenaRankerRecordInfo Parse(IDictionary d)
	{
		return Parse(d, new GuildArenaRankerRecordInfo());
	}

	public static GuildArenaRankerRecordInfo Parse(IDictionary d, GuildArenaRankerRecordInfo o)
	{
		o.SessionIndex = BaseJsonMarshaler.ParseValueInt(d, "SessionIndex");
		o.SeasonIndex = BaseJsonMarshaler.ParseValueInt(d, "SeasonIndex");
		o.LeftRanker = BaseJsonMarshaler.ParseObject(d, "LeftRanker", JM_NShared_GuildArenaRankerInfo.Parse);
		o.RightRanker = BaseJsonMarshaler.ParseObject(d, "RightRanker", JM_NShared_GuildArenaRankerInfo.Parse);
		o.Win = BaseJsonMarshaler.ParseValueInt(d, "Win");
		o.SessionNumber = BaseJsonMarshaler.ParseValueInt(d, "SessionNumber");
		return o;
	}

	public static string EncodeJsonString(GuildArenaRankerRecordInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(GuildArenaRankerRecordInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("SessionIndex", o.SessionIndex.ToString());
		dictionary.Add("SeasonIndex", o.SeasonIndex.ToString());
		dictionary.Add("LeftRanker", JM_NShared_GuildArenaRankerInfo.EncodeJsonObject(o.LeftRanker));
		dictionary.Add("RightRanker", JM_NShared_GuildArenaRankerInfo.EncodeJsonObject(o.RightRanker));
		dictionary.Add("Win", o.Win.ToString());
		dictionary.Add("SessionNumber", o.SessionNumber.ToString());
		return dictionary;
	}
}

using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_GuildArenaSessionRecordInfo
{
	public static GuildArenaSessionRecordInfo Parse(string s)
	{
		return Parse(s, new GuildArenaSessionRecordInfo());
	}

	public static GuildArenaSessionRecordInfo Parse(string s, GuildArenaSessionRecordInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static GuildArenaSessionRecordInfo Parse(IDictionary d)
	{
		return Parse(d, new GuildArenaSessionRecordInfo());
	}

	public static GuildArenaSessionRecordInfo Parse(IDictionary d, GuildArenaSessionRecordInfo o)
	{
		o.SessionIndex = BaseJsonMarshaler.ParseValueInt(d, "SessionIndex");
		o.SeasonIndex = BaseJsonMarshaler.ParseValueInt(d, "SeasonIndex");
		o.LeftServerGroup = BaseJsonMarshaler.ParseValueString(d, "LeftServerGroup");
		o.LeftGuildId = BaseJsonMarshaler.ParseValueLong(d, "LeftGuildId");
		o.LeftGuildLogo = BaseJsonMarshaler.ParseValueByte(d, "LeftGuildLogo");
		o.LeftGuildBack = BaseJsonMarshaler.ParseValueByte(d, "LeftGuildBack");
		o.LeftGuildName = BaseJsonMarshaler.ParseValueString(d, "LeftGuildName");
		o.LeftCountryCode = BaseJsonMarshaler.ParseValueString(d, "LeftCountryCode");
		o.LeftTierIndex = BaseJsonMarshaler.ParseValueInt(d, "LeftTierIndex");
		o.LeftSessionScore = BaseJsonMarshaler.ParseValueInt(d, "LeftSessionScore");
		o.RightServerGroup = BaseJsonMarshaler.ParseValueString(d, "RightServerGroup");
		o.RightGuildId = BaseJsonMarshaler.ParseValueLong(d, "RightGuildId");
		o.RightGuildLogo = BaseJsonMarshaler.ParseValueByte(d, "RightGuildLogo");
		o.RightGuildBack = BaseJsonMarshaler.ParseValueByte(d, "RightGuildBack");
		o.RightGuildName = BaseJsonMarshaler.ParseValueString(d, "RightGuildName");
		o.RightCountryCode = BaseJsonMarshaler.ParseValueString(d, "RightCountryCode");
		o.RightTierIndex = BaseJsonMarshaler.ParseValueInt(d, "RightTierIndex");
		o.RightSessionScore = BaseJsonMarshaler.ParseValueInt(d, "RightSessionScore");
		o.Win = BaseJsonMarshaler.ParseValueInt(d, "Win");
		o.UpdatedTime = BaseJsonMarshaler.ParseValueString(d, "UpdatedTime");
		return o;
	}

	public static string EncodeJsonString(GuildArenaSessionRecordInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(GuildArenaSessionRecordInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("SessionIndex", o.SessionIndex.ToString());
		dictionary.Add("SeasonIndex", o.SeasonIndex.ToString());
		if (o.LeftServerGroup != null)
		{
			dictionary.Add("LeftServerGroup", o.LeftServerGroup);
		}
		dictionary.Add("LeftGuildId", o.LeftGuildId.ToString());
		dictionary.Add("LeftGuildLogo", o.LeftGuildLogo.ToString());
		dictionary.Add("LeftGuildBack", o.LeftGuildBack.ToString());
		if (o.LeftGuildName != null)
		{
			dictionary.Add("LeftGuildName", o.LeftGuildName);
		}
		if (o.LeftCountryCode != null)
		{
			dictionary.Add("LeftCountryCode", o.LeftCountryCode);
		}
		dictionary.Add("LeftTierIndex", o.LeftTierIndex.ToString());
		dictionary.Add("LeftSessionScore", o.LeftSessionScore.ToString());
		if (o.RightServerGroup != null)
		{
			dictionary.Add("RightServerGroup", o.RightServerGroup);
		}
		dictionary.Add("RightGuildId", o.RightGuildId.ToString());
		dictionary.Add("RightGuildLogo", o.RightGuildLogo.ToString());
		dictionary.Add("RightGuildBack", o.RightGuildBack.ToString());
		if (o.RightGuildName != null)
		{
			dictionary.Add("RightGuildName", o.RightGuildName);
		}
		if (o.RightCountryCode != null)
		{
			dictionary.Add("RightCountryCode", o.RightCountryCode);
		}
		dictionary.Add("RightTierIndex", o.RightTierIndex.ToString());
		dictionary.Add("RightSessionScore", o.RightSessionScore.ToString());
		dictionary.Add("Win", o.Win.ToString());
		if (o.UpdatedTime != null)
		{
			dictionary.Add("UpdatedTime", o.UpdatedTime);
		}
		return dictionary;
	}
}

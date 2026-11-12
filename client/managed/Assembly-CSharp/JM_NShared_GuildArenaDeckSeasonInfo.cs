using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_GuildArenaDeckSeasonInfo
{
	public static GuildArenaDeckSeasonInfo Parse(string s)
	{
		return Parse(s, new GuildArenaDeckSeasonInfo());
	}

	public static GuildArenaDeckSeasonInfo Parse(string s, GuildArenaDeckSeasonInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static GuildArenaDeckSeasonInfo Parse(IDictionary d)
	{
		return Parse(d, new GuildArenaDeckSeasonInfo());
	}

	public static GuildArenaDeckSeasonInfo Parse(IDictionary d, GuildArenaDeckSeasonInfo o)
	{
		o.SeasonIndex = BaseJsonMarshaler.ParseValueInt(d, "SeasonIndex");
		o.ServerGroup = BaseJsonMarshaler.ParseValueString(d, "ServerGroup");
		o.GuidId = BaseJsonMarshaler.ParseValueInt(d, "GuidId");
		o.AccountId = BaseJsonMarshaler.ParseValueLong(d, "AccountId");
		o.DeckIndex = BaseJsonMarshaler.ParseValueInt(d, "DeckIndex");
		o.HeroIndices = BaseJsonMarshaler.ParseValueString(d, "HeroIndices");
		o.SkillSlotIndices = BaseJsonMarshaler.ParseValueString(d, "SkillSlotIndices");
		o.MatchedNpcInfo = BaseJsonMarshaler.ParseObject(d, "MatchedNpcInfo", JM_NShared_MatchedAccountInfo.Parse);
		o.CachedAccountInfo = BaseJsonMarshaler.ParseValueString(d, "CachedAccountInfo");
		o.AppliedTime = BaseJsonMarshaler.ParseValueString(d, "AppliedTime");
		o.RaidIndex = BaseJsonMarshaler.ParseValueInt(d, "RaidIndex");
		o.Level = BaseJsonMarshaler.ParseValueInt(d, "Level");
		o.MonsterHp0 = BaseJsonMarshaler.ParseValueLong(d, "MonsterHp0");
		o.TotalDamage = BaseJsonMarshaler.ParseValueLong(d, "TotalDamage");
		return o;
	}

	public static string EncodeJsonString(GuildArenaDeckSeasonInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(GuildArenaDeckSeasonInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("SeasonIndex", o.SeasonIndex.ToString());
		if (o.ServerGroup != null)
		{
			dictionary.Add("ServerGroup", o.ServerGroup);
		}
		dictionary.Add("GuidId", o.GuidId.ToString());
		dictionary.Add("AccountId", o.AccountId.ToString());
		dictionary.Add("DeckIndex", o.DeckIndex.ToString());
		if (o.HeroIndices != null)
		{
			dictionary.Add("HeroIndices", o.HeroIndices);
		}
		if (o.SkillSlotIndices != null)
		{
			dictionary.Add("SkillSlotIndices", o.SkillSlotIndices);
		}
		dictionary.Add("MatchedNpcInfo", JM_NShared_MatchedAccountInfo.EncodeJsonObject(o.MatchedNpcInfo));
		if (o.CachedAccountInfo != null)
		{
			dictionary.Add("CachedAccountInfo", o.CachedAccountInfo);
		}
		if (o.AppliedTime != null)
		{
			dictionary.Add("AppliedTime", o.AppliedTime);
		}
		dictionary.Add("RaidIndex", o.RaidIndex.ToString());
		dictionary.Add("Level", o.Level.ToString());
		dictionary.Add("MonsterHp0", o.MonsterHp0.ToString());
		dictionary.Add("TotalDamage", o.TotalDamage.ToString());
		return dictionary;
	}
}

using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_MatchedAccountInfo
{
	public static MatchedAccountInfo Parse(string s)
	{
		return Parse(s, new MatchedAccountInfo());
	}

	public static MatchedAccountInfo Parse(string s, MatchedAccountInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static MatchedAccountInfo Parse(IDictionary d)
	{
		return Parse(d, new MatchedAccountInfo());
	}

	public static MatchedAccountInfo Parse(IDictionary d, MatchedAccountInfo o)
	{
		o.Host = BaseJsonMarshaler.ParseValueString(d, "Host");
		o.ServerGroup = BaseJsonMarshaler.ParseValueString(d, "ServerGroup");
		o.MatchServerHost = BaseJsonMarshaler.ParseValueString(d, "MatchServerHost");
		o.IsNpc = BaseJsonMarshaler.ParseValueBool(d, "IsNpc");
		o.ArenaType = BaseJsonMarshaler.ParseEnum<MatchArenaType>(d, "ArenaType");
		o.TeamType = BaseJsonMarshaler.ParseEnum<TeamType>(d, "TeamType");
		o.Mmr = BaseJsonMarshaler.ParseValueInt(d, "Mmr");
		o.FightingPower = BaseJsonMarshaler.ParseValueInt(d, "FightingPower");
		o.Rank = BaseJsonMarshaler.ParseValueInt(d, "Rank");
		o.LeaderHeroIndex = BaseJsonMarshaler.ParseValueInt(d, "LeaderHeroIndex");
		o.GuildInfo = BaseJsonMarshaler.ParseArray(d, "GuildInfo", JM_NShared_GuildServerInfo.Parse);
		o.GuildSkills = BaseJsonMarshaler.ParseArray(d, "GuildSkills", JM_NShared_GuildSkillDataBase.Parse);
		o.AccountBuffs = BaseJsonMarshaler.ParseArray(d, "AccountBuffs", JM_NShared_AccountBuffDataBase.Parse);
		o.ClassBuffDataBases = BaseJsonMarshaler.ParseArray(d, "ClassBuffDataBases", JM_NShared_ClassBuffDataBase.Parse);
		o.GuildArenaBuffDataBases = BaseJsonMarshaler.ParseArray(d, "GuildArenaBuffDataBases", JM_NShared_GuildArenaBuffDataBase.Parse);
		o.ExtraStatDataBases = BaseJsonMarshaler.ParseArray(d, "ExtraStatDataBases", JM_NShared_DefaultExtraStatData.Parse);
		o.PetStatDataBases = BaseJsonMarshaler.ParseArray(d, "PetStatDataBases", JM_NShared_PetStatDataBase.Parse);
		o.BattleInfo = BaseJsonMarshaler.ParseObject(d, "BattleInfo", JM_NShared_PlayerBattleInfo.Parse);
		o.AvgFightingPower = BaseJsonMarshaler.ParseValueInt(d, "AvgFightingPower");
		o.LuckyDeckIndex = BaseJsonMarshaler.ParseValueInt(d, "LuckyDeckIndex");
		o.UserInfo = BaseJsonMarshaler.ParseObject(d, "UserInfo", JM_NShared_CachedUserInfo.Parse);
		o.HeroInfos = BaseJsonMarshaler.ParseDictionary<int, CachedHeroInfo, Dictionary<int, CachedHeroInfo>>(d, "HeroInfos", JM_NShared_CachedHeroInfo.Parse);
		o.AiHeroInfos = BaseJsonMarshaler.ParseDictionary<int, CachedHeroInfo, Dictionary<int, CachedHeroInfo>>(d, "AiHeroInfos", JM_NShared_CachedHeroInfo.Parse);
		o.GroupHeroInfos = BaseJsonMarshaler.ParseDictionary<int, CachedHeroInfo, Dictionary<int, CachedHeroInfo>>(d, "GroupHeroInfos", JM_NShared_CachedHeroInfo.Parse);
		o.DeckInfos = BaseJsonMarshaler.ParseDictionary<BattleType, DeckInfo, Dictionary<BattleType, DeckInfo>>(d, "DeckInfos", JM_NShared_DeckInfo.Parse);
		return o;
	}

	public static string EncodeJsonString(MatchedAccountInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(MatchedAccountInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.Host != null)
		{
			dictionary.Add("Host", o.Host);
		}
		if (o.ServerGroup != null)
		{
			dictionary.Add("ServerGroup", o.ServerGroup);
		}
		if (o.MatchServerHost != null)
		{
			dictionary.Add("MatchServerHost", o.MatchServerHost);
		}
		dictionary.Add("IsNpc", o.IsNpc.ToString());
		dictionary.Add("ArenaType", o.ArenaType.ToString());
		dictionary.Add("TeamType", o.TeamType.ToString());
		dictionary.Add("Mmr", o.Mmr.ToString());
		dictionary.Add("FightingPower", o.FightingPower.ToString());
		dictionary.Add("Rank", o.Rank.ToString());
		dictionary.Add("LeaderHeroIndex", o.LeaderHeroIndex.ToString());
		dictionary.Add("GuildInfo", BaseJsonMarshaler.EncodeArray(o.GuildInfo, JM_NShared_GuildServerInfo.EncodeJsonObject));
		dictionary.Add("GuildSkills", BaseJsonMarshaler.EncodeArray(o.GuildSkills, JM_NShared_GuildSkillDataBase.EncodeJsonObject));
		dictionary.Add("AccountBuffs", BaseJsonMarshaler.EncodeArray(o.AccountBuffs, JM_NShared_AccountBuffDataBase.EncodeJsonObject));
		dictionary.Add("ClassBuffDataBases", BaseJsonMarshaler.EncodeArray(o.ClassBuffDataBases, JM_NShared_ClassBuffDataBase.EncodeJsonObject));
		dictionary.Add("GuildArenaBuffDataBases", BaseJsonMarshaler.EncodeArray(o.GuildArenaBuffDataBases, JM_NShared_GuildArenaBuffDataBase.EncodeJsonObject));
		dictionary.Add("ExtraStatDataBases", BaseJsonMarshaler.EncodeArray(o.ExtraStatDataBases, JM_NShared_DefaultExtraStatData.EncodeJsonObject));
		dictionary.Add("PetStatDataBases", BaseJsonMarshaler.EncodeArray(o.PetStatDataBases, JM_NShared_PetStatDataBase.EncodeJsonObject));
		dictionary.Add("BattleInfo", JM_NShared_PlayerBattleInfo.EncodeJsonObject(o.BattleInfo));
		dictionary.Add("AvgFightingPower", o.AvgFightingPower.ToString());
		dictionary.Add("LuckyDeckIndex", o.LuckyDeckIndex.ToString());
		dictionary.Add("UserInfo", JM_NShared_CachedUserInfo.EncodeJsonObject(o.UserInfo));
		dictionary.Add("HeroInfos", BaseJsonMarshaler.EncodeDictionary(o.HeroInfos, JM_NShared_CachedHeroInfo.EncodeJsonObject));
		dictionary.Add("AiHeroInfos", BaseJsonMarshaler.EncodeDictionary(o.AiHeroInfos, JM_NShared_CachedHeroInfo.EncodeJsonObject));
		dictionary.Add("GroupHeroInfos", BaseJsonMarshaler.EncodeDictionary(o.GroupHeroInfos, JM_NShared_CachedHeroInfo.EncodeJsonObject));
		dictionary.Add("DeckInfos", BaseJsonMarshaler.EncodeDictionary(o.DeckInfos, JM_NShared_DeckInfo.EncodeJsonObject));
		return dictionary;
	}
}

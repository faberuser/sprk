using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_BattleSimulationAccountInfo
{
	public static BattleSimulationAccountInfo Parse(string s)
	{
		return Parse(s, new BattleSimulationAccountInfo());
	}

	public static BattleSimulationAccountInfo Parse(string s, BattleSimulationAccountInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static BattleSimulationAccountInfo Parse(IDictionary d)
	{
		return Parse(d, new BattleSimulationAccountInfo());
	}

	public static BattleSimulationAccountInfo Parse(IDictionary d, BattleSimulationAccountInfo o)
	{
		o.GuildSkills = BaseJsonMarshaler.ParseArray(d, "GuildSkills", JM_NShared_GuildSkillDataBase.Parse);
		o.AccountBuffs = BaseJsonMarshaler.ParseArray(d, "AccountBuffs", JM_NShared_AccountBuffDataBase.Parse);
		o.ClassBuffDataBases = BaseJsonMarshaler.ParseArray(d, "ClassBuffDataBases", JM_NShared_ClassBuffDataBase.Parse);
		o.ExtraStatDatas = BaseJsonMarshaler.ParseArray(d, "ExtraStatDatas", JM_NShared_DefaultExtraStatData.Parse);
		o.PetStatDataBases = BaseJsonMarshaler.ParseArray(d, "PetStatDataBases", JM_NShared_PetStatDataBase.Parse);
		o.UserInfo = BaseJsonMarshaler.ParseObject(d, "UserInfo", JM_NShared_CachedUserInfo.Parse);
		o.HeroInfos = BaseJsonMarshaler.ParseDictionary<int, CachedHeroInfo, Dictionary<int, CachedHeroInfo>>(d, "HeroInfos", JM_NShared_CachedHeroInfo.Parse);
		o.AiHeroInfos = BaseJsonMarshaler.ParseDictionary<int, CachedHeroInfo, Dictionary<int, CachedHeroInfo>>(d, "AiHeroInfos", JM_NShared_CachedHeroInfo.Parse);
		o.GroupHeroInfos = BaseJsonMarshaler.ParseDictionary<int, CachedHeroInfo, Dictionary<int, CachedHeroInfo>>(d, "GroupHeroInfos", JM_NShared_CachedHeroInfo.Parse);
		o.DeckInfos = BaseJsonMarshaler.ParseDictionary<BattleType, DeckInfo, Dictionary<BattleType, DeckInfo>>(d, "DeckInfos", JM_NShared_DeckInfo.Parse);
		return o;
	}

	public static string EncodeJsonString(BattleSimulationAccountInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(BattleSimulationAccountInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("GuildSkills", BaseJsonMarshaler.EncodeArray(o.GuildSkills, JM_NShared_GuildSkillDataBase.EncodeJsonObject));
		dictionary.Add("AccountBuffs", BaseJsonMarshaler.EncodeArray(o.AccountBuffs, JM_NShared_AccountBuffDataBase.EncodeJsonObject));
		dictionary.Add("ClassBuffDataBases", BaseJsonMarshaler.EncodeArray(o.ClassBuffDataBases, JM_NShared_ClassBuffDataBase.EncodeJsonObject));
		dictionary.Add("ExtraStatDatas", BaseJsonMarshaler.EncodeArray(o.ExtraStatDatas, JM_NShared_DefaultExtraStatData.EncodeJsonObject));
		dictionary.Add("PetStatDataBases", BaseJsonMarshaler.EncodeArray(o.PetStatDataBases, JM_NShared_PetStatDataBase.EncodeJsonObject));
		dictionary.Add("UserInfo", JM_NShared_CachedUserInfo.EncodeJsonObject(o.UserInfo));
		dictionary.Add("HeroInfos", BaseJsonMarshaler.EncodeDictionary(o.HeroInfos, JM_NShared_CachedHeroInfo.EncodeJsonObject));
		dictionary.Add("AiHeroInfos", BaseJsonMarshaler.EncodeDictionary(o.AiHeroInfos, JM_NShared_CachedHeroInfo.EncodeJsonObject));
		dictionary.Add("GroupHeroInfos", BaseJsonMarshaler.EncodeDictionary(o.GroupHeroInfos, JM_NShared_CachedHeroInfo.EncodeJsonObject));
		dictionary.Add("DeckInfos", BaseJsonMarshaler.EncodeDictionary(o.DeckInfos, JM_NShared_DeckInfo.EncodeJsonObject));
		return dictionary;
	}
}

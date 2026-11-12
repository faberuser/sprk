using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_BattleCreatureCreateInfo
{
	public static BattleCreatureCreateInfo Parse(string s)
	{
		return Parse(s, new BattleCreatureCreateInfo());
	}

	public static BattleCreatureCreateInfo Parse(string s, BattleCreatureCreateInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static BattleCreatureCreateInfo Parse(IDictionary d)
	{
		return Parse(d, new BattleCreatureCreateInfo());
	}

	public static BattleCreatureCreateInfo Parse(IDictionary d, BattleCreatureCreateInfo o)
	{
		o.HeroInfo = BaseJsonMarshaler.ParseObject(d, "HeroInfo", JM_NShared_CachedHeroInfo.Parse);
		o.StatData = BaseJsonMarshaler.ParseObject(d, "StatData", JM_NShared_StatData.Parse);
		o.Position = BaseJsonMarshaler.ParseObject(d, "Position", JM_NShared_NUtil_Vector3.Parse);
		o.Tier = BaseJsonMarshaler.ParseValueInt(d, "Tier");
		o.Grade = BaseJsonMarshaler.ParseEnum<MonsterGradeType>(d, "Grade");
		o.IsLeader = BaseJsonMarshaler.ParseValueBool(d, "IsLeader");
		o.IsBoss = BaseJsonMarshaler.ParseValueBool(d, "IsBoss");
		o.IsAi = BaseJsonMarshaler.ParseValueBool(d, "IsAi");
		o.IsSmartAi = BaseJsonMarshaler.ParseValueBool(d, "IsSmartAi");
		o.IsPlayer = BaseJsonMarshaler.ParseValueBool(d, "IsPlayer");
		o.IsDummy = BaseJsonMarshaler.ParseValueBool(d, "IsDummy");
		o.IsInitialResting = BaseJsonMarshaler.ParseValueBool(d, "IsInitialResting");
		o.RandomSeed = BaseJsonMarshaler.ParseValueInt(d, "RandomSeed");
		o.WaveSlotIndex = BaseJsonMarshaler.ParseValueInt(d, "WaveSlotIndex");
		o.VisualInfo = BaseJsonMarshaler.ParseValueString(d, "VisualInfo");
		o.CreatureScaleR = BaseJsonMarshaler.ParseValueInt(d, "CreatureScaleR");
		o.CreatureHeightMm = BaseJsonMarshaler.ParseValueInt(d, "CreatureHeightMm");
		o.OffsetMx = BaseJsonMarshaler.ParseValueInt(d, "OffsetMx");
		o.OffsetMz = BaseJsonMarshaler.ParseValueInt(d, "OffsetMz");
		o.DelayMs = BaseJsonMarshaler.ParseValueInt(d, "DelayMs");
		o.AddExtraSkill = BaseJsonMarshaler.ParseArray(d, "AddExtraSkill", JM_NShared_ExtraSkillInfo.Parse);
		o.WeaponIndex = BaseJsonMarshaler.ParseValueInt(d, "WeaponIndex");
		o.GuildSkills = BaseJsonMarshaler.ParseArray(d, "GuildSkills", JM_NShared_GuildSkillDataBase.Parse);
		o.AccountBuffs = BaseJsonMarshaler.ParseArray(d, "AccountBuffs", JM_NShared_AccountBuffDataBase.Parse);
		o.ClassBuffDataBases = BaseJsonMarshaler.ParseArray(d, "ClassBuffDataBases", JM_NShared_ClassBuffDataBase.Parse);
		o.GuildArenaBuffDatasBases = BaseJsonMarshaler.ParseArray(d, "GuildArenaBuffDatasBases", JM_NShared_GuildArenaBuffDataBase.Parse);
		o.OrdealArenaSkills = BaseJsonMarshaler.ParseArray(d, "OrdealArenaSkills", JM_NShared_OrdealArenaSkillDataBase.Parse);
		o.OrdealArenaStats = BaseJsonMarshaler.ParseArray(d, "OrdealArenaStats", JM_NShared_OrdealArenaStatDataBase.Parse);
		o.PetStats = BaseJsonMarshaler.ParseArray(d, "PetStats", JM_NShared_PetStatDataBase.Parse);
		o.ExtraStats = BaseJsonMarshaler.ParseArray(d, "ExtraStats", JM_NShared_DefaultExtraStatData.Parse);
		o.GroupIndex = BaseJsonMarshaler.ParseValueInt(d, "GroupIndex");
		o.FirstSkillSlotIndex = BaseJsonMarshaler.ParseValueInt(d, "FirstSkillSlotIndex");
		o.HasDungeonSkill = BaseJsonMarshaler.ParseValueBool(d, "HasDungeonSkill");
		o.ForceMinHp = BaseJsonMarshaler.ParseValueLong(d, "ForceMinHp");
		return o;
	}

	public static string EncodeJsonString(BattleCreatureCreateInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(BattleCreatureCreateInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("HeroInfo", JM_NShared_CachedHeroInfo.EncodeJsonObject(o.HeroInfo));
		dictionary.Add("StatData", JM_NShared_StatData.EncodeJsonObject(o.StatData));
		dictionary.Add("Position", JM_NShared_NUtil_Vector3.EncodeJsonObject(o.Position));
		dictionary.Add("Tier", o.Tier.ToString());
		dictionary.Add("Grade", o.Grade.ToString());
		dictionary.Add("IsLeader", o.IsLeader.ToString());
		dictionary.Add("IsBoss", o.IsBoss.ToString());
		dictionary.Add("IsAi", o.IsAi.ToString());
		dictionary.Add("IsSmartAi", o.IsSmartAi.ToString());
		dictionary.Add("IsPlayer", o.IsPlayer.ToString());
		dictionary.Add("IsDummy", o.IsDummy.ToString());
		dictionary.Add("IsInitialResting", o.IsInitialResting.ToString());
		dictionary.Add("RandomSeed", o.RandomSeed.ToString());
		dictionary.Add("WaveSlotIndex", o.WaveSlotIndex.ToString());
		if (o.VisualInfo != null)
		{
			dictionary.Add("VisualInfo", o.VisualInfo);
		}
		dictionary.Add("CreatureScaleR", o.CreatureScaleR.ToString());
		dictionary.Add("CreatureHeightMm", o.CreatureHeightMm.ToString());
		dictionary.Add("OffsetMx", o.OffsetMx.ToString());
		dictionary.Add("OffsetMz", o.OffsetMz.ToString());
		dictionary.Add("DelayMs", o.DelayMs.ToString());
		dictionary.Add("AddExtraSkill", BaseJsonMarshaler.EncodeArray(o.AddExtraSkill, JM_NShared_ExtraSkillInfo.EncodeJsonObject));
		dictionary.Add("WeaponIndex", o.WeaponIndex.ToString());
		dictionary.Add("GuildSkills", BaseJsonMarshaler.EncodeArray(o.GuildSkills, JM_NShared_GuildSkillDataBase.EncodeJsonObject));
		dictionary.Add("AccountBuffs", BaseJsonMarshaler.EncodeArray(o.AccountBuffs, JM_NShared_AccountBuffDataBase.EncodeJsonObject));
		dictionary.Add("ClassBuffDataBases", BaseJsonMarshaler.EncodeArray(o.ClassBuffDataBases, JM_NShared_ClassBuffDataBase.EncodeJsonObject));
		dictionary.Add("GuildArenaBuffDatasBases", BaseJsonMarshaler.EncodeArray(o.GuildArenaBuffDatasBases, JM_NShared_GuildArenaBuffDataBase.EncodeJsonObject));
		dictionary.Add("OrdealArenaSkills", BaseJsonMarshaler.EncodeArray(o.OrdealArenaSkills, JM_NShared_OrdealArenaSkillDataBase.EncodeJsonObject));
		dictionary.Add("OrdealArenaStats", BaseJsonMarshaler.EncodeArray(o.OrdealArenaStats, JM_NShared_OrdealArenaStatDataBase.EncodeJsonObject));
		dictionary.Add("PetStats", BaseJsonMarshaler.EncodeArray(o.PetStats, JM_NShared_PetStatDataBase.EncodeJsonObject));
		dictionary.Add("ExtraStats", BaseJsonMarshaler.EncodeArray(o.ExtraStats, JM_NShared_DefaultExtraStatData.EncodeJsonObject));
		dictionary.Add("GroupIndex", o.GroupIndex.ToString());
		dictionary.Add("FirstSkillSlotIndex", o.FirstSkillSlotIndex.ToString());
		dictionary.Add("HasDungeonSkill", o.HasDungeonSkill.ToString());
		dictionary.Add("ForceMinHp", o.ForceMinHp.ToString());
		return dictionary;
	}
}

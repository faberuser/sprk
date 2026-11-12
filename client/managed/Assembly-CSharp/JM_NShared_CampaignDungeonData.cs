using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_CampaignDungeonData
{
	public static CampaignDungeonData Parse(string s)
	{
		return Parse(s, new CampaignDungeonData());
	}

	public static CampaignDungeonData Parse(string s, CampaignDungeonData o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static CampaignDungeonData Parse(IDictionary d)
	{
		return Parse(d, new CampaignDungeonData());
	}

	public static CampaignDungeonData Parse(IDictionary d, CampaignDungeonData o)
	{
		o.ChapterIndex = BaseJsonMarshaler.ParseValueInt(d, "ChapterIndex");
		o.DungeonIndex = BaseJsonMarshaler.ParseValueInt(d, "DungeonIndex");
		o.BattleField = BaseJsonMarshaler.ParseValueHashString(d, "BattleField");
		o.IsExceededEntryChapter = BaseJsonMarshaler.ParseValueBool(d, "IsExceededEntryChapter");
		o.BattleType = BaseJsonMarshaler.ParseEnum<BattleType>(d, "BattleType");
		o.BattleDefineCode = BaseJsonMarshaler.ParseValueHashString(d, "BattleDefineCode");
		o.BgmCode = BaseJsonMarshaler.ParseValueHashString(d, "BgmCode");
		o.Scenario = BaseJsonMarshaler.ParseValueBool(d, "Scenario");
		o.ScenarioEndWave = BaseJsonMarshaler.ParseValueInt(d, "ScenarioEndWave");
		o.ReqScenarioDungeon = BaseJsonMarshaler.ParseArrayCast(d, "ReqScenarioDungeon", BaseJsonMarshaler.CastToInt);
		o.Constant = BaseJsonMarshaler.ParseArrayCast(d, "Constant", BaseJsonMarshaler.CastToString);
		o.ForcedHeroIndex = BaseJsonMarshaler.ParseArrayCast(d, "ForcedHeroIndex", BaseJsonMarshaler.CastToInt);
		o.ScenarioForcedHeroIndex = BaseJsonMarshaler.ParseArrayCast(d, "ScenarioForcedHeroIndex", BaseJsonMarshaler.CastToInt);
		o.RandomSkillProjectileGroup = BaseJsonMarshaler.ParseArrayCast(d, "RandomSkillProjectileGroup", BaseJsonMarshaler.CastToInt);
		o.FirstRewardIndex = BaseJsonMarshaler.ParseValueInt(d, "FirstRewardIndex");
		o.ReqStaminaType = BaseJsonMarshaler.ParseEnum<StaminaType>(d, "ReqStaminaType");
		o.ReqStamina = BaseJsonMarshaler.ParseValueInt(d, "ReqStamina");
		o.AdvancedReqStamina = BaseJsonMarshaler.ParseValueInt(d, "AdvancedReqStamina");
		o.MainSquardCount = BaseJsonMarshaler.ParseValueSByte(d, "MainSquardCount");
		o.SubSquardCount = BaseJsonMarshaler.ParseValueSByte(d, "SubSquardCount");
		o.LocalStorageKey = BaseJsonMarshaler.ParseValueString(d, "LocalStorageKey");
		o.FirstDungeonIndex = BaseJsonMarshaler.ParseValueInt(d, "FirstDungeonIndex");
		o.NextDungeonIndex = BaseJsonMarshaler.ParseValueInt(d, "NextDungeonIndex");
		o.DropRewardIndex_Easy = BaseJsonMarshaler.ParseValueInt(d, "DropRewardIndex_Easy");
		o.CreatureExp_Easy = BaseJsonMarshaler.ParseValueInt(d, "CreatureExp_Easy");
		o.FirstClearRewardIndex_Easy = BaseJsonMarshaler.ParseValueInt(d, "FirstClearRewardIndex_Easy");
		o.FirstClearGem_Easy = BaseJsonMarshaler.ParseValueInt(d, "FirstClearGem_Easy");
		o.ReqStamina_Easy = BaseJsonMarshaler.ParseValueInt(d, "ReqStamina_Easy");
		o.DispatchType_Easy = BaseJsonMarshaler.ParseEnum<DispatchType>(d, "DispatchType_Easy");
		o.DropRewardIndex_Normal = BaseJsonMarshaler.ParseValueInt(d, "DropRewardIndex_Normal");
		o.CreatureExp_Normal = BaseJsonMarshaler.ParseValueInt(d, "CreatureExp_Normal");
		o.FirstClearRewardIndex_Normal = BaseJsonMarshaler.ParseValueInt(d, "FirstClearRewardIndex_Normal");
		o.FirstClearGem_Normal = BaseJsonMarshaler.ParseValueInt(d, "FirstClearGem_Normal");
		o.ReqStamina_Normal = BaseJsonMarshaler.ParseValueInt(d, "ReqStamina_Normal");
		o.DispatchType_Normal = BaseJsonMarshaler.ParseEnum<DispatchType>(d, "DispatchType_Normal");
		o.DropRewardIndex_Hard = BaseJsonMarshaler.ParseValueInt(d, "DropRewardIndex_Hard");
		o.CreatureExp_Hard = BaseJsonMarshaler.ParseValueInt(d, "CreatureExp_Hard");
		o.FirstClearRewardIndex_Hard = BaseJsonMarshaler.ParseValueInt(d, "FirstClearRewardIndex_Hard");
		o.FirstClearGem_Hard = BaseJsonMarshaler.ParseValueInt(d, "FirstClearGem_Hard");
		o.ReqStamina_Hard = BaseJsonMarshaler.ParseValueInt(d, "ReqStamina_Hard");
		o.DispatchType_Hard = BaseJsonMarshaler.ParseEnum<DispatchType>(d, "DispatchType_Hard");
		o.DropRewardIndex_Hell = BaseJsonMarshaler.ParseValueInt(d, "DropRewardIndex_Hell");
		o.CreatureExp_Hell = BaseJsonMarshaler.ParseValueInt(d, "CreatureExp_Hell");
		o.FirstClearRewardIndex_Hell = BaseJsonMarshaler.ParseValueInt(d, "FirstClearRewardIndex_Hell");
		o.FirstClearGem_Hell = BaseJsonMarshaler.ParseValueInt(d, "FirstClearGem_Hell");
		o.ReqStamina_Hell = BaseJsonMarshaler.ParseValueInt(d, "ReqStamina_Hell");
		o.DispatchType_Hell = BaseJsonMarshaler.ParseEnum<DispatchType>(d, "DispatchType_Hell");
		o.CheckClearChapter = BaseJsonMarshaler.ParseValueByte(d, "CheckClearChapter");
		o.Value = BaseJsonMarshaler.ParseArrayCast(d, "Value", BaseJsonMarshaler.CastToInt);
		o.OpenDifficulty = BaseJsonMarshaler.ParseValueBool(d, "OpenDifficulty");
		o.IsBranch = BaseJsonMarshaler.ParseValueBool(d, "IsBranch");
		o.NoDifficulty = BaseJsonMarshaler.ParseValueBool(d, "NoDifficulty");
		o.EGuildFlagDisplayType = BaseJsonMarshaler.ParseEnum<EGuildFlagDisplayType>(d, "EGuildFlagDisplayType");
		o.DispatchDeckCount = BaseJsonMarshaler.ParseValueInt(d, "DispatchDeckCount");
		o.DungeonSkillIndexList = BaseJsonMarshaler.ParseArrayCast(d, "DungeonSkillIndexList", BaseJsonMarshaler.CastToInt);
		o.ScenarioBattleDefine = BaseJsonMarshaler.ParseValueString(d, "ScenarioBattleDefine");
		o.DungeonPassiveEnvironmentIndices = BaseJsonMarshaler.ParseArrayCast(d, "DungeonPassiveEnvironmentIndices", BaseJsonMarshaler.CastToInt);
		o.IsNotInItDailyCount = BaseJsonMarshaler.ParseValueBool(d, "IsNotInItDailyCount");
		o.DeckAttributeCondition = BaseJsonMarshaler.ParseArray<SkillAttrType>(d, "DeckAttributeCondition");
		o.AffixGroupIndex_Easy = BaseJsonMarshaler.ParseValueInt(d, "AffixGroupIndex_Easy");
		o.AffixGroupIndex_Normal = BaseJsonMarshaler.ParseValueInt(d, "AffixGroupIndex_Normal");
		o.AffixGroupIndex_Hard = BaseJsonMarshaler.ParseValueInt(d, "AffixGroupIndex_Hard");
		o.AffixGroupIndex_Hell = BaseJsonMarshaler.ParseValueInt(d, "AffixGroupIndex_Hell");
		o.AffixMultiple_Easy = BaseJsonMarshaler.ParseValueInt(d, "AffixMultiple_Easy");
		o.AffixMultiple_Normal = BaseJsonMarshaler.ParseValueInt(d, "AffixMultiple_Normal");
		o.AffixMultiple_Hard = BaseJsonMarshaler.ParseValueInt(d, "AffixMultiple_Hard");
		o.AffixMultiple_Hell = BaseJsonMarshaler.ParseValueInt(d, "AffixMultiple_Hell");
		o.ClearPoint_Easy = BaseJsonMarshaler.ParseValueInt(d, "ClearPoint_Easy");
		o.ClearPoint_Normal = BaseJsonMarshaler.ParseValueInt(d, "ClearPoint_Normal");
		o.ClearPoint_Hard = BaseJsonMarshaler.ParseValueInt(d, "ClearPoint_Hard");
		o.ClearPoint_Hell = BaseJsonMarshaler.ParseValueInt(d, "ClearPoint_Hell");
		o.SweepDungeonType = BaseJsonMarshaler.ParseEnum<SweepDungeonType>(d, "SweepDungeonType");
		o.SweepTicketCount = BaseJsonMarshaler.ParseValueInt(d, "SweepTicketCount");
		o.SweepTicketMaxCount = BaseJsonMarshaler.ParseValueInt(d, "SweepTicketMaxCount");
		return o;
	}

	public static string EncodeJsonString(CampaignDungeonData o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(CampaignDungeonData o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ChapterIndex", o.ChapterIndex.ToString());
		dictionary.Add("DungeonIndex", o.DungeonIndex.ToString());
		if (o.BattleField != null)
		{
			dictionary.Add("BattleField", o.BattleField.ToString());
		}
		dictionary.Add("IsExceededEntryChapter", o.IsExceededEntryChapter.ToString());
		dictionary.Add("BattleType", o.BattleType.ToString());
		if (o.BattleDefineCode != null)
		{
			dictionary.Add("BattleDefineCode", o.BattleDefineCode.ToString());
		}
		if (o.BgmCode != null)
		{
			dictionary.Add("BgmCode", o.BgmCode.ToString());
		}
		dictionary.Add("Scenario", o.Scenario.ToString());
		dictionary.Add("ScenarioEndWave", o.ScenarioEndWave.ToString());
		dictionary.Add("ReqScenarioDungeon", BaseJsonMarshaler.EncodeArray(o.ReqScenarioDungeon));
		dictionary.Add("Constant", BaseJsonMarshaler.EncodeArray(o.Constant));
		dictionary.Add("ForcedHeroIndex", BaseJsonMarshaler.EncodeArray(o.ForcedHeroIndex));
		dictionary.Add("ScenarioForcedHeroIndex", BaseJsonMarshaler.EncodeArray(o.ScenarioForcedHeroIndex));
		dictionary.Add("RandomSkillProjectileGroup", BaseJsonMarshaler.EncodeArray(o.RandomSkillProjectileGroup));
		dictionary.Add("FirstRewardIndex", o.FirstRewardIndex.ToString());
		dictionary.Add("ReqStaminaType", o.ReqStaminaType.ToString());
		dictionary.Add("ReqStamina", o.ReqStamina.ToString());
		dictionary.Add("AdvancedReqStamina", o.AdvancedReqStamina.ToString());
		dictionary.Add("MainSquardCount", o.MainSquardCount.ToString());
		dictionary.Add("SubSquardCount", o.SubSquardCount.ToString());
		if (o.LocalStorageKey != null)
		{
			dictionary.Add("LocalStorageKey", o.LocalStorageKey);
		}
		dictionary.Add("FirstDungeonIndex", o.FirstDungeonIndex.ToString());
		dictionary.Add("NextDungeonIndex", o.NextDungeonIndex.ToString());
		dictionary.Add("DropRewardIndex_Easy", o.DropRewardIndex_Easy.ToString());
		dictionary.Add("CreatureExp_Easy", o.CreatureExp_Easy.ToString());
		dictionary.Add("FirstClearRewardIndex_Easy", o.FirstClearRewardIndex_Easy.ToString());
		dictionary.Add("FirstClearGem_Easy", o.FirstClearGem_Easy.ToString());
		dictionary.Add("ReqStamina_Easy", o.ReqStamina_Easy.ToString());
		dictionary.Add("DispatchType_Easy", o.DispatchType_Easy.ToString());
		dictionary.Add("DropRewardIndex_Normal", o.DropRewardIndex_Normal.ToString());
		dictionary.Add("CreatureExp_Normal", o.CreatureExp_Normal.ToString());
		dictionary.Add("FirstClearRewardIndex_Normal", o.FirstClearRewardIndex_Normal.ToString());
		dictionary.Add("FirstClearGem_Normal", o.FirstClearGem_Normal.ToString());
		dictionary.Add("ReqStamina_Normal", o.ReqStamina_Normal.ToString());
		dictionary.Add("DispatchType_Normal", o.DispatchType_Normal.ToString());
		dictionary.Add("DropRewardIndex_Hard", o.DropRewardIndex_Hard.ToString());
		dictionary.Add("CreatureExp_Hard", o.CreatureExp_Hard.ToString());
		dictionary.Add("FirstClearRewardIndex_Hard", o.FirstClearRewardIndex_Hard.ToString());
		dictionary.Add("FirstClearGem_Hard", o.FirstClearGem_Hard.ToString());
		dictionary.Add("ReqStamina_Hard", o.ReqStamina_Hard.ToString());
		dictionary.Add("DispatchType_Hard", o.DispatchType_Hard.ToString());
		dictionary.Add("DropRewardIndex_Hell", o.DropRewardIndex_Hell.ToString());
		dictionary.Add("CreatureExp_Hell", o.CreatureExp_Hell.ToString());
		dictionary.Add("FirstClearRewardIndex_Hell", o.FirstClearRewardIndex_Hell.ToString());
		dictionary.Add("FirstClearGem_Hell", o.FirstClearGem_Hell.ToString());
		dictionary.Add("ReqStamina_Hell", o.ReqStamina_Hell.ToString());
		dictionary.Add("DispatchType_Hell", o.DispatchType_Hell.ToString());
		dictionary.Add("CheckClearChapter", o.CheckClearChapter.ToString());
		dictionary.Add("Value", BaseJsonMarshaler.EncodeArray(o.Value));
		dictionary.Add("OpenDifficulty", o.OpenDifficulty.ToString());
		dictionary.Add("IsBranch", o.IsBranch.ToString());
		dictionary.Add("NoDifficulty", o.NoDifficulty.ToString());
		dictionary.Add("EGuildFlagDisplayType", o.EGuildFlagDisplayType.ToString());
		dictionary.Add("DispatchDeckCount", o.DispatchDeckCount.ToString());
		dictionary.Add("DungeonSkillIndexList", BaseJsonMarshaler.EncodeArray(o.DungeonSkillIndexList));
		if (o.ScenarioBattleDefine != null)
		{
			dictionary.Add("ScenarioBattleDefine", o.ScenarioBattleDefine);
		}
		dictionary.Add("DungeonPassiveEnvironmentIndices", BaseJsonMarshaler.EncodeArray(o.DungeonPassiveEnvironmentIndices));
		dictionary.Add("IsNotInItDailyCount", o.IsNotInItDailyCount.ToString());
		dictionary.Add("DeckAttributeCondition", BaseJsonMarshaler.EncodeArray(o.DeckAttributeCondition));
		dictionary.Add("AffixGroupIndex_Easy", o.AffixGroupIndex_Easy.ToString());
		dictionary.Add("AffixGroupIndex_Normal", o.AffixGroupIndex_Normal.ToString());
		dictionary.Add("AffixGroupIndex_Hard", o.AffixGroupIndex_Hard.ToString());
		dictionary.Add("AffixGroupIndex_Hell", o.AffixGroupIndex_Hell.ToString());
		dictionary.Add("AffixMultiple_Easy", o.AffixMultiple_Easy.ToString());
		dictionary.Add("AffixMultiple_Normal", o.AffixMultiple_Normal.ToString());
		dictionary.Add("AffixMultiple_Hard", o.AffixMultiple_Hard.ToString());
		dictionary.Add("AffixMultiple_Hell", o.AffixMultiple_Hell.ToString());
		dictionary.Add("ClearPoint_Easy", o.ClearPoint_Easy.ToString());
		dictionary.Add("ClearPoint_Normal", o.ClearPoint_Normal.ToString());
		dictionary.Add("ClearPoint_Hard", o.ClearPoint_Hard.ToString());
		dictionary.Add("ClearPoint_Hell", o.ClearPoint_Hell.ToString());
		dictionary.Add("SweepDungeonType", o.SweepDungeonType.ToString());
		dictionary.Add("SweepTicketCount", o.SweepTicketCount.ToString());
		dictionary.Add("SweepTicketMaxCount", o.SweepTicketMaxCount.ToString());
		return dictionary;
	}
}

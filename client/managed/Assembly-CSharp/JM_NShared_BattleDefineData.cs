using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_BattleDefineData
{
	public static BattleDefineData Parse(string s)
	{
		return Parse(s, new BattleDefineData());
	}

	public static BattleDefineData Parse(string s, BattleDefineData o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static BattleDefineData Parse(IDictionary d)
	{
		return Parse(d, new BattleDefineData());
	}

	public static BattleDefineData Parse(IDictionary d, BattleDefineData o)
	{
		o.Code = BaseJsonMarshaler.ParseValueHashString(d, "Code");
		o.MaxTime = BaseJsonMarshaler.ParseValueInt(d, "MaxTime");
		o.ExtraTime = BaseJsonMarshaler.ParseValueInt(d, "ExtraTime");
		o.LabelPrefix = BaseJsonMarshaler.ParseValueString(d, "LabelPrefix");
		o.StartMpRatioM = BaseJsonMarshaler.ParseValueInt(d, "StartMpRatioM");
		o.AutoPlayStarCount = BaseJsonMarshaler.ParseValueInt(d, "AutoPlayStarCount");
		o.FastSpeedStarCount = BaseJsonMarshaler.ParseValueInt(d, "FastSpeedStarCount");
		o.AutoRepeatStarCount = BaseJsonMarshaler.ParseValueInt(d, "AutoRepeatStarCount");
		o.ApplyLevelPenalty = BaseJsonMarshaler.ParseValueBool(d, "ApplyLevelPenalty");
		o.FastSkillTooltip = BaseJsonMarshaler.ParseValueBool(d, "FastSkillTooltip");
		o.DenyRaidInvite = BaseJsonMarshaler.ParseValueBool(d, "DenyRaidInvite");
		o.DenyDuel = BaseJsonMarshaler.ParseValueBool(d, "DenyDuel");
		o.HPPercent = BaseJsonMarshaler.ParseValueBool(d, "HPPercent");
		o.DamageMeterFlag = BaseJsonMarshaler.ParseEnum<DamageMeterFlag>(d, "DamageMeterFlag");
		o.DisableBossHUD = BaseJsonMarshaler.ParseValueBool(d, "DisableBossHUD");
		o.HideAutoStartButton = BaseJsonMarshaler.ParseValueBool(d, "HideAutoStartButton");
		o.RetryCondition = BaseJsonMarshaler.ParseArray<BattleRetryConditionType>(d, "RetryCondition");
		o.BattleCameraName = BaseJsonMarshaler.ParseValueString(d, "BattleCameraName");
		o.IgnoreDropItem = BaseJsonMarshaler.ParseValueBool(d, "IgnoreDropItem");
		o.ModStatType1 = BaseJsonMarshaler.ParseEnum<StatType>(d, "ModStatType1");
		o.ModStatRatioM1 = BaseJsonMarshaler.ParseValueInt(d, "ModStatRatioM1");
		o.ModStatType2 = BaseJsonMarshaler.ParseEnum<StatType>(d, "ModStatType2");
		o.ModStatRatioM2 = BaseJsonMarshaler.ParseValueInt(d, "ModStatRatioM2");
		o.ModStatType3 = BaseJsonMarshaler.ParseEnum<StatType>(d, "ModStatType3");
		o.ModStatRatioM3 = BaseJsonMarshaler.ParseValueInt(d, "ModStatRatioM3");
		o.ModStatType4 = BaseJsonMarshaler.ParseEnum<StatType>(d, "ModStatType4");
		o.ModStatRatioM4 = BaseJsonMarshaler.ParseValueInt(d, "ModStatRatioM4");
		o.ModStatType5 = BaseJsonMarshaler.ParseEnum<StatType>(d, "ModStatType5");
		o.ModStatRatioM5 = BaseJsonMarshaler.ParseValueInt(d, "ModStatRatioM5");
		o.BattleMenu = BaseJsonMarshaler.ParseEnum<EBattleMenu>(d, "BattleMenu");
		o.SkillCameraOff = BaseJsonMarshaler.ParseValueBool(d, "SkillCameraOff");
		o.DisableInventoryInResult = BaseJsonMarshaler.ParseValueBool(d, "DisableInventoryInResult");
		o.DisablePartySettingInResult = BaseJsonMarshaler.ParseValueBool(d, "DisablePartySettingInResult");
		o.DisableHeroListInResult = BaseJsonMarshaler.ParseValueBool(d, "DisableHeroListInResult");
		o.ExInfoTypeOnPartySetting = BaseJsonMarshaler.ParseEnum<ExInfoType>(d, "ExInfoTypeOnPartySetting");
		o.DisableLastKill = BaseJsonMarshaler.ParseValueBool(d, "DisableLastKill");
		o.ShowBossManaGauge = BaseJsonMarshaler.ParseValueBool(d, "ShowBossManaGauge");
		o.CameraFarClipPlane = BaseJsonMarshaler.ParseValueInt(d, "CameraFarClipPlane");
		o.EnhanceTimeSyncMode = BaseJsonMarshaler.ParseValueBool(d, "EnhanceTimeSyncMode");
		o.BetterBossHpUI = BaseJsonMarshaler.ParseValueBool(d, "BetterBossHpUI");
		return o;
	}

	public static string EncodeJsonString(BattleDefineData o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(BattleDefineData o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.Code != null)
		{
			dictionary.Add("Code", o.Code.ToString());
		}
		dictionary.Add("MaxTime", o.MaxTime.ToString());
		dictionary.Add("ExtraTime", o.ExtraTime.ToString());
		if (o.LabelPrefix != null)
		{
			dictionary.Add("LabelPrefix", o.LabelPrefix);
		}
		dictionary.Add("StartMpRatioM", o.StartMpRatioM.ToString());
		dictionary.Add("AutoPlayStarCount", o.AutoPlayStarCount.ToString());
		dictionary.Add("FastSpeedStarCount", o.FastSpeedStarCount.ToString());
		dictionary.Add("AutoRepeatStarCount", o.AutoRepeatStarCount.ToString());
		dictionary.Add("ApplyLevelPenalty", o.ApplyLevelPenalty.ToString());
		dictionary.Add("FastSkillTooltip", o.FastSkillTooltip.ToString());
		dictionary.Add("DenyRaidInvite", o.DenyRaidInvite.ToString());
		dictionary.Add("DenyDuel", o.DenyDuel.ToString());
		dictionary.Add("HPPercent", o.HPPercent.ToString());
		dictionary.Add("DamageMeterFlag", o.DamageMeterFlag.ToString());
		dictionary.Add("DisableBossHUD", o.DisableBossHUD.ToString());
		dictionary.Add("HideAutoStartButton", o.HideAutoStartButton.ToString());
		dictionary.Add("RetryCondition", BaseJsonMarshaler.EncodeArray(o.RetryCondition));
		if (o.BattleCameraName != null)
		{
			dictionary.Add("BattleCameraName", o.BattleCameraName);
		}
		dictionary.Add("IgnoreDropItem", o.IgnoreDropItem.ToString());
		dictionary.Add("ModStatType1", o.ModStatType1.ToString());
		dictionary.Add("ModStatRatioM1", o.ModStatRatioM1.ToString());
		dictionary.Add("ModStatType2", o.ModStatType2.ToString());
		dictionary.Add("ModStatRatioM2", o.ModStatRatioM2.ToString());
		dictionary.Add("ModStatType3", o.ModStatType3.ToString());
		dictionary.Add("ModStatRatioM3", o.ModStatRatioM3.ToString());
		dictionary.Add("ModStatType4", o.ModStatType4.ToString());
		dictionary.Add("ModStatRatioM4", o.ModStatRatioM4.ToString());
		dictionary.Add("ModStatType5", o.ModStatType5.ToString());
		dictionary.Add("ModStatRatioM5", o.ModStatRatioM5.ToString());
		dictionary.Add("BattleMenu", o.BattleMenu.ToString());
		dictionary.Add("SkillCameraOff", o.SkillCameraOff.ToString());
		dictionary.Add("DisableInventoryInResult", o.DisableInventoryInResult.ToString());
		dictionary.Add("DisablePartySettingInResult", o.DisablePartySettingInResult.ToString());
		dictionary.Add("DisableHeroListInResult", o.DisableHeroListInResult.ToString());
		dictionary.Add("ExInfoTypeOnPartySetting", o.ExInfoTypeOnPartySetting.ToString());
		dictionary.Add("DisableLastKill", o.DisableLastKill.ToString());
		dictionary.Add("ShowBossManaGauge", o.ShowBossManaGauge.ToString());
		dictionary.Add("CameraFarClipPlane", o.CameraFarClipPlane.ToString());
		dictionary.Add("EnhanceTimeSyncMode", o.EnhanceTimeSyncMode.ToString());
		dictionary.Add("BetterBossHpUI", o.BetterBossHpUI.ToString());
		return dictionary;
	}
}

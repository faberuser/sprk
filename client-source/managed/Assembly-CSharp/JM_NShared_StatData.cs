using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_StatData
{
	public static StatData Parse(string s)
	{
		return Parse(s, new StatData());
	}

	public static StatData Parse(string s, StatData o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static StatData Parse(IDictionary d)
	{
		return Parse(d, new StatData());
	}

	public static StatData Parse(IDictionary d, StatData o)
	{
		o.PhysicalCriticalChance = BaseJsonMarshaler.ParseValueShort(d, "PhysicalCriticalChance");
		o.PhysicalCriticalPower = BaseJsonMarshaler.ParseValueShort(d, "PhysicalCriticalPower");
		o.PhysicalDodgeChance = BaseJsonMarshaler.ParseValueShort(d, "PhysicalDodgeChance");
		o.PhysicalBlockChance = BaseJsonMarshaler.ParseValueShort(d, "PhysicalBlockChance");
		o.PhysicalBlockPower = BaseJsonMarshaler.ParseValueShort(d, "PhysicalBlockPower");
		o.PhysicalHitChance = BaseJsonMarshaler.ParseValueShort(d, "PhysicalHitChance");
		o.PhysicalPiercePower = BaseJsonMarshaler.ParseValueShort(d, "PhysicalPiercePower");
		o.PhysicalWeakness = BaseJsonMarshaler.ParseValueShort(d, "PhysicalWeakness");
		o.PhysicalToughness = BaseJsonMarshaler.ParseValueShort(d, "PhysicalToughness");
		o.PhysicalCriticalDodgeChance = BaseJsonMarshaler.ParseValueShort(d, "PhysicalCriticalDodgeChance");
		o.PhysicalAttackPower = BaseJsonMarshaler.ParseValueShort(d, "PhysicalAttackPower");
		o.PhysicalDefensePower = BaseJsonMarshaler.ParseValueShort(d, "PhysicalDefensePower");
		o.PhysicalDotPower = BaseJsonMarshaler.ParseValueShort(d, "PhysicalDotPower");
		o.PhysicalIgnoreBlockPower = BaseJsonMarshaler.ParseValueShort(d, "PhysicalIgnoreBlockPower");
		o.PhysicalIgnoreDefensePower = BaseJsonMarshaler.ParseValueShort(d, "PhysicalIgnoreDefensePower");
		o.MagicalCriticalChance = BaseJsonMarshaler.ParseValueShort(d, "MagicalCriticalChance");
		o.MagicalCriticalPower = BaseJsonMarshaler.ParseValueShort(d, "MagicalCriticalPower");
		o.MagicalDodgeChance = BaseJsonMarshaler.ParseValueShort(d, "MagicalDodgeChance");
		o.MagicalBlockChance = BaseJsonMarshaler.ParseValueShort(d, "MagicalBlockChance");
		o.MagicalBlockPower = BaseJsonMarshaler.ParseValueShort(d, "MagicalBlockPower");
		o.MagicalHitChance = BaseJsonMarshaler.ParseValueShort(d, "MagicalHitChance");
		o.MagicalPiercePower = BaseJsonMarshaler.ParseValueShort(d, "MagicalPiercePower");
		o.MagicalWeakness = BaseJsonMarshaler.ParseValueShort(d, "MagicalWeakness");
		o.MagicalToughness = BaseJsonMarshaler.ParseValueShort(d, "MagicalToughness");
		o.MagicalCriticalDodgeChance = BaseJsonMarshaler.ParseValueShort(d, "MagicalCriticalDodgeChance");
		o.HealPower = BaseJsonMarshaler.ParseValueShort(d, "HealPower");
		o.RecoverPower = BaseJsonMarshaler.ParseValueShort(d, "RecoverPower");
		o.MagicalAttackPower = BaseJsonMarshaler.ParseValueShort(d, "MagicalAttackPower");
		o.MagicalDefensePower = BaseJsonMarshaler.ParseValueShort(d, "MagicalDefensePower");
		o.MagicalDotPower = BaseJsonMarshaler.ParseValueShort(d, "MagicalDotPower");
		o.MagicalIgnoreBlockPower = BaseJsonMarshaler.ParseValueShort(d, "MagicalIgnoreBlockPower");
		o.MagicalIgnoreDefensePower = BaseJsonMarshaler.ParseValueShort(d, "MagicalIgnoreDefensePower");
		o.AntiCcChance = BaseJsonMarshaler.ParseValueShort(d, "AntiCcChance");
		o.AntiDebuffChance = BaseJsonMarshaler.ParseValueShort(d, "AntiDebuffChance");
		o.HitCcChance = BaseJsonMarshaler.ParseValueShort(d, "HitCcChance");
		o.HitDebuffChance = BaseJsonMarshaler.ParseValueShort(d, "HitDebuffChance");
		o.HpStealPower = BaseJsonMarshaler.ParseValueInt(d, "HpStealPower");
		o.HpOnDamage = BaseJsonMarshaler.ParseValueInt(d, "HpOnDamage");
		o.HpOnAttack = BaseJsonMarshaler.ParseValueInt(d, "HpOnAttack");
		o.HpOnKill = BaseJsonMarshaler.ParseValueInt(d, "HpOnKill");
		o.HpOnWave = BaseJsonMarshaler.ParseValueInt(d, "HpOnWave");
		o.HpOnTime = BaseJsonMarshaler.ParseValueInt(d, "HpOnTime");
		o.HpRatioOnDamage = BaseJsonMarshaler.ParseValueShort(d, "HpRatioOnDamage");
		o.HpRatioOnAttack = BaseJsonMarshaler.ParseValueShort(d, "HpRatioOnAttack");
		o.HpRatioOnKill = BaseJsonMarshaler.ParseValueShort(d, "HpRatioOnKill");
		o.HpRatioOnWave = BaseJsonMarshaler.ParseValueShort(d, "HpRatioOnWave");
		o.HpRatioOnTime = BaseJsonMarshaler.ParseValueShort(d, "HpRatioOnTime");
		o.MaxMp = BaseJsonMarshaler.ParseValueInt(d, "MaxMp");
		o.MpOnDamage = BaseJsonMarshaler.ParseValueShort(d, "MpOnDamage");
		o.MpOnAttack = BaseJsonMarshaler.ParseValueShort(d, "MpOnAttack");
		o.MpOnKill = BaseJsonMarshaler.ParseValueShort(d, "MpOnKill");
		o.MpOnWave = BaseJsonMarshaler.ParseValueShort(d, "MpOnWave");
		o.MpOnTime = BaseJsonMarshaler.ParseValueShort(d, "MpOnTime");
		o.MpRatioOnDamage = BaseJsonMarshaler.ParseValueShort(d, "MpRatioOnDamage");
		o.MpRatioOnAttack = BaseJsonMarshaler.ParseValueShort(d, "MpRatioOnAttack");
		o.MpRatioOnTime = BaseJsonMarshaler.ParseValueShort(d, "MpRatioOnTime");
		o.MpRegenRatioM = BaseJsonMarshaler.ParseValueShort(d, "MpRegenRatioM");
		o.AttackSpeed = BaseJsonMarshaler.ParseValueInt(d, "AttackSpeed");
		o.MoveSpeedMms = BaseJsonMarshaler.ParseValueInt(d, "MoveSpeedMms");
		o.ShieldPowerR = BaseJsonMarshaler.ParseValueInt(d, "ShieldPowerR");
		o.AddMaxHpR = BaseJsonMarshaler.ParseValueInt(d, "AddMaxHpR");
		o.AddPhysicalAttackR = BaseJsonMarshaler.ParseValueInt(d, "AddPhysicalAttackR");
		o.AddPhysicalDefenseR = BaseJsonMarshaler.ParseValueInt(d, "AddPhysicalDefenseR");
		o.AddMagicalAttackR = BaseJsonMarshaler.ParseValueInt(d, "AddMagicalAttackR");
		o.AddMagicalDefenseR = BaseJsonMarshaler.ParseValueInt(d, "AddMagicalDefenseR");
		o.AddMpOnDamageR = BaseJsonMarshaler.ParseValueInt(d, "AddMpOnDamageR");
		o.AddMpOnAttackR = BaseJsonMarshaler.ParseValueInt(d, "AddMpOnAttackR");
		o.AddMpOnTimeR = BaseJsonMarshaler.ParseValueInt(d, "AddMpOnTimeR");
		o.SkillLevelAllSlot = BaseJsonMarshaler.ParseValueInt(d, "SkillLevelAllSlot");
		o.SkillLevelSlot1 = BaseJsonMarshaler.ParseValueInt(d, "SkillLevelSlot1");
		o.SkillLevelSlot2 = BaseJsonMarshaler.ParseValueInt(d, "SkillLevelSlot2");
		o.SkillLevelSlot3 = BaseJsonMarshaler.ParseValueInt(d, "SkillLevelSlot3");
		o.SkillLevelSlot4 = BaseJsonMarshaler.ParseValueInt(d, "SkillLevelSlot4");
		o.SoulWeaponMaxUseCount = BaseJsonMarshaler.ParseValueInt(d, "SoulWeaponMaxUseCount");
		o.ThreatGain = BaseJsonMarshaler.ParseValueInt(d, "ThreatGain");
		o.ThreatGainR = BaseJsonMarshaler.ParseValueInt(d, "ThreatGainR");
		o.PhysicalAttack = BaseJsonMarshaler.ParseValueInt(d, "PhysicalAttack");
		o.MagicalAttack = BaseJsonMarshaler.ParseValueInt(d, "MagicalAttack");
		o.MaxHp = BaseJsonMarshaler.ParseValueLong(d, "MaxHp");
		o.PhysicalDefense = BaseJsonMarshaler.ParseValueInt(d, "PhysicalDefense");
		o.MagicalDefense = BaseJsonMarshaler.ParseValueInt(d, "MagicalDefense");
		return o;
	}

	public static string EncodeJsonString(StatData o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(StatData o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("PhysicalCriticalChance", o.PhysicalCriticalChance.ToString());
		dictionary.Add("PhysicalCriticalPower", o.PhysicalCriticalPower.ToString());
		dictionary.Add("PhysicalDodgeChance", o.PhysicalDodgeChance.ToString());
		dictionary.Add("PhysicalBlockChance", o.PhysicalBlockChance.ToString());
		dictionary.Add("PhysicalBlockPower", o.PhysicalBlockPower.ToString());
		dictionary.Add("PhysicalHitChance", o.PhysicalHitChance.ToString());
		dictionary.Add("PhysicalPiercePower", o.PhysicalPiercePower.ToString());
		dictionary.Add("PhysicalWeakness", o.PhysicalWeakness.ToString());
		dictionary.Add("PhysicalToughness", o.PhysicalToughness.ToString());
		dictionary.Add("PhysicalCriticalDodgeChance", o.PhysicalCriticalDodgeChance.ToString());
		dictionary.Add("PhysicalAttackPower", o.PhysicalAttackPower.ToString());
		dictionary.Add("PhysicalDefensePower", o.PhysicalDefensePower.ToString());
		dictionary.Add("PhysicalDotPower", o.PhysicalDotPower.ToString());
		dictionary.Add("PhysicalIgnoreBlockPower", o.PhysicalIgnoreBlockPower.ToString());
		dictionary.Add("PhysicalIgnoreDefensePower", o.PhysicalIgnoreDefensePower.ToString());
		dictionary.Add("MagicalCriticalChance", o.MagicalCriticalChance.ToString());
		dictionary.Add("MagicalCriticalPower", o.MagicalCriticalPower.ToString());
		dictionary.Add("MagicalDodgeChance", o.MagicalDodgeChance.ToString());
		dictionary.Add("MagicalBlockChance", o.MagicalBlockChance.ToString());
		dictionary.Add("MagicalBlockPower", o.MagicalBlockPower.ToString());
		dictionary.Add("MagicalHitChance", o.MagicalHitChance.ToString());
		dictionary.Add("MagicalPiercePower", o.MagicalPiercePower.ToString());
		dictionary.Add("MagicalWeakness", o.MagicalWeakness.ToString());
		dictionary.Add("MagicalToughness", o.MagicalToughness.ToString());
		dictionary.Add("MagicalCriticalDodgeChance", o.MagicalCriticalDodgeChance.ToString());
		dictionary.Add("HealPower", o.HealPower.ToString());
		dictionary.Add("RecoverPower", o.RecoverPower.ToString());
		dictionary.Add("MagicalAttackPower", o.MagicalAttackPower.ToString());
		dictionary.Add("MagicalDefensePower", o.MagicalDefensePower.ToString());
		dictionary.Add("MagicalDotPower", o.MagicalDotPower.ToString());
		dictionary.Add("MagicalIgnoreBlockPower", o.MagicalIgnoreBlockPower.ToString());
		dictionary.Add("MagicalIgnoreDefensePower", o.MagicalIgnoreDefensePower.ToString());
		dictionary.Add("AntiCcChance", o.AntiCcChance.ToString());
		dictionary.Add("AntiDebuffChance", o.AntiDebuffChance.ToString());
		dictionary.Add("HitCcChance", o.HitCcChance.ToString());
		dictionary.Add("HitDebuffChance", o.HitDebuffChance.ToString());
		dictionary.Add("HpStealPower", o.HpStealPower.ToString());
		dictionary.Add("HpOnDamage", o.HpOnDamage.ToString());
		dictionary.Add("HpOnAttack", o.HpOnAttack.ToString());
		dictionary.Add("HpOnKill", o.HpOnKill.ToString());
		dictionary.Add("HpOnWave", o.HpOnWave.ToString());
		dictionary.Add("HpOnTime", o.HpOnTime.ToString());
		dictionary.Add("HpRatioOnDamage", o.HpRatioOnDamage.ToString());
		dictionary.Add("HpRatioOnAttack", o.HpRatioOnAttack.ToString());
		dictionary.Add("HpRatioOnKill", o.HpRatioOnKill.ToString());
		dictionary.Add("HpRatioOnWave", o.HpRatioOnWave.ToString());
		dictionary.Add("HpRatioOnTime", o.HpRatioOnTime.ToString());
		dictionary.Add("MaxMp", o.MaxMp.ToString());
		dictionary.Add("MpOnDamage", o.MpOnDamage.ToString());
		dictionary.Add("MpOnAttack", o.MpOnAttack.ToString());
		dictionary.Add("MpOnKill", o.MpOnKill.ToString());
		dictionary.Add("MpOnWave", o.MpOnWave.ToString());
		dictionary.Add("MpOnTime", o.MpOnTime.ToString());
		dictionary.Add("MpRatioOnDamage", o.MpRatioOnDamage.ToString());
		dictionary.Add("MpRatioOnAttack", o.MpRatioOnAttack.ToString());
		dictionary.Add("MpRatioOnTime", o.MpRatioOnTime.ToString());
		dictionary.Add("MpRegenRatioM", o.MpRegenRatioM.ToString());
		dictionary.Add("AttackSpeed", o.AttackSpeed.ToString());
		dictionary.Add("MoveSpeedMms", o.MoveSpeedMms.ToString());
		dictionary.Add("ShieldPowerR", o.ShieldPowerR.ToString());
		dictionary.Add("AddMaxHpR", o.AddMaxHpR.ToString());
		dictionary.Add("AddPhysicalAttackR", o.AddPhysicalAttackR.ToString());
		dictionary.Add("AddPhysicalDefenseR", o.AddPhysicalDefenseR.ToString());
		dictionary.Add("AddMagicalAttackR", o.AddMagicalAttackR.ToString());
		dictionary.Add("AddMagicalDefenseR", o.AddMagicalDefenseR.ToString());
		dictionary.Add("AddMpOnDamageR", o.AddMpOnDamageR.ToString());
		dictionary.Add("AddMpOnAttackR", o.AddMpOnAttackR.ToString());
		dictionary.Add("AddMpOnTimeR", o.AddMpOnTimeR.ToString());
		dictionary.Add("SkillLevelAllSlot", o.SkillLevelAllSlot.ToString());
		dictionary.Add("SkillLevelSlot1", o.SkillLevelSlot1.ToString());
		dictionary.Add("SkillLevelSlot2", o.SkillLevelSlot2.ToString());
		dictionary.Add("SkillLevelSlot3", o.SkillLevelSlot3.ToString());
		dictionary.Add("SkillLevelSlot4", o.SkillLevelSlot4.ToString());
		dictionary.Add("SoulWeaponMaxUseCount", o.SoulWeaponMaxUseCount.ToString());
		dictionary.Add("ThreatGain", o.ThreatGain.ToString());
		dictionary.Add("ThreatGainR", o.ThreatGainR.ToString());
		dictionary.Add("PhysicalAttack", o.PhysicalAttack.ToString());
		dictionary.Add("MagicalAttack", o.MagicalAttack.ToString());
		dictionary.Add("MaxHp", o.MaxHp.ToString());
		dictionary.Add("PhysicalDefense", o.PhysicalDefense.ToString());
		dictionary.Add("MagicalDefense", o.MagicalDefense.ToString());
		return dictionary;
	}
}

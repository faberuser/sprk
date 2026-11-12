using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class StatDataFormatter : IMessagePackFormatter<StatData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, StatData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		writer.WriteArrayHeader(203);
		writer.Write(value.PhysicalAttack);
		writer.Write(value.MagicalAttack);
		writer.Write(value.MaxHp);
		writer.Write(value.PhysicalDefense);
		writer.Write(value.MagicalDefense);
		writer.Write(value.PhysicalCriticalChance);
		writer.Write(value.PhysicalCriticalPower);
		writer.Write(value.PhysicalDodgeChance);
		writer.Write(value.PhysicalBlockChance);
		writer.Write(value.PhysicalBlockPower);
		writer.Write(value.PhysicalHitChance);
		writer.Write(value.PhysicalPiercePower);
		writer.Write(value.PhysicalWeakness);
		writer.Write(value.PhysicalToughness);
		writer.Write(value.PhysicalCriticalDodgeChance);
		writer.Write(value.PhysicalAttackPower);
		writer.Write(value.PhysicalDefensePower);
		writer.Write(value.PhysicalDotPower);
		writer.Write(value.PhysicalIgnoreBlockPower);
		writer.Write(value.PhysicalIgnoreDefensePower);
		writer.Write(value.MagicalCriticalChance);
		writer.Write(value.MagicalCriticalPower);
		writer.Write(value.MagicalDodgeChance);
		writer.Write(value.MagicalBlockChance);
		writer.Write(value.MagicalBlockPower);
		writer.Write(value.MagicalHitChance);
		writer.Write(value.MagicalPiercePower);
		writer.Write(value.MagicalWeakness);
		writer.Write(value.MagicalToughness);
		writer.Write(value.MagicalCriticalDodgeChance);
		writer.Write(value.HealPower);
		writer.Write(value.RecoverPower);
		writer.Write(value.MagicalAttackPower);
		writer.Write(value.MagicalDefensePower);
		writer.Write(value.MagicalDotPower);
		writer.Write(value.MagicalIgnoreBlockPower);
		writer.Write(value.MagicalIgnoreDefensePower);
		writer.Write(value.AntiCcChance);
		writer.Write(value.AntiDebuffChance);
		writer.Write(value.HitCcChance);
		writer.Write(value.HitDebuffChance);
		writer.Write(value.HpStealPower);
		writer.Write(value.HpOnDamage);
		writer.Write(value.HpOnAttack);
		writer.Write(value.HpOnKill);
		writer.Write(value.HpOnWave);
		writer.Write(value.HpOnTime);
		writer.Write(value.HpRatioOnDamage);
		writer.Write(value.HpRatioOnAttack);
		writer.Write(value.HpRatioOnKill);
		writer.Write(value.HpRatioOnWave);
		writer.Write(value.HpRatioOnTime);
		writer.Write(value.MaxMp);
		writer.Write(value.MpOnDamage);
		writer.Write(value.MpOnAttack);
		writer.Write(value.MpOnKill);
		writer.Write(value.MpOnWave);
		writer.Write(value.MpOnTime);
		writer.Write(value.MpRatioOnDamage);
		writer.Write(value.MpRatioOnAttack);
		writer.Write(value.MpRatioOnTime);
		writer.Write(value.MpRegenRatioM);
		writer.Write(value.AttackSpeed);
		writer.Write(value.MoveSpeedMms);
		writer.Write(value.ShieldPowerR);
		writer.Write(value.AddMaxHpR);
		writer.Write(value.AddPhysicalAttackR);
		writer.Write(value.AddPhysicalDefenseR);
		writer.Write(value.AddMagicalAttackR);
		writer.Write(value.AddMagicalDefenseR);
		writer.Write(value.AddMpOnDamageR);
		writer.Write(value.AddMpOnAttackR);
		writer.Write(value.AddMpOnTimeR);
		writer.Write(value.SkillLevelAllSlot);
		writer.Write(value.SkillLevelSlot1);
		writer.Write(value.SkillLevelSlot2);
		writer.Write(value.SkillLevelSlot3);
		writer.Write(value.SkillLevelSlot4);
		writer.Write(value.SoulWeaponMaxUseCount);
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.Write(value.ThreatGain);
		writer.Write(value.ThreatGainR);
	}

	public StatData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		StatData statData = new StatData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				statData.PhysicalAttack = reader.ReadInt32();
				break;
			case 1:
				statData.MagicalAttack = reader.ReadInt32();
				break;
			case 2:
				statData.MaxHp = reader.ReadInt64();
				break;
			case 3:
				statData.PhysicalDefense = reader.ReadInt32();
				break;
			case 4:
				statData.MagicalDefense = reader.ReadInt32();
				break;
			case 5:
				statData.PhysicalCriticalChance = reader.ReadInt16();
				break;
			case 6:
				statData.PhysicalCriticalPower = reader.ReadInt16();
				break;
			case 7:
				statData.PhysicalDodgeChance = reader.ReadInt16();
				break;
			case 8:
				statData.PhysicalBlockChance = reader.ReadInt16();
				break;
			case 9:
				statData.PhysicalBlockPower = reader.ReadInt16();
				break;
			case 10:
				statData.PhysicalHitChance = reader.ReadInt16();
				break;
			case 11:
				statData.PhysicalPiercePower = reader.ReadInt16();
				break;
			case 12:
				statData.PhysicalWeakness = reader.ReadInt16();
				break;
			case 13:
				statData.PhysicalToughness = reader.ReadInt16();
				break;
			case 14:
				statData.PhysicalCriticalDodgeChance = reader.ReadInt16();
				break;
			case 15:
				statData.PhysicalAttackPower = reader.ReadInt16();
				break;
			case 16:
				statData.PhysicalDefensePower = reader.ReadInt16();
				break;
			case 17:
				statData.PhysicalDotPower = reader.ReadInt16();
				break;
			case 18:
				statData.PhysicalIgnoreBlockPower = reader.ReadInt16();
				break;
			case 19:
				statData.PhysicalIgnoreDefensePower = reader.ReadInt16();
				break;
			case 20:
				statData.MagicalCriticalChance = reader.ReadInt16();
				break;
			case 21:
				statData.MagicalCriticalPower = reader.ReadInt16();
				break;
			case 22:
				statData.MagicalDodgeChance = reader.ReadInt16();
				break;
			case 23:
				statData.MagicalBlockChance = reader.ReadInt16();
				break;
			case 24:
				statData.MagicalBlockPower = reader.ReadInt16();
				break;
			case 25:
				statData.MagicalHitChance = reader.ReadInt16();
				break;
			case 26:
				statData.MagicalPiercePower = reader.ReadInt16();
				break;
			case 27:
				statData.MagicalWeakness = reader.ReadInt16();
				break;
			case 28:
				statData.MagicalToughness = reader.ReadInt16();
				break;
			case 29:
				statData.MagicalCriticalDodgeChance = reader.ReadInt16();
				break;
			case 30:
				statData.HealPower = reader.ReadInt16();
				break;
			case 31:
				statData.RecoverPower = reader.ReadInt16();
				break;
			case 32:
				statData.MagicalAttackPower = reader.ReadInt16();
				break;
			case 33:
				statData.MagicalDefensePower = reader.ReadInt16();
				break;
			case 34:
				statData.MagicalDotPower = reader.ReadInt16();
				break;
			case 35:
				statData.MagicalIgnoreBlockPower = reader.ReadInt16();
				break;
			case 36:
				statData.MagicalIgnoreDefensePower = reader.ReadInt16();
				break;
			case 37:
				statData.AntiCcChance = reader.ReadInt16();
				break;
			case 38:
				statData.AntiDebuffChance = reader.ReadInt16();
				break;
			case 39:
				statData.HitCcChance = reader.ReadInt16();
				break;
			case 40:
				statData.HitDebuffChance = reader.ReadInt16();
				break;
			case 41:
				statData.HpStealPower = reader.ReadInt32();
				break;
			case 42:
				statData.HpOnDamage = reader.ReadInt32();
				break;
			case 43:
				statData.HpOnAttack = reader.ReadInt32();
				break;
			case 44:
				statData.HpOnKill = reader.ReadInt32();
				break;
			case 45:
				statData.HpOnWave = reader.ReadInt32();
				break;
			case 46:
				statData.HpOnTime = reader.ReadInt32();
				break;
			case 47:
				statData.HpRatioOnDamage = reader.ReadInt16();
				break;
			case 48:
				statData.HpRatioOnAttack = reader.ReadInt16();
				break;
			case 49:
				statData.HpRatioOnKill = reader.ReadInt16();
				break;
			case 50:
				statData.HpRatioOnWave = reader.ReadInt16();
				break;
			case 51:
				statData.HpRatioOnTime = reader.ReadInt16();
				break;
			case 52:
				statData.MaxMp = reader.ReadInt32();
				break;
			case 53:
				statData.MpOnDamage = reader.ReadInt16();
				break;
			case 54:
				statData.MpOnAttack = reader.ReadInt16();
				break;
			case 55:
				statData.MpOnKill = reader.ReadInt16();
				break;
			case 56:
				statData.MpOnWave = reader.ReadInt16();
				break;
			case 57:
				statData.MpOnTime = reader.ReadInt16();
				break;
			case 58:
				statData.MpRatioOnDamage = reader.ReadInt16();
				break;
			case 59:
				statData.MpRatioOnAttack = reader.ReadInt16();
				break;
			case 60:
				statData.MpRatioOnTime = reader.ReadInt16();
				break;
			case 61:
				statData.MpRegenRatioM = reader.ReadInt16();
				break;
			case 62:
				statData.AttackSpeed = reader.ReadInt32();
				break;
			case 63:
				statData.MoveSpeedMms = reader.ReadInt32();
				break;
			case 64:
				statData.ShieldPowerR = reader.ReadInt32();
				break;
			case 65:
				statData.AddMaxHpR = reader.ReadInt32();
				break;
			case 66:
				statData.AddPhysicalAttackR = reader.ReadInt32();
				break;
			case 67:
				statData.AddPhysicalDefenseR = reader.ReadInt32();
				break;
			case 68:
				statData.AddMagicalAttackR = reader.ReadInt32();
				break;
			case 69:
				statData.AddMagicalDefenseR = reader.ReadInt32();
				break;
			case 70:
				statData.AddMpOnDamageR = reader.ReadInt32();
				break;
			case 71:
				statData.AddMpOnAttackR = reader.ReadInt32();
				break;
			case 72:
				statData.AddMpOnTimeR = reader.ReadInt32();
				break;
			case 73:
				statData.SkillLevelAllSlot = reader.ReadInt32();
				break;
			case 74:
				statData.SkillLevelSlot1 = reader.ReadInt32();
				break;
			case 75:
				statData.SkillLevelSlot2 = reader.ReadInt32();
				break;
			case 76:
				statData.SkillLevelSlot3 = reader.ReadInt32();
				break;
			case 77:
				statData.SkillLevelSlot4 = reader.ReadInt32();
				break;
			case 78:
				statData.SoulWeaponMaxUseCount = reader.ReadInt32();
				break;
			case 201:
				statData.ThreatGain = reader.ReadInt32();
				break;
			case 202:
				statData.ThreatGainR = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return statData;
	}
}

using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class RuneItemDataFormatter : IMessagePackFormatter<RuneItemData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, RuneItemData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
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
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ItemCode, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.GroupCode, options);
		resolver.GetFormatterWithVerify<EquipPartType[]>().Serialize(ref writer, value.PartTypes, options);
		resolver.GetFormatterWithVerify<RuneColorType>().Serialize(ref writer, value.ColorType, options);
		resolver.GetFormatterWithVerify<RuneSlotType[]>().Serialize(ref writer, value.SlotTypes, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.Tag, options);
		resolver.GetFormatterWithVerify<EquipPartTypeFlag>().Serialize(ref writer, value.PartTypeFlag, options);
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

	public RuneItemData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		RuneItemData runeItemData = new RuneItemData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				runeItemData.PhysicalAttack = reader.ReadInt32();
				break;
			case 1:
				runeItemData.MagicalAttack = reader.ReadInt32();
				break;
			case 2:
				runeItemData.MaxHp = reader.ReadInt64();
				break;
			case 3:
				runeItemData.PhysicalDefense = reader.ReadInt32();
				break;
			case 4:
				runeItemData.MagicalDefense = reader.ReadInt32();
				break;
			case 5:
				runeItemData.PhysicalCriticalChance = reader.ReadInt16();
				break;
			case 6:
				runeItemData.PhysicalCriticalPower = reader.ReadInt16();
				break;
			case 7:
				runeItemData.PhysicalDodgeChance = reader.ReadInt16();
				break;
			case 8:
				runeItemData.PhysicalBlockChance = reader.ReadInt16();
				break;
			case 9:
				runeItemData.PhysicalBlockPower = reader.ReadInt16();
				break;
			case 10:
				runeItemData.PhysicalHitChance = reader.ReadInt16();
				break;
			case 11:
				runeItemData.PhysicalPiercePower = reader.ReadInt16();
				break;
			case 12:
				runeItemData.PhysicalWeakness = reader.ReadInt16();
				break;
			case 13:
				runeItemData.PhysicalToughness = reader.ReadInt16();
				break;
			case 14:
				runeItemData.PhysicalCriticalDodgeChance = reader.ReadInt16();
				break;
			case 15:
				runeItemData.PhysicalAttackPower = reader.ReadInt16();
				break;
			case 16:
				runeItemData.PhysicalDefensePower = reader.ReadInt16();
				break;
			case 17:
				runeItemData.PhysicalDotPower = reader.ReadInt16();
				break;
			case 18:
				runeItemData.PhysicalIgnoreBlockPower = reader.ReadInt16();
				break;
			case 19:
				runeItemData.PhysicalIgnoreDefensePower = reader.ReadInt16();
				break;
			case 20:
				runeItemData.MagicalCriticalChance = reader.ReadInt16();
				break;
			case 21:
				runeItemData.MagicalCriticalPower = reader.ReadInt16();
				break;
			case 22:
				runeItemData.MagicalDodgeChance = reader.ReadInt16();
				break;
			case 23:
				runeItemData.MagicalBlockChance = reader.ReadInt16();
				break;
			case 24:
				runeItemData.MagicalBlockPower = reader.ReadInt16();
				break;
			case 25:
				runeItemData.MagicalHitChance = reader.ReadInt16();
				break;
			case 26:
				runeItemData.MagicalPiercePower = reader.ReadInt16();
				break;
			case 27:
				runeItemData.MagicalWeakness = reader.ReadInt16();
				break;
			case 28:
				runeItemData.MagicalToughness = reader.ReadInt16();
				break;
			case 29:
				runeItemData.MagicalCriticalDodgeChance = reader.ReadInt16();
				break;
			case 30:
				runeItemData.HealPower = reader.ReadInt16();
				break;
			case 31:
				runeItemData.RecoverPower = reader.ReadInt16();
				break;
			case 32:
				runeItemData.MagicalAttackPower = reader.ReadInt16();
				break;
			case 33:
				runeItemData.MagicalDefensePower = reader.ReadInt16();
				break;
			case 34:
				runeItemData.MagicalDotPower = reader.ReadInt16();
				break;
			case 35:
				runeItemData.MagicalIgnoreBlockPower = reader.ReadInt16();
				break;
			case 36:
				runeItemData.MagicalIgnoreDefensePower = reader.ReadInt16();
				break;
			case 37:
				runeItemData.AntiCcChance = reader.ReadInt16();
				break;
			case 38:
				runeItemData.AntiDebuffChance = reader.ReadInt16();
				break;
			case 39:
				runeItemData.HitCcChance = reader.ReadInt16();
				break;
			case 40:
				runeItemData.HitDebuffChance = reader.ReadInt16();
				break;
			case 41:
				runeItemData.HpStealPower = reader.ReadInt32();
				break;
			case 42:
				runeItemData.HpOnDamage = reader.ReadInt32();
				break;
			case 43:
				runeItemData.HpOnAttack = reader.ReadInt32();
				break;
			case 44:
				runeItemData.HpOnKill = reader.ReadInt32();
				break;
			case 45:
				runeItemData.HpOnWave = reader.ReadInt32();
				break;
			case 46:
				runeItemData.HpOnTime = reader.ReadInt32();
				break;
			case 47:
				runeItemData.HpRatioOnDamage = reader.ReadInt16();
				break;
			case 48:
				runeItemData.HpRatioOnAttack = reader.ReadInt16();
				break;
			case 49:
				runeItemData.HpRatioOnKill = reader.ReadInt16();
				break;
			case 50:
				runeItemData.HpRatioOnWave = reader.ReadInt16();
				break;
			case 51:
				runeItemData.HpRatioOnTime = reader.ReadInt16();
				break;
			case 52:
				runeItemData.MaxMp = reader.ReadInt32();
				break;
			case 53:
				runeItemData.MpOnDamage = reader.ReadInt16();
				break;
			case 54:
				runeItemData.MpOnAttack = reader.ReadInt16();
				break;
			case 55:
				runeItemData.MpOnKill = reader.ReadInt16();
				break;
			case 56:
				runeItemData.MpOnWave = reader.ReadInt16();
				break;
			case 57:
				runeItemData.MpOnTime = reader.ReadInt16();
				break;
			case 58:
				runeItemData.MpRatioOnDamage = reader.ReadInt16();
				break;
			case 59:
				runeItemData.MpRatioOnAttack = reader.ReadInt16();
				break;
			case 60:
				runeItemData.MpRatioOnTime = reader.ReadInt16();
				break;
			case 61:
				runeItemData.MpRegenRatioM = reader.ReadInt16();
				break;
			case 62:
				runeItemData.AttackSpeed = reader.ReadInt32();
				break;
			case 63:
				runeItemData.MoveSpeedMms = reader.ReadInt32();
				break;
			case 64:
				runeItemData.ShieldPowerR = reader.ReadInt32();
				break;
			case 65:
				runeItemData.AddMaxHpR = reader.ReadInt32();
				break;
			case 66:
				runeItemData.AddPhysicalAttackR = reader.ReadInt32();
				break;
			case 67:
				runeItemData.AddPhysicalDefenseR = reader.ReadInt32();
				break;
			case 68:
				runeItemData.AddMagicalAttackR = reader.ReadInt32();
				break;
			case 69:
				runeItemData.AddMagicalDefenseR = reader.ReadInt32();
				break;
			case 70:
				runeItemData.AddMpOnDamageR = reader.ReadInt32();
				break;
			case 71:
				runeItemData.AddMpOnAttackR = reader.ReadInt32();
				break;
			case 72:
				runeItemData.AddMpOnTimeR = reader.ReadInt32();
				break;
			case 73:
				runeItemData.SkillLevelAllSlot = reader.ReadInt32();
				break;
			case 74:
				runeItemData.SkillLevelSlot1 = reader.ReadInt32();
				break;
			case 75:
				runeItemData.SkillLevelSlot2 = reader.ReadInt32();
				break;
			case 76:
				runeItemData.SkillLevelSlot3 = reader.ReadInt32();
				break;
			case 77:
				runeItemData.SkillLevelSlot4 = reader.ReadInt32();
				break;
			case 78:
				runeItemData.SoulWeaponMaxUseCount = reader.ReadInt32();
				break;
			case 79:
				runeItemData.ItemCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 80:
				runeItemData.GroupCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 81:
				runeItemData.PartTypes = resolver.GetFormatterWithVerify<EquipPartType[]>().Deserialize(ref reader, options);
				break;
			case 82:
				runeItemData.ColorType = resolver.GetFormatterWithVerify<RuneColorType>().Deserialize(ref reader, options);
				break;
			case 83:
				runeItemData.SlotTypes = resolver.GetFormatterWithVerify<RuneSlotType[]>().Deserialize(ref reader, options);
				break;
			case 84:
				runeItemData.Tag = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 85:
				runeItemData.PartTypeFlag = resolver.GetFormatterWithVerify<EquipPartTypeFlag>().Deserialize(ref reader, options);
				break;
			case 201:
				runeItemData.ThreatGain = reader.ReadInt32();
				break;
			case 202:
				runeItemData.ThreatGainR = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return runeItemData;
	}
}

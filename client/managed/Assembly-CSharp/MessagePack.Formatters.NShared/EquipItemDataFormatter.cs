using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class EquipItemDataFormatter : IMessagePackFormatter<EquipItemData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, EquipItemData value, MessagePackSerializerOptions options)
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
		resolver.GetFormatterWithVerify<HashString>().Serialize(ref writer, value.EquipCode, options);
		resolver.GetFormatterWithVerify<EquipPartType>().Serialize(ref writer, value.PartType, options);
		resolver.GetFormatterWithVerify<CreatureTagType[]>().Serialize(ref writer, value.TagType, options);
		resolver.GetFormatterWithVerify<EquipItemSubType>().Serialize(ref writer, value.SubType, options);
		writer.Write(value.IsOpenSoulWeapon);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.CreatureIndex, options);
		writer.Write(value.Tier);
		writer.Write(value.ReqLevel);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.OptionIndex, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.OptionGroupIndex, options);
		writer.Write(value.EnableDuplicationOption);
		writer.Write(value.FixedOptionIndex1);
		writer.Write(value.FixedOptionIndex2);
		writer.Write(value.FixedOptionIndex3);
		writer.Write(value.FixedOptionIndex4);
		writer.Write(value.FixedOptionIndex5);
		writer.Write(value.SetIndex);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.BodyPrefix, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.WeaponPrefix, options);
		writer.Write(value.MinRuneCount);
		writer.Write(value.MaxRuneCount);
		writer.Write(value.Artifact);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.EnchantOptionItemCodes, options);
		writer.Write(value.DetailIndex);
		writer.Write(value.TableOrder);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.UniqueOptionIndex, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.UniqueOptionGroupIndex, options);
		writer.Write(value.EnableDuplicationUniqueOption);
		resolver.GetFormatterWithVerify<EquipItemWorkPlace>().Serialize(ref writer, value.WorkPlace, options);
		resolver.GetFormatterWithVerify<EquipItemEquipType>().Serialize(ref writer, value.EquipType, options);
		resolver.GetFormatterWithVerify<EquipItemEquipGrade>().Serialize(ref writer, value.EquipGrade, options);
		resolver.GetFormatterWithVerify<EquipItemSetGroupType>().Serialize(ref writer, value.SetGroupType, options);
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

	public EquipItemData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		EquipItemData equipItemData = new EquipItemData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				equipItemData.PhysicalAttack = reader.ReadInt32();
				break;
			case 1:
				equipItemData.MagicalAttack = reader.ReadInt32();
				break;
			case 2:
				equipItemData.MaxHp = reader.ReadInt64();
				break;
			case 3:
				equipItemData.PhysicalDefense = reader.ReadInt32();
				break;
			case 4:
				equipItemData.MagicalDefense = reader.ReadInt32();
				break;
			case 5:
				equipItemData.PhysicalCriticalChance = reader.ReadInt16();
				break;
			case 6:
				equipItemData.PhysicalCriticalPower = reader.ReadInt16();
				break;
			case 7:
				equipItemData.PhysicalDodgeChance = reader.ReadInt16();
				break;
			case 8:
				equipItemData.PhysicalBlockChance = reader.ReadInt16();
				break;
			case 9:
				equipItemData.PhysicalBlockPower = reader.ReadInt16();
				break;
			case 10:
				equipItemData.PhysicalHitChance = reader.ReadInt16();
				break;
			case 11:
				equipItemData.PhysicalPiercePower = reader.ReadInt16();
				break;
			case 12:
				equipItemData.PhysicalWeakness = reader.ReadInt16();
				break;
			case 13:
				equipItemData.PhysicalToughness = reader.ReadInt16();
				break;
			case 14:
				equipItemData.PhysicalCriticalDodgeChance = reader.ReadInt16();
				break;
			case 15:
				equipItemData.PhysicalAttackPower = reader.ReadInt16();
				break;
			case 16:
				equipItemData.PhysicalDefensePower = reader.ReadInt16();
				break;
			case 17:
				equipItemData.PhysicalDotPower = reader.ReadInt16();
				break;
			case 18:
				equipItemData.PhysicalIgnoreBlockPower = reader.ReadInt16();
				break;
			case 19:
				equipItemData.PhysicalIgnoreDefensePower = reader.ReadInt16();
				break;
			case 20:
				equipItemData.MagicalCriticalChance = reader.ReadInt16();
				break;
			case 21:
				equipItemData.MagicalCriticalPower = reader.ReadInt16();
				break;
			case 22:
				equipItemData.MagicalDodgeChance = reader.ReadInt16();
				break;
			case 23:
				equipItemData.MagicalBlockChance = reader.ReadInt16();
				break;
			case 24:
				equipItemData.MagicalBlockPower = reader.ReadInt16();
				break;
			case 25:
				equipItemData.MagicalHitChance = reader.ReadInt16();
				break;
			case 26:
				equipItemData.MagicalPiercePower = reader.ReadInt16();
				break;
			case 27:
				equipItemData.MagicalWeakness = reader.ReadInt16();
				break;
			case 28:
				equipItemData.MagicalToughness = reader.ReadInt16();
				break;
			case 29:
				equipItemData.MagicalCriticalDodgeChance = reader.ReadInt16();
				break;
			case 30:
				equipItemData.HealPower = reader.ReadInt16();
				break;
			case 31:
				equipItemData.RecoverPower = reader.ReadInt16();
				break;
			case 32:
				equipItemData.MagicalAttackPower = reader.ReadInt16();
				break;
			case 33:
				equipItemData.MagicalDefensePower = reader.ReadInt16();
				break;
			case 34:
				equipItemData.MagicalDotPower = reader.ReadInt16();
				break;
			case 35:
				equipItemData.MagicalIgnoreBlockPower = reader.ReadInt16();
				break;
			case 36:
				equipItemData.MagicalIgnoreDefensePower = reader.ReadInt16();
				break;
			case 37:
				equipItemData.AntiCcChance = reader.ReadInt16();
				break;
			case 38:
				equipItemData.AntiDebuffChance = reader.ReadInt16();
				break;
			case 39:
				equipItemData.HitCcChance = reader.ReadInt16();
				break;
			case 40:
				equipItemData.HitDebuffChance = reader.ReadInt16();
				break;
			case 41:
				equipItemData.HpStealPower = reader.ReadInt32();
				break;
			case 42:
				equipItemData.HpOnDamage = reader.ReadInt32();
				break;
			case 43:
				equipItemData.HpOnAttack = reader.ReadInt32();
				break;
			case 44:
				equipItemData.HpOnKill = reader.ReadInt32();
				break;
			case 45:
				equipItemData.HpOnWave = reader.ReadInt32();
				break;
			case 46:
				equipItemData.HpOnTime = reader.ReadInt32();
				break;
			case 47:
				equipItemData.HpRatioOnDamage = reader.ReadInt16();
				break;
			case 48:
				equipItemData.HpRatioOnAttack = reader.ReadInt16();
				break;
			case 49:
				equipItemData.HpRatioOnKill = reader.ReadInt16();
				break;
			case 50:
				equipItemData.HpRatioOnWave = reader.ReadInt16();
				break;
			case 51:
				equipItemData.HpRatioOnTime = reader.ReadInt16();
				break;
			case 52:
				equipItemData.MaxMp = reader.ReadInt32();
				break;
			case 53:
				equipItemData.MpOnDamage = reader.ReadInt16();
				break;
			case 54:
				equipItemData.MpOnAttack = reader.ReadInt16();
				break;
			case 55:
				equipItemData.MpOnKill = reader.ReadInt16();
				break;
			case 56:
				equipItemData.MpOnWave = reader.ReadInt16();
				break;
			case 57:
				equipItemData.MpOnTime = reader.ReadInt16();
				break;
			case 58:
				equipItemData.MpRatioOnDamage = reader.ReadInt16();
				break;
			case 59:
				equipItemData.MpRatioOnAttack = reader.ReadInt16();
				break;
			case 60:
				equipItemData.MpRatioOnTime = reader.ReadInt16();
				break;
			case 61:
				equipItemData.MpRegenRatioM = reader.ReadInt16();
				break;
			case 62:
				equipItemData.AttackSpeed = reader.ReadInt32();
				break;
			case 63:
				equipItemData.MoveSpeedMms = reader.ReadInt32();
				break;
			case 64:
				equipItemData.ShieldPowerR = reader.ReadInt32();
				break;
			case 65:
				equipItemData.AddMaxHpR = reader.ReadInt32();
				break;
			case 66:
				equipItemData.AddPhysicalAttackR = reader.ReadInt32();
				break;
			case 67:
				equipItemData.AddPhysicalDefenseR = reader.ReadInt32();
				break;
			case 68:
				equipItemData.AddMagicalAttackR = reader.ReadInt32();
				break;
			case 69:
				equipItemData.AddMagicalDefenseR = reader.ReadInt32();
				break;
			case 70:
				equipItemData.AddMpOnDamageR = reader.ReadInt32();
				break;
			case 71:
				equipItemData.AddMpOnAttackR = reader.ReadInt32();
				break;
			case 72:
				equipItemData.AddMpOnTimeR = reader.ReadInt32();
				break;
			case 73:
				equipItemData.SkillLevelAllSlot = reader.ReadInt32();
				break;
			case 74:
				equipItemData.SkillLevelSlot1 = reader.ReadInt32();
				break;
			case 75:
				equipItemData.SkillLevelSlot2 = reader.ReadInt32();
				break;
			case 76:
				equipItemData.SkillLevelSlot3 = reader.ReadInt32();
				break;
			case 77:
				equipItemData.SkillLevelSlot4 = reader.ReadInt32();
				break;
			case 78:
				equipItemData.SoulWeaponMaxUseCount = reader.ReadInt32();
				break;
			case 79:
				equipItemData.EquipCode = resolver.GetFormatterWithVerify<HashString>().Deserialize(ref reader, options);
				break;
			case 80:
				equipItemData.PartType = resolver.GetFormatterWithVerify<EquipPartType>().Deserialize(ref reader, options);
				break;
			case 81:
				equipItemData.TagType = resolver.GetFormatterWithVerify<CreatureTagType[]>().Deserialize(ref reader, options);
				break;
			case 82:
				equipItemData.SubType = resolver.GetFormatterWithVerify<EquipItemSubType>().Deserialize(ref reader, options);
				break;
			case 83:
				equipItemData.IsOpenSoulWeapon = reader.ReadBoolean();
				break;
			case 84:
				equipItemData.CreatureIndex = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 85:
				equipItemData.Tier = reader.ReadInt16();
				break;
			case 86:
				equipItemData.ReqLevel = reader.ReadInt16();
				break;
			case 87:
				equipItemData.OptionIndex = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 88:
				equipItemData.OptionGroupIndex = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 89:
				equipItemData.EnableDuplicationOption = reader.ReadBoolean();
				break;
			case 90:
				equipItemData.FixedOptionIndex1 = reader.ReadInt32();
				break;
			case 91:
				equipItemData.FixedOptionIndex2 = reader.ReadInt32();
				break;
			case 92:
				equipItemData.FixedOptionIndex3 = reader.ReadInt32();
				break;
			case 93:
				equipItemData.FixedOptionIndex4 = reader.ReadInt32();
				break;
			case 94:
				equipItemData.FixedOptionIndex5 = reader.ReadInt32();
				break;
			case 95:
				equipItemData.SetIndex = reader.ReadInt32();
				break;
			case 96:
				equipItemData.BodyPrefix = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 97:
				equipItemData.WeaponPrefix = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 98:
				equipItemData.MinRuneCount = reader.ReadInt32();
				break;
			case 99:
				equipItemData.MaxRuneCount = reader.ReadInt32();
				break;
			case 100:
				equipItemData.Artifact = reader.ReadInt16();
				break;
			case 101:
				equipItemData.EnchantOptionItemCodes = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 102:
				equipItemData.DetailIndex = reader.ReadInt32();
				break;
			case 103:
				equipItemData.TableOrder = reader.ReadInt32();
				break;
			case 104:
				equipItemData.UniqueOptionIndex = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 105:
				equipItemData.UniqueOptionGroupIndex = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 106:
				equipItemData.EnableDuplicationUniqueOption = reader.ReadBoolean();
				break;
			case 107:
				equipItemData.WorkPlace = resolver.GetFormatterWithVerify<EquipItemWorkPlace>().Deserialize(ref reader, options);
				break;
			case 108:
				equipItemData.EquipType = resolver.GetFormatterWithVerify<EquipItemEquipType>().Deserialize(ref reader, options);
				break;
			case 109:
				equipItemData.EquipGrade = resolver.GetFormatterWithVerify<EquipItemEquipGrade>().Deserialize(ref reader, options);
				break;
			case 110:
				equipItemData.SetGroupType = resolver.GetFormatterWithVerify<EquipItemSetGroupType>().Deserialize(ref reader, options);
				break;
			case 201:
				equipItemData.ThreatGain = reader.ReadInt32();
				break;
			case 202:
				equipItemData.ThreatGainR = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return equipItemData;
	}
}

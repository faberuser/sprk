using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class CreatureDataFormatter : IMessagePackFormatter<CreatureData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, CreatureData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(101);
		writer.Write(value.Index);
		writer.WriteNil();
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ReplaceNameKey, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.NameParam, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.CodeName, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ModelCode, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ModelPrefix, options);
		resolver.GetFormatterWithVerify<CreatureOpenType>().Serialize(ref writer, value.OpenType, options);
		resolver.GetFormatterWithVerify<CreatureOpenStatusType>().Serialize(ref writer, value.OpenStatus, options);
		writer.Write(value.OpenDate);
		resolver.GetFormatterWithVerify<CreatureRaceType>().Serialize(ref writer, value.Race, options);
		resolver.GetFormatterWithVerify<CreatureTagType>().Serialize(ref writer, value.TagType, options);
		resolver.GetFormatterWithVerify<SkillAttrType[]>().Serialize(ref writer, value.AttrType, options);
		resolver.GetFormatterWithVerify<SkillAttrType[]>().Serialize(ref writer, value.WeakType, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ArchiveStory, options);
		resolver.GetFormatterWithVerify<ArchiveType>().Serialize(ref writer, value.ArchiveType, options);
		resolver.GetFormatterWithVerify<AttackAttrType>().Serialize(ref writer, value.AttackType, options);
		resolver.GetFormatterWithVerify<AttackAttrType[]>().Serialize(ref writer, value.AttackWeakType, options);
		resolver.GetFormatterWithVerify<CreaturePositionType>().Serialize(ref writer, value.PositionType, options);
		writer.Write(value.PositionWeight);
		writer.Write(value.SizeMm);
		writer.Write(value.BlockDash);
		writer.Write(value.HeightMm);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.IgnoreCc, options);
		resolver.GetFormatterWithVerify<CreatureTransformType>().Serialize(ref writer, value.TransformType, options);
		resolver.GetFormatterWithVerify<SkillTargetType[]>().Serialize(ref writer, value.AvoidSkillTargetType, options);
		writer.Write(value.OffsetMx);
		writer.Write(value.OffsetMz);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.GradeStoneItemCode, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.StarStoneItemCode, options);
		writer.Write(value.DropItemWeight);
		writer.Write(value.DropGoldWeight);
		writer.Write(value.BaseSkillIndex);
		writer.Write(value.SkillIndex1);
		writer.Write(value.SkillIndex2);
		writer.Write(value.SkillIndex3);
		writer.Write(value.SkillIndex4);
		writer.Write(value.SkillIndex5);
		writer.Write(value.LeaderSkillIndex);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.SkillExtend1, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.SkillExtend2, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.SkillExtend3, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.SkillExtend4, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.SkillExtend5, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.LinkedSkillIndex1, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.LinkedSkillIndex2, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.LinkedSkillIndex3, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.LinkedSkillIndex4, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.LinkedSkillIndex5, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.ExtraSkill, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.ExtraActiveSkill, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.ShowInfoSkill, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.GetTranscendSkillPoint, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.TranscendSkill1, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.ReqSkillPoint1, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.TranscendSkill2, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.ReqSkillPoint2, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.TranscendSkill3, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.ReqSkillPoint3, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.TranscendSkill4, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.ReqSkillPoint4, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.TranscendSkill5, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.ReqSkillPoint5, options);
		writer.Write(value.StartHeroStar);
		writer.Write(value.StartHeroLevel);
		writer.WriteNil();
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ReplaceDescKey, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.DescParam, options);
		writer.WriteNil();
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ReplaceTitleKey, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.TitleParam, options);
		writer.WriteNil();
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ClearTip, options);
		writer.Write(value.AddStamina);
		writer.Write(value.IsDestroyOnDied);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.HeroPositionIcon1, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.HeroPositionText1, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.HeroPositionIcon2, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.HeroPositionText2, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.HeroPositionIcon3, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.HeroPositionText3, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.TranscendRecommend1, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.TranscendRecommend2, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.TranscendRecommend3, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.EquipoptionRecommend1, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.EquipoptionRecommend2, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.EquipoptionRecommend3, options);
		writer.Write(value.SoulWeaponBasicSkillIndex);
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.WriteNil();
		writer.Write(value.Grade);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.PetOptionIndices, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.PetBonusIndices, options);
		writer.Write(value.PlayableCharacter);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.HeroPositionIcon4, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.HeroPositionText4, options);
		writer.Write(value.MaxFood);
		writer.Write(value.IsLinePet);
	}

	public CreatureData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		CreatureData creatureData = new CreatureData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				creatureData.Index = reader.ReadInt32();
				break;
			case 2:
				creatureData.ReplaceNameKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 3:
				creatureData.NameParam = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 4:
				creatureData.CodeName = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 5:
				creatureData.ModelCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 6:
				creatureData.ModelPrefix = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 7:
				creatureData.OpenType = resolver.GetFormatterWithVerify<CreatureOpenType>().Deserialize(ref reader, options);
				break;
			case 8:
				creatureData.OpenStatus = resolver.GetFormatterWithVerify<CreatureOpenStatusType>().Deserialize(ref reader, options);
				break;
			case 9:
				creatureData.OpenDate = reader.ReadInt32();
				break;
			case 10:
				creatureData.Race = resolver.GetFormatterWithVerify<CreatureRaceType>().Deserialize(ref reader, options);
				break;
			case 11:
				creatureData.TagType = resolver.GetFormatterWithVerify<CreatureTagType>().Deserialize(ref reader, options);
				break;
			case 12:
				creatureData.AttrType = resolver.GetFormatterWithVerify<SkillAttrType[]>().Deserialize(ref reader, options);
				break;
			case 13:
				creatureData.WeakType = resolver.GetFormatterWithVerify<SkillAttrType[]>().Deserialize(ref reader, options);
				break;
			case 14:
				creatureData.ArchiveStory = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 15:
				creatureData.ArchiveType = resolver.GetFormatterWithVerify<ArchiveType>().Deserialize(ref reader, options);
				break;
			case 16:
				creatureData.AttackType = resolver.GetFormatterWithVerify<AttackAttrType>().Deserialize(ref reader, options);
				break;
			case 17:
				creatureData.AttackWeakType = resolver.GetFormatterWithVerify<AttackAttrType[]>().Deserialize(ref reader, options);
				break;
			case 18:
				creatureData.PositionType = resolver.GetFormatterWithVerify<CreaturePositionType>().Deserialize(ref reader, options);
				break;
			case 19:
				creatureData.PositionWeight = reader.ReadInt32();
				break;
			case 20:
				creatureData.SizeMm = reader.ReadInt32();
				break;
			case 21:
				creatureData.BlockDash = reader.ReadBoolean();
				break;
			case 22:
				creatureData.HeightMm = reader.ReadInt32();
				break;
			case 23:
				creatureData.IgnoreCc = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 24:
				creatureData.TransformType = resolver.GetFormatterWithVerify<CreatureTransformType>().Deserialize(ref reader, options);
				break;
			case 25:
				creatureData.AvoidSkillTargetType = resolver.GetFormatterWithVerify<SkillTargetType[]>().Deserialize(ref reader, options);
				break;
			case 26:
				creatureData.OffsetMx = reader.ReadInt32();
				break;
			case 27:
				creatureData.OffsetMz = reader.ReadInt32();
				break;
			case 28:
				creatureData.GradeStoneItemCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 29:
				creatureData.StarStoneItemCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 30:
				creatureData.DropItemWeight = reader.ReadInt32();
				break;
			case 31:
				creatureData.DropGoldWeight = reader.ReadInt32();
				break;
			case 32:
				creatureData.BaseSkillIndex = reader.ReadInt32();
				break;
			case 33:
				creatureData.SkillIndex1 = reader.ReadInt32();
				break;
			case 34:
				creatureData.SkillIndex2 = reader.ReadInt32();
				break;
			case 35:
				creatureData.SkillIndex3 = reader.ReadInt32();
				break;
			case 36:
				creatureData.SkillIndex4 = reader.ReadInt32();
				break;
			case 37:
				creatureData.SkillIndex5 = reader.ReadInt32();
				break;
			case 38:
				creatureData.LeaderSkillIndex = reader.ReadInt32();
				break;
			case 39:
				creatureData.SkillExtend1 = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 40:
				creatureData.SkillExtend2 = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 41:
				creatureData.SkillExtend3 = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 42:
				creatureData.SkillExtend4 = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 43:
				creatureData.SkillExtend5 = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 44:
				creatureData.LinkedSkillIndex1 = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 45:
				creatureData.LinkedSkillIndex2 = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 46:
				creatureData.LinkedSkillIndex3 = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 47:
				creatureData.LinkedSkillIndex4 = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 48:
				creatureData.LinkedSkillIndex5 = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 49:
				creatureData.ExtraSkill = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 50:
				creatureData.ExtraActiveSkill = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 51:
				creatureData.ShowInfoSkill = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 52:
				creatureData.GetTranscendSkillPoint = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 53:
				creatureData.TranscendSkill1 = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 54:
				creatureData.ReqSkillPoint1 = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 55:
				creatureData.TranscendSkill2 = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 56:
				creatureData.ReqSkillPoint2 = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 57:
				creatureData.TranscendSkill3 = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 58:
				creatureData.ReqSkillPoint3 = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 59:
				creatureData.TranscendSkill4 = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 60:
				creatureData.ReqSkillPoint4 = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 61:
				creatureData.TranscendSkill5 = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 62:
				creatureData.ReqSkillPoint5 = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 63:
				creatureData.StartHeroStar = reader.ReadSByte();
				break;
			case 64:
				creatureData.StartHeroLevel = reader.ReadInt32();
				break;
			case 66:
				creatureData.ReplaceDescKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 67:
				creatureData.DescParam = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 69:
				creatureData.ReplaceTitleKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 70:
				creatureData.TitleParam = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 72:
				creatureData.ClearTip = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 73:
				creatureData.AddStamina = reader.ReadInt32();
				break;
			case 74:
				creatureData.IsDestroyOnDied = reader.ReadBoolean();
				break;
			case 75:
				creatureData.HeroPositionIcon1 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 76:
				creatureData.HeroPositionText1 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 77:
				creatureData.HeroPositionIcon2 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 78:
				creatureData.HeroPositionText2 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 79:
				creatureData.HeroPositionIcon3 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 80:
				creatureData.HeroPositionText3 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 81:
				creatureData.TranscendRecommend1 = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 82:
				creatureData.TranscendRecommend2 = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 83:
				creatureData.TranscendRecommend3 = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 84:
				creatureData.EquipoptionRecommend1 = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 85:
				creatureData.EquipoptionRecommend2 = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 86:
				creatureData.EquipoptionRecommend3 = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 87:
				creatureData.SoulWeaponBasicSkillIndex = reader.ReadInt32();
				break;
			case 93:
				creatureData.Grade = reader.ReadByte();
				break;
			case 94:
				creatureData.PetOptionIndices = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 95:
				creatureData.PetBonusIndices = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 96:
				creatureData.PlayableCharacter = reader.ReadBoolean();
				break;
			case 97:
				creatureData.HeroPositionIcon4 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 98:
				creatureData.HeroPositionText4 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 99:
				creatureData.MaxFood = reader.ReadInt32();
				break;
			case 100:
				creatureData.IsLinePet = reader.ReadBoolean();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return creatureData;
	}
}

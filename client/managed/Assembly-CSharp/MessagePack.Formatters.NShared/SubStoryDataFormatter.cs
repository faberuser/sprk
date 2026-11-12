using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class SubStoryDataFormatter : IMessagePackFormatter<SubStoryData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, SubStoryData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(46);
		writer.Write(value.ChapterIndex);
		writer.Write(value.DungeonIndex);
		writer.Write(value.NoShowNodeInfo);
		writer.Write(value.CostumeIndex);
		writer.Write(value.CreatureIndex);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.SubStoryTactic, options);
		writer.Write(value.ReqCreatureIndex);
		writer.Write(value.ReqStar);
		writer.Write(value.ReqTranscended);
		writer.Write(value.ReqLevel);
		writer.Write(value.ReqChapterIndex);
		writer.Write(value.ReqDungeonIndex);
		writer.Write(value.FirstRewardIndex);
		writer.Write(value.UnlockCostumeIndex);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.IllustPath, options);
		writer.Write(value.New);
		writer.Write(value.SubStoryEventIndex);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.ForcedHeroIndex, options);
		writer.Write(value.HeroStar1);
		writer.Write(value.HeroLevel1);
		writer.Write(value.HeroTier1);
		writer.Write(value.HeroScaleR1);
		writer.Write(value.HeroCostume1);
		writer.Write(value.HeroWeaponIndex1);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.HeroAddExtraSkill1, options);
		writer.Write(value.HeroStar2);
		writer.Write(value.HeroLevel2);
		writer.Write(value.HeroTier2);
		writer.Write(value.HeroScaleR2);
		writer.Write(value.HeroCostume2);
		writer.Write(value.HeroWeaponIndex2);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.HeroAddExtraSkill2, options);
		writer.Write(value.HeroStar3);
		writer.Write(value.HeroLevel3);
		writer.Write(value.HeroTier3);
		writer.Write(value.HeroScaleR3);
		writer.Write(value.HeroCostume3);
		writer.Write(value.HeroWeaponIndex3);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.HeroAddExtraSkill3, options);
		writer.Write(value.HeroStar4);
		writer.Write(value.HeroLevel4);
		writer.Write(value.HeroTier4);
		writer.Write(value.HeroScaleR4);
		writer.Write(value.HeroCostume4);
		writer.Write(value.HeroWeaponIndex4);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.HeroAddExtraSkill4, options);
	}

	public SubStoryData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		SubStoryData subStoryData = new SubStoryData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				subStoryData.ChapterIndex = reader.ReadInt32();
				break;
			case 1:
				subStoryData.DungeonIndex = reader.ReadInt32();
				break;
			case 2:
				subStoryData.NoShowNodeInfo = reader.ReadBoolean();
				break;
			case 3:
				subStoryData.CostumeIndex = reader.ReadInt32();
				break;
			case 4:
				subStoryData.CreatureIndex = reader.ReadInt32();
				break;
			case 5:
				subStoryData.SubStoryTactic = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 6:
				subStoryData.ReqCreatureIndex = reader.ReadInt32();
				break;
			case 7:
				subStoryData.ReqStar = reader.ReadInt32();
				break;
			case 8:
				subStoryData.ReqTranscended = reader.ReadInt32();
				break;
			case 9:
				subStoryData.ReqLevel = reader.ReadInt32();
				break;
			case 10:
				subStoryData.ReqChapterIndex = reader.ReadInt32();
				break;
			case 11:
				subStoryData.ReqDungeonIndex = reader.ReadInt32();
				break;
			case 12:
				subStoryData.FirstRewardIndex = reader.ReadInt32();
				break;
			case 13:
				subStoryData.UnlockCostumeIndex = reader.ReadInt32();
				break;
			case 14:
				subStoryData.IllustPath = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 15:
				subStoryData.New = reader.ReadBoolean();
				break;
			case 16:
				subStoryData.SubStoryEventIndex = reader.ReadInt32();
				break;
			case 17:
				subStoryData.ForcedHeroIndex = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 18:
				subStoryData.HeroStar1 = reader.ReadInt32();
				break;
			case 19:
				subStoryData.HeroLevel1 = reader.ReadInt32();
				break;
			case 20:
				subStoryData.HeroTier1 = reader.ReadInt32();
				break;
			case 21:
				subStoryData.HeroScaleR1 = reader.ReadInt32();
				break;
			case 22:
				subStoryData.HeroCostume1 = reader.ReadInt32();
				break;
			case 23:
				subStoryData.HeroWeaponIndex1 = reader.ReadInt32();
				break;
			case 24:
				subStoryData.HeroAddExtraSkill1 = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 25:
				subStoryData.HeroStar2 = reader.ReadInt32();
				break;
			case 26:
				subStoryData.HeroLevel2 = reader.ReadInt32();
				break;
			case 27:
				subStoryData.HeroTier2 = reader.ReadInt32();
				break;
			case 28:
				subStoryData.HeroScaleR2 = reader.ReadInt32();
				break;
			case 29:
				subStoryData.HeroCostume2 = reader.ReadInt32();
				break;
			case 30:
				subStoryData.HeroWeaponIndex2 = reader.ReadInt32();
				break;
			case 31:
				subStoryData.HeroAddExtraSkill2 = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 32:
				subStoryData.HeroStar3 = reader.ReadInt32();
				break;
			case 33:
				subStoryData.HeroLevel3 = reader.ReadInt32();
				break;
			case 34:
				subStoryData.HeroTier3 = reader.ReadInt32();
				break;
			case 35:
				subStoryData.HeroScaleR3 = reader.ReadInt32();
				break;
			case 36:
				subStoryData.HeroCostume3 = reader.ReadInt32();
				break;
			case 37:
				subStoryData.HeroWeaponIndex3 = reader.ReadInt32();
				break;
			case 38:
				subStoryData.HeroAddExtraSkill3 = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 39:
				subStoryData.HeroStar4 = reader.ReadInt32();
				break;
			case 40:
				subStoryData.HeroLevel4 = reader.ReadInt32();
				break;
			case 41:
				subStoryData.HeroTier4 = reader.ReadInt32();
				break;
			case 42:
				subStoryData.HeroScaleR4 = reader.ReadInt32();
				break;
			case 43:
				subStoryData.HeroCostume4 = reader.ReadInt32();
				break;
			case 44:
				subStoryData.HeroWeaponIndex4 = reader.ReadInt32();
				break;
			case 45:
				subStoryData.HeroAddExtraSkill4 = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return subStoryData;
	}
}

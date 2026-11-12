using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class DungeonHeroSetDataFormatter : IMessagePackFormatter<DungeonHeroSetData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, DungeonHeroSetData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(11);
		writer.Write(value.ChapterIndex);
		writer.Write(value.DungeonIndex);
		resolver.GetFormatterWithVerify<DungeonDifficultyType>().Serialize(ref writer, value.Difficulty, options);
		writer.Write(value.HeroListSetDataIndex);
		writer.Write(value.CommonUniqueWeapon);
		writer.Write(value.CommonHeroStar);
		writer.Write(value.CommonHeroLevel);
		writer.Write(value.CommonHeroTranscended);
		writer.Write(value.CommonHeroTier);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.CommonHeroAddExtraSkill, options);
		writer.Write(value.IsScenario);
	}

	public DungeonHeroSetData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		DungeonHeroSetData dungeonHeroSetData = new DungeonHeroSetData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				dungeonHeroSetData.ChapterIndex = reader.ReadInt32();
				break;
			case 1:
				dungeonHeroSetData.DungeonIndex = reader.ReadInt32();
				break;
			case 2:
				dungeonHeroSetData.Difficulty = resolver.GetFormatterWithVerify<DungeonDifficultyType>().Deserialize(ref reader, options);
				break;
			case 3:
				dungeonHeroSetData.HeroListSetDataIndex = reader.ReadInt32();
				break;
			case 4:
				dungeonHeroSetData.CommonUniqueWeapon = reader.ReadBoolean();
				break;
			case 5:
				dungeonHeroSetData.CommonHeroStar = reader.ReadInt32();
				break;
			case 6:
				dungeonHeroSetData.CommonHeroLevel = reader.ReadInt32();
				break;
			case 7:
				dungeonHeroSetData.CommonHeroTranscended = reader.ReadInt32();
				break;
			case 8:
				dungeonHeroSetData.CommonHeroTier = reader.ReadInt32();
				break;
			case 9:
				dungeonHeroSetData.CommonHeroAddExtraSkill = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 10:
				dungeonHeroSetData.IsScenario = reader.ReadBoolean();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return dungeonHeroSetData;
	}
}

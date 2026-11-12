using System;
using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class CampaignTacticKeyFormatter : IMessagePackFormatter<CampaignTacticKey>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, CampaignTacticKey value, MessagePackSerializerOptions options)
	{
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(3);
		writer.Write(value.ChapterIndex);
		writer.Write(value.DungeonIndex);
		resolver.GetFormatterWithVerify<DungeonDifficultyType>().Serialize(ref writer, value.Difficulty, options);
	}

	public CampaignTacticKey Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			throw new InvalidOperationException("typecode is null, struct not supported");
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		int chapterIndex = 0;
		int dungeonIndex = 0;
		DungeonDifficultyType difficulty = DungeonDifficultyType.Easy;
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				chapterIndex = reader.ReadInt32();
				break;
			case 1:
				dungeonIndex = reader.ReadInt32();
				break;
			case 2:
				difficulty = resolver.GetFormatterWithVerify<DungeonDifficultyType>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		CampaignTacticKey result = new CampaignTacticKey(chapterIndex, dungeonIndex, difficulty);
		reader.Depth--;
		return result;
	}
}

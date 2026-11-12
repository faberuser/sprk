using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class CampaignWaveDataContainer_ChapterDungeonInfoFormatter : IMessagePackFormatter<CampaignWaveDataContainer.ChapterDungeonInfo>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, CampaignWaveDataContainer.ChapterDungeonInfo value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(2);
		resolver.GetFormatterWithVerify<DungeonDifficultyType>().Serialize(ref writer, value.MinDifficulty, options);
		resolver.GetFormatterWithVerify<DungeonDifficultyType>().Serialize(ref writer, value.MaxDifficulty, options);
	}

	public CampaignWaveDataContainer.ChapterDungeonInfo Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		CampaignWaveDataContainer.ChapterDungeonInfo chapterDungeonInfo = new CampaignWaveDataContainer.ChapterDungeonInfo();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				chapterDungeonInfo.MinDifficulty = resolver.GetFormatterWithVerify<DungeonDifficultyType>().Deserialize(ref reader, options);
				break;
			case 1:
				chapterDungeonInfo.MaxDifficulty = resolver.GetFormatterWithVerify<DungeonDifficultyType>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return chapterDungeonInfo;
	}
}

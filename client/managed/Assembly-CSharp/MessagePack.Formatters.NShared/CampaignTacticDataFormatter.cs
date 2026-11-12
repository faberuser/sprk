using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class CampaignTacticDataFormatter : IMessagePackFormatter<CampaignTacticData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, CampaignTacticData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(8);
		writer.Write(value.ChapterIndex);
		writer.Write(value.DungeonIndex);
		resolver.GetFormatterWithVerify<DungeonDifficultyType>().Serialize(ref writer, value.Difficulty, options);
		writer.Write(value.ScenarioOnly);
		resolver.GetFormatterWithVerify<BattleTacticType>().Serialize(ref writer, value.TacticType, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ComponentName, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.TacticValue, options);
		writer.Write(value.BossTactic);
	}

	public CampaignTacticData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		CampaignTacticData campaignTacticData = new CampaignTacticData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				campaignTacticData.ChapterIndex = reader.ReadInt32();
				break;
			case 1:
				campaignTacticData.DungeonIndex = reader.ReadInt32();
				break;
			case 2:
				campaignTacticData.Difficulty = resolver.GetFormatterWithVerify<DungeonDifficultyType>().Deserialize(ref reader, options);
				break;
			case 3:
				campaignTacticData.ScenarioOnly = reader.ReadBoolean();
				break;
			case 4:
				campaignTacticData.TacticType = resolver.GetFormatterWithVerify<BattleTacticType>().Deserialize(ref reader, options);
				break;
			case 5:
				campaignTacticData.ComponentName = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 6:
				campaignTacticData.TacticValue = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 7:
				campaignTacticData.BossTactic = reader.ReadBoolean();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return campaignTacticData;
	}
}

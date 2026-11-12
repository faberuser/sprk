using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class GodkingTrialDataFormatter : IMessagePackFormatter<GodkingTrialData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, GodkingTrialData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		writer.WriteArrayHeader(4);
		writer.Write(value.ChapterIndex);
		writer.Write(value.DungeonIndex);
		writer.Write(value.DifficultyNameKey);
		writer.Write(value.RewardIndex);
	}

	public GodkingTrialData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		GodkingTrialData godkingTrialData = new GodkingTrialData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				godkingTrialData.ChapterIndex = reader.ReadInt32();
				break;
			case 1:
				godkingTrialData.DungeonIndex = reader.ReadInt32();
				break;
			case 2:
				godkingTrialData.DifficultyNameKey = reader.ReadInt32();
				break;
			case 3:
				godkingTrialData.RewardIndex = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return godkingTrialData;
	}
}

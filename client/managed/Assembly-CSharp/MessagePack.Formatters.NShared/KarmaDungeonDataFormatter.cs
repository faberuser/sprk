using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class KarmaDungeonDataFormatter : IMessagePackFormatter<KarmaDungeonData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, KarmaDungeonData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		writer.WriteArrayHeader(6);
		writer.Write(value.PunishmentGroupIndex);
		writer.Write(value.StageLevel);
		writer.Write(value.NextLevel);
		writer.Write(value.ChapterIndex);
		writer.Write(value.DungeonIndex);
		writer.Write(value.TotalWaveCount);
	}

	public KarmaDungeonData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		KarmaDungeonData karmaDungeonData = new KarmaDungeonData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				karmaDungeonData.PunishmentGroupIndex = reader.ReadInt32();
				break;
			case 1:
				karmaDungeonData.StageLevel = reader.ReadInt32();
				break;
			case 2:
				karmaDungeonData.NextLevel = reader.ReadInt32();
				break;
			case 3:
				karmaDungeonData.ChapterIndex = reader.ReadInt32();
				break;
			case 4:
				karmaDungeonData.DungeonIndex = reader.ReadInt32();
				break;
			case 5:
				karmaDungeonData.TotalWaveCount = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return karmaDungeonData;
	}
}

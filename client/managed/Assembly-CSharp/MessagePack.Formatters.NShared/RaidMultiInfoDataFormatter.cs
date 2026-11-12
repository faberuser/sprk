using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class RaidMultiInfoDataFormatter : IMessagePackFormatter<RaidMultiInfoData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, RaidMultiInfoData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(8);
		writer.Write(value.RaidIndex);
		writer.Write(value.Index);
		writer.Write(value.ChapterIndex);
		writer.Write(value.DungeonIndex);
		writer.Write(value.Level);
		writer.Write(value.WaveIndex);
		writer.Write(value.GroupIndex);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Tactic, options);
	}

	public RaidMultiInfoData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		RaidMultiInfoData raidMultiInfoData = new RaidMultiInfoData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				raidMultiInfoData.RaidIndex = reader.ReadInt32();
				break;
			case 1:
				raidMultiInfoData.Index = reader.ReadInt32();
				break;
			case 2:
				raidMultiInfoData.ChapterIndex = reader.ReadInt32();
				break;
			case 3:
				raidMultiInfoData.DungeonIndex = reader.ReadInt32();
				break;
			case 4:
				raidMultiInfoData.Level = reader.ReadInt32();
				break;
			case 5:
				raidMultiInfoData.WaveIndex = reader.ReadInt32();
				break;
			case 6:
				raidMultiInfoData.GroupIndex = reader.ReadInt32();
				break;
			case 7:
				raidMultiInfoData.Tactic = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return raidMultiInfoData;
	}
}

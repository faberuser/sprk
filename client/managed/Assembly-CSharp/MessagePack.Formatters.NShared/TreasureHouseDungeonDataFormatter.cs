using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class TreasureHouseDungeonDataFormatter : IMessagePackFormatter<TreasureHouseDungeonData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, TreasureHouseDungeonData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(14);
		writer.Write(value.Index);
		writer.Write(value.InfoIndex);
		writer.Write(value.ShowIndex);
		writer.Write(value.ChapterIndex);
		writer.Write(value.DungeonIndex);
		writer.Write(value.CreatureIndex);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.IconPortraitPath, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.SelectIconPortraitPath, options);
		writer.Write(value.RewardIndex);
		writer.Write(value.Ratio);
		writer.Write(value.SequenceIndex);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.CreaturePosition, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.CreatureRotation, options);
		writer.Write(value.CreatureScale);
	}

	public TreasureHouseDungeonData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		TreasureHouseDungeonData treasureHouseDungeonData = new TreasureHouseDungeonData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				treasureHouseDungeonData.Index = reader.ReadInt32();
				break;
			case 1:
				treasureHouseDungeonData.InfoIndex = reader.ReadInt32();
				break;
			case 2:
				treasureHouseDungeonData.ShowIndex = reader.ReadInt32();
				break;
			case 3:
				treasureHouseDungeonData.ChapterIndex = reader.ReadInt32();
				break;
			case 4:
				treasureHouseDungeonData.DungeonIndex = reader.ReadInt32();
				break;
			case 5:
				treasureHouseDungeonData.CreatureIndex = reader.ReadInt32();
				break;
			case 6:
				treasureHouseDungeonData.IconPortraitPath = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 7:
				treasureHouseDungeonData.SelectIconPortraitPath = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 8:
				treasureHouseDungeonData.RewardIndex = reader.ReadInt32();
				break;
			case 9:
				treasureHouseDungeonData.Ratio = reader.ReadInt32();
				break;
			case 10:
				treasureHouseDungeonData.SequenceIndex = reader.ReadInt32();
				break;
			case 11:
				treasureHouseDungeonData.CreaturePosition = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 12:
				treasureHouseDungeonData.CreatureRotation = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 13:
				treasureHouseDungeonData.CreatureScale = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return treasureHouseDungeonData;
	}
}

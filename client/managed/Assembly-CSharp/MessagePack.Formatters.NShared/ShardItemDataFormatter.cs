using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class ShardItemDataFormatter : IMessagePackFormatter<ShardItemData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, ShardItemData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(8);
		writer.Write(value.Index);
		writer.Write(value.GroupIndex);
		writer.Write(value.ShowIndex);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.AvailableRaidIndices, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.AffixIndexes, options);
		writer.Write(value.Duration);
		writer.Write(value.IsWithDraw);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.BattleIconPath, options);
	}

	public ShardItemData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		ShardItemData shardItemData = new ShardItemData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				shardItemData.Index = reader.ReadInt32();
				break;
			case 1:
				shardItemData.GroupIndex = reader.ReadInt32();
				break;
			case 2:
				shardItemData.ShowIndex = reader.ReadInt32();
				break;
			case 3:
				shardItemData.AvailableRaidIndices = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 4:
				shardItemData.AffixIndexes = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 5:
				shardItemData.Duration = reader.ReadInt32();
				break;
			case 6:
				shardItemData.IsWithDraw = reader.ReadInt32();
				break;
			case 7:
				shardItemData.BattleIconPath = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return shardItemData;
	}
}

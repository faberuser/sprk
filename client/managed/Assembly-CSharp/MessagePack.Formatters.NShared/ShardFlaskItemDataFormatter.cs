using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class ShardFlaskItemDataFormatter : IMessagePackFormatter<ShardFlaskItemData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, ShardFlaskItemData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(7);
		writer.Write(value.Index);
		writer.Write(value.GroupIndex);
		writer.Write(value.ShowIndex);
		writer.Write(value.FlaskGauge);
		writer.Write(value.ShardItemIndex);
		writer.Write(value.IsDefault);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.BattleIconPath, options);
	}

	public ShardFlaskItemData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		ShardFlaskItemData shardFlaskItemData = new ShardFlaskItemData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				shardFlaskItemData.Index = reader.ReadInt32();
				break;
			case 1:
				shardFlaskItemData.GroupIndex = reader.ReadInt32();
				break;
			case 2:
				shardFlaskItemData.ShowIndex = reader.ReadInt32();
				break;
			case 3:
				shardFlaskItemData.FlaskGauge = reader.ReadInt32();
				break;
			case 4:
				shardFlaskItemData.ShardItemIndex = reader.ReadInt32();
				break;
			case 5:
				shardFlaskItemData.IsDefault = reader.ReadBoolean();
				break;
			case 6:
				shardFlaskItemData.BattleIconPath = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return shardFlaskItemData;
	}
}

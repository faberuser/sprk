using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class ItemBreakDataFormatter : IMessagePackFormatter<ItemBreakData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, ItemBreakData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		writer.WriteArrayHeader(2);
		writer.Write(value.ItemIndex);
		writer.Write(value.RewardIndex);
	}

	public ItemBreakData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		ItemBreakData itemBreakData = new ItemBreakData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				itemBreakData.ItemIndex = reader.ReadInt32();
				break;
			case 1:
				itemBreakData.RewardIndex = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return itemBreakData;
	}
}

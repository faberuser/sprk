using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class ItemGroupData_SubDataFormatter : IMessagePackFormatter<ItemGroupData.SubData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, ItemGroupData.SubData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(8);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ItemCode, options);
		writer.Write(value.ItemRate);
		writer.Write(value.ItemMin);
		writer.Write(value.ItemMax);
		writer.Write(value.ItemStar);
		writer.Write(value.ItemStarMin);
		writer.Write(value.ItemStarMax);
		writer.Write(value.Free);
	}

	public ItemGroupData.SubData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		ItemGroupData.SubData subData = new ItemGroupData.SubData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				subData.ItemCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 1:
				subData.ItemRate = reader.ReadInt32();
				break;
			case 2:
				subData.ItemMin = reader.ReadInt32();
				break;
			case 3:
				subData.ItemMax = reader.ReadInt32();
				break;
			case 4:
				subData.ItemStar = reader.ReadInt32();
				break;
			case 5:
				subData.ItemStarMin = reader.ReadInt32();
				break;
			case 6:
				subData.ItemStarMax = reader.ReadInt32();
				break;
			case 7:
				subData.Free = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return subData;
	}
}

using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class ItemProbabilityDataFormatter : IMessagePackFormatter<ItemProbabilityData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, ItemProbabilityData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(6);
		writer.Write(value.ItemIndex);
		writer.Write(value.SubIndex);
		writer.Write(value.InformIndex);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ComponentTitle, options);
		writer.Write(value.ComponentCount);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ComponentRate, options);
	}

	public ItemProbabilityData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		ItemProbabilityData itemProbabilityData = new ItemProbabilityData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				itemProbabilityData.ItemIndex = reader.ReadInt32();
				break;
			case 1:
				itemProbabilityData.SubIndex = reader.ReadInt32();
				break;
			case 2:
				itemProbabilityData.InformIndex = reader.ReadInt32();
				break;
			case 3:
				itemProbabilityData.ComponentTitle = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 4:
				itemProbabilityData.ComponentCount = reader.ReadInt32();
				break;
			case 5:
				itemProbabilityData.ComponentRate = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return itemProbabilityData;
	}
}

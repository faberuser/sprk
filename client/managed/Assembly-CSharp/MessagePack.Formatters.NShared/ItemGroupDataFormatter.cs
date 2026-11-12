using System.Collections.Generic;
using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class ItemGroupDataFormatter : IMessagePackFormatter<ItemGroupData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, ItemGroupData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(6);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ItemGroupCode, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ItemGroupDesc, options);
		writer.Write(value.ShowDetail);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ItemGroupNameKey, options);
		writer.Write(value.TotalRate);
		resolver.GetFormatterWithVerify<List<ItemGroupData.SubData>>().Serialize(ref writer, value.SubDataList, options);
	}

	public ItemGroupData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		ItemGroupData itemGroupData = new ItemGroupData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				itemGroupData.ItemGroupCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 1:
				itemGroupData.ItemGroupDesc = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 2:
				itemGroupData.ShowDetail = reader.ReadBoolean();
				break;
			case 3:
				itemGroupData.ItemGroupNameKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 4:
				itemGroupData.TotalRate = reader.ReadInt32();
				break;
			case 5:
				itemGroupData.SubDataList = resolver.GetFormatterWithVerify<List<ItemGroupData.SubData>>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return itemGroupData;
	}
}

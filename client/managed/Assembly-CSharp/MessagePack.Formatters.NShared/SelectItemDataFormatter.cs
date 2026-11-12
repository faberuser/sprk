using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class SelectItemDataFormatter : IMessagePackFormatter<SelectItemData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, SelectItemData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(4);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ItemCode, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.ItemIndices, options);
		resolver.GetFormatterWithVerify<SelectItemPopupType>().Serialize(ref writer, value.SelectType, options);
		writer.Write(value.SelectAbleCount);
	}

	public SelectItemData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		SelectItemData selectItemData = new SelectItemData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				selectItemData.ItemCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 1:
				selectItemData.ItemIndices = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 2:
				selectItemData.SelectType = resolver.GetFormatterWithVerify<SelectItemPopupType>().Deserialize(ref reader, options);
				break;
			case 3:
				selectItemData.SelectAbleCount = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return selectItemData;
	}
}

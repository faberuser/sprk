using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class GachaSelectItemDataFormatter : IMessagePackFormatter<GachaSelectItemData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, GachaSelectItemData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(4);
		writer.Write(value.ItemIndex);
		resolver.GetFormatterWithVerify<EquipGachaType>().Serialize(ref writer, value.GachaType, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.GachaIndices, options);
		resolver.GetFormatterWithVerify<NewPayShopCategoryType>().Serialize(ref writer, value.GachaCategoryType, options);
	}

	public GachaSelectItemData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		GachaSelectItemData gachaSelectItemData = new GachaSelectItemData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				gachaSelectItemData.ItemIndex = reader.ReadInt32();
				break;
			case 1:
				gachaSelectItemData.GachaType = resolver.GetFormatterWithVerify<EquipGachaType>().Deserialize(ref reader, options);
				break;
			case 2:
				gachaSelectItemData.GachaIndices = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 3:
				gachaSelectItemData.GachaCategoryType = resolver.GetFormatterWithVerify<NewPayShopCategoryType>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return gachaSelectItemData;
	}
}

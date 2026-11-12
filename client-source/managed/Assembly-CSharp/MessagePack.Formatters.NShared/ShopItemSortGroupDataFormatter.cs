using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class ShopItemSortGroupDataFormatter : IMessagePackFormatter<ShopItemSortGroupData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, ShopItemSortGroupData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(4);
		writer.Write(value.Index);
		resolver.GetFormatterWithVerify<EShopItemFilterGroupType>().Serialize(ref writer, value.GroupType, options);
		writer.Write(value.SortingOrder);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.LocalKey, options);
	}

	public ShopItemSortGroupData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		ShopItemSortGroupData shopItemSortGroupData = new ShopItemSortGroupData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				shopItemSortGroupData.Index = reader.ReadInt32();
				break;
			case 1:
				shopItemSortGroupData.GroupType = resolver.GetFormatterWithVerify<EShopItemFilterGroupType>().Deserialize(ref reader, options);
				break;
			case 2:
				shopItemSortGroupData.SortingOrder = reader.ReadInt32();
				break;
			case 3:
				shopItemSortGroupData.LocalKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return shopItemSortGroupData;
	}
}

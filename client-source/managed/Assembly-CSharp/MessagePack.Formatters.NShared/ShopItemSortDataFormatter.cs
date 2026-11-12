using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class ShopItemSortDataFormatter : IMessagePackFormatter<ShopItemSortData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, ShopItemSortData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(6);
		writer.Write(value.Index);
		writer.Write(value.GroupIndex);
		writer.Write(value.SortingOrder);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.LocalKey, options);
		resolver.GetFormatterWithVerify<EShopItemType>().Serialize(ref writer, value.ShopItemType, options);
		resolver.GetFormatterWithVerify<EShopSortType>().Serialize(ref writer, value.ShopSortType, options);
	}

	public ShopItemSortData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		ShopItemSortData shopItemSortData = new ShopItemSortData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				shopItemSortData.Index = reader.ReadInt32();
				break;
			case 1:
				shopItemSortData.GroupIndex = reader.ReadInt32();
				break;
			case 2:
				shopItemSortData.SortingOrder = reader.ReadInt32();
				break;
			case 3:
				shopItemSortData.LocalKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 4:
				shopItemSortData.ShopItemType = resolver.GetFormatterWithVerify<EShopItemType>().Deserialize(ref reader, options);
				break;
			case 5:
				shopItemSortData.ShopSortType = resolver.GetFormatterWithVerify<EShopSortType>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return shopItemSortData;
	}
}

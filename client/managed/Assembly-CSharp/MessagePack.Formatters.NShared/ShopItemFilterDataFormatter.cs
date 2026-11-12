using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class ShopItemFilterDataFormatter : IMessagePackFormatter<ShopItemFilterData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, ShopItemFilterData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(3);
		writer.Write(value.Index);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ItemCode, options);
		resolver.GetFormatterWithVerify<EShopItemType>().Serialize(ref writer, value.ShopItemType, options);
	}

	public ShopItemFilterData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		ShopItemFilterData shopItemFilterData = new ShopItemFilterData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				shopItemFilterData.Index = reader.ReadInt32();
				break;
			case 1:
				shopItemFilterData.ItemCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 2:
				shopItemFilterData.ShopItemType = resolver.GetFormatterWithVerify<EShopItemType>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return shopItemFilterData;
	}
}

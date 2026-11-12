using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class NewPayShopProductDataFormatter : IMessagePackFormatter<NewPayShopProductData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, NewPayShopProductData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(10);
		writer.Write(value.Index);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.GoogleId, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.OneStoreId, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.AppleId, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.UDPStoreId, options);
		writer.Write(value.ExcludeConsoleRequest);
		resolver.GetFormatterWithVerify<NewPayShopProductCategoryType>().Serialize(ref writer, value.Category, options);
		resolver.GetFormatterWithVerify<ProductType>().Serialize(ref writer, value.ProductType, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.RewardAnyMiscTypes, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ParentAnyMiscType, options);
	}

	public NewPayShopProductData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		NewPayShopProductData newPayShopProductData = new NewPayShopProductData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				newPayShopProductData.Index = reader.ReadInt32();
				break;
			case 1:
				newPayShopProductData.GoogleId = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 2:
				newPayShopProductData.OneStoreId = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 3:
				newPayShopProductData.AppleId = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 4:
				newPayShopProductData.UDPStoreId = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 5:
				newPayShopProductData.ExcludeConsoleRequest = reader.ReadBoolean();
				break;
			case 6:
				newPayShopProductData.Category = resolver.GetFormatterWithVerify<NewPayShopProductCategoryType>().Deserialize(ref reader, options);
				break;
			case 7:
				newPayShopProductData.ProductType = resolver.GetFormatterWithVerify<ProductType>().Deserialize(ref reader, options);
				break;
			case 8:
				newPayShopProductData.RewardAnyMiscTypes = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 9:
				newPayShopProductData.ParentAnyMiscType = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return newPayShopProductData;
	}
}

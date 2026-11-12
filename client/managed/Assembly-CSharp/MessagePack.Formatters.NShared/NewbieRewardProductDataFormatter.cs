using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class NewbieRewardProductDataFormatter : IMessagePackFormatter<NewbieRewardProductData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, NewbieRewardProductData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(7);
		writer.Write(value.Index);
		resolver.GetFormatterWithVerify<NewbieRewardType>().Serialize(ref writer, value.RewardType, options);
		resolver.GetFormatterWithVerify<NewbieRewardPurchaseType>().Serialize(ref writer, value.PurchaseType, options);
		writer.Write(value.PayShopProductIndex);
		writer.Write(value.GroupIndex);
		writer.Write(value.ShowIndex);
		writer.Write(value.ProductGroupIndex);
	}

	public NewbieRewardProductData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		NewbieRewardProductData newbieRewardProductData = new NewbieRewardProductData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				newbieRewardProductData.Index = reader.ReadInt32();
				break;
			case 1:
				newbieRewardProductData.RewardType = resolver.GetFormatterWithVerify<NewbieRewardType>().Deserialize(ref reader, options);
				break;
			case 2:
				newbieRewardProductData.PurchaseType = resolver.GetFormatterWithVerify<NewbieRewardPurchaseType>().Deserialize(ref reader, options);
				break;
			case 3:
				newbieRewardProductData.PayShopProductIndex = reader.ReadInt32();
				break;
			case 4:
				newbieRewardProductData.GroupIndex = reader.ReadInt32();
				break;
			case 5:
				newbieRewardProductData.ShowIndex = reader.ReadInt32();
				break;
			case 6:
				newbieRewardProductData.ProductGroupIndex = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return newbieRewardProductData;
	}
}

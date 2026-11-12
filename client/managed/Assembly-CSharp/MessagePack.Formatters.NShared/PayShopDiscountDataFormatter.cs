using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class PayShopDiscountDataFormatter : IMessagePackFormatter<PayShopDiscountData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, PayShopDiscountData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(4);
		writer.Write(value.ProductIndex);
		writer.Write(value.GroupIndex);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.TitleKey, options);
		writer.Write(value.AnimationIndex);
	}

	public PayShopDiscountData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		PayShopDiscountData payShopDiscountData = new PayShopDiscountData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				payShopDiscountData.ProductIndex = reader.ReadInt32();
				break;
			case 1:
				payShopDiscountData.GroupIndex = reader.ReadInt32();
				break;
			case 2:
				payShopDiscountData.TitleKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 3:
				payShopDiscountData.AnimationIndex = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return payShopDiscountData;
	}
}

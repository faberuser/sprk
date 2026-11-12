using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class GachaSelectShopDataFormatter : IMessagePackFormatter<GachaSelectShopData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, GachaSelectShopData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(9);
		writer.Write(value.Index);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.CategoryGroup, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.CategoryName, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ItemGroupCode, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ItemGroupCodeNameKeyOther, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ItemGroupCodeNameKeyApple, options);
		writer.Write(value.ProductIndexGem);
		writer.Write(value.ProductIndexCash);
		writer.Write(value.Rate);
	}

	public GachaSelectShopData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		GachaSelectShopData gachaSelectShopData = new GachaSelectShopData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				gachaSelectShopData.Index = reader.ReadInt32();
				break;
			case 1:
				gachaSelectShopData.CategoryGroup = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 2:
				gachaSelectShopData.CategoryName = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 3:
				gachaSelectShopData.ItemGroupCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 4:
				gachaSelectShopData.ItemGroupCodeNameKeyOther = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 5:
				gachaSelectShopData.ItemGroupCodeNameKeyApple = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 6:
				gachaSelectShopData.ProductIndexGem = reader.ReadInt32();
				break;
			case 7:
				gachaSelectShopData.ProductIndexCash = reader.ReadInt32();
				break;
			case 8:
				gachaSelectShopData.Rate = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return gachaSelectShopData;
	}
}

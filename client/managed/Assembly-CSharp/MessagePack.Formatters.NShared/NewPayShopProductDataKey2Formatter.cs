using System;
using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class NewPayShopProductDataKey2Formatter : IMessagePackFormatter<NewPayShopProductDataKey2>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, NewPayShopProductDataKey2 value, MessagePackSerializerOptions options)
	{
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(2);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ProductId, options);
		writer.Write(value.ProductIndex);
	}

	public NewPayShopProductDataKey2 Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			throw new InvalidOperationException("typecode is null, struct not supported");
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		string productId = null;
		int productIndex = 0;
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				productId = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 1:
				productIndex = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		NewPayShopProductDataKey2 result = new NewPayShopProductDataKey2(productId, productIndex);
		reader.Depth--;
		return result;
	}
}

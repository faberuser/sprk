using System;
using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class ValanceEnchantDataKeyFormatter : IMessagePackFormatter<ValanceEnchantDataKey>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, ValanceEnchantDataKey value, MessagePackSerializerOptions options)
	{
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(3);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ItemCode, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.PartType, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.SubType, options);
	}

	public ValanceEnchantDataKey Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			throw new InvalidOperationException("typecode is null, struct not supported");
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		string itemCode = null;
		string partType = null;
		string subType = null;
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				itemCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 1:
				partType = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 2:
				subType = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		ValanceEnchantDataKey result = new ValanceEnchantDataKey(itemCode, partType, subType);
		reader.Depth--;
		return result;
	}
}

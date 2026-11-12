using System;
using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class ValanceEnchantOptionCountDataKeyFormatter : IMessagePackFormatter<ValanceEnchantOptionCountDataKey>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, ValanceEnchantOptionCountDataKey value, MessagePackSerializerOptions options)
	{
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(2);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ItemCode, options);
		writer.Write(value.OptionCount);
	}

	public ValanceEnchantOptionCountDataKey Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			throw new InvalidOperationException("typecode is null, struct not supported");
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		string itemCode = null;
		int optionCount = 0;
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				itemCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 1:
				optionCount = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		ValanceEnchantOptionCountDataKey result = new ValanceEnchantOptionCountDataKey(itemCode, optionCount);
		reader.Depth--;
		return result;
	}
}

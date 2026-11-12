using System;
using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class OptionSellKeyFormatter : IMessagePackFormatter<OptionSellKey>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, OptionSellKey value, MessagePackSerializerOptions options)
	{
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(2);
		resolver.GetFormatterWithVerify<OptionSellCategory>().Serialize(ref writer, value.Category, options);
		writer.Write(value.Value);
	}

	public OptionSellKey Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			throw new InvalidOperationException("typecode is null, struct not supported");
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		OptionSellCategory category = OptionSellCategory.Tier;
		int value = 0;
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				category = resolver.GetFormatterWithVerify<OptionSellCategory>().Deserialize(ref reader, options);
				break;
			case 1:
				value = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		OptionSellKey result = new OptionSellKey(category, value);
		reader.Depth--;
		return result;
	}
}

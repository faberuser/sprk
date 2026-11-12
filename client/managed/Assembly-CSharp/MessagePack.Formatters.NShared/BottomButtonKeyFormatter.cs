using System;
using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class BottomButtonKeyFormatter : IMessagePackFormatter<BottomButtonKey>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, BottomButtonKey value, MessagePackSerializerOptions options)
	{
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(2);
		resolver.GetFormatterWithVerify<ERightMenuButton>().Serialize(ref writer, value.Type, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.SubType, options);
	}

	public BottomButtonKey Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			throw new InvalidOperationException("typecode is null, struct not supported");
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		ERightMenuButton type = ERightMenuButton.None;
		string subType = null;
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				type = resolver.GetFormatterWithVerify<ERightMenuButton>().Deserialize(ref reader, options);
				break;
			case 1:
				subType = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		BottomButtonKey result = new BottomButtonKey(type, subType);
		reader.Depth--;
		return result;
	}
}

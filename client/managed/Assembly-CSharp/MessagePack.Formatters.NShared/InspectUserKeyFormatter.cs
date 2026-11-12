using System;
using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class InspectUserKeyFormatter : IMessagePackFormatter<InspectUserKey>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, InspectUserKey value, MessagePackSerializerOptions options)
	{
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(2);
		resolver.GetFormatterWithVerify<InspectUserCategory>().Serialize(ref writer, value.Category, options);
		resolver.GetFormatterWithVerify<InspectUserType>().Serialize(ref writer, value.Type, options);
	}

	public InspectUserKey Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			throw new InvalidOperationException("typecode is null, struct not supported");
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		InspectUserCategory category = InspectUserCategory.None;
		InspectUserType type = InspectUserType.None;
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				category = resolver.GetFormatterWithVerify<InspectUserCategory>().Deserialize(ref reader, options);
				break;
			case 1:
				type = resolver.GetFormatterWithVerify<InspectUserType>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		InspectUserKey result = new InspectUserKey(category, type);
		reader.Depth--;
		return result;
	}
}

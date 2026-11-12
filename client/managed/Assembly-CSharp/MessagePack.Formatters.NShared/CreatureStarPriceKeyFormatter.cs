using System;
using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class CreatureStarPriceKeyFormatter : IMessagePackFormatter<CreatureStarPriceKey>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, CreatureStarPriceKey value, MessagePackSerializerOptions options)
	{
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(2);
		resolver.GetFormatterWithVerify<CreatureOpenStatusType>().Serialize(ref writer, value.OpenStatus, options);
		writer.Write(value.Star);
	}

	public CreatureStarPriceKey Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			throw new InvalidOperationException("typecode is null, struct not supported");
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		CreatureOpenStatusType openStatus = CreatureOpenStatusType.Normal;
		int star = 0;
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				openStatus = resolver.GetFormatterWithVerify<CreatureOpenStatusType>().Deserialize(ref reader, options);
				break;
			case 1:
				star = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		CreatureStarPriceKey result = new CreatureStarPriceKey(openStatus, star);
		reader.Depth--;
		return result;
	}
}

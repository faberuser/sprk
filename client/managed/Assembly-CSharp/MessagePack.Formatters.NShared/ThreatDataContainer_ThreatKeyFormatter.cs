using System;
using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class ThreatDataContainer_ThreatKeyFormatter : IMessagePackFormatter<ThreatDataContainer.ThreatKey>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, ThreatDataContainer.ThreatKey value, MessagePackSerializerOptions options)
	{
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(2);
		resolver.GetFormatterWithVerify<CreatureTagType>().Serialize(ref writer, value.TagType, options);
		resolver.GetFormatterWithVerify<ThreatType>().Serialize(ref writer, value.ThreatType, options);
	}

	public ThreatDataContainer.ThreatKey Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			throw new InvalidOperationException("typecode is null, struct not supported");
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		CreatureTagType tagType = CreatureTagType.None;
		ThreatType threatType = ThreatType.None;
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				tagType = resolver.GetFormatterWithVerify<CreatureTagType>().Deserialize(ref reader, options);
				break;
			case 1:
				threatType = resolver.GetFormatterWithVerify<ThreatType>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		ThreatDataContainer.ThreatKey result = new ThreatDataContainer.ThreatKey(tagType, threatType);
		reader.Depth--;
		return result;
	}
}

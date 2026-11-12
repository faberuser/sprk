using System;
using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class ValanceAwakenDataKeyFormatter : IMessagePackFormatter<ValanceAwakenDataKey>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, ValanceAwakenDataKey value, MessagePackSerializerOptions options)
	{
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(4);
		writer.Write(value.Tier);
		writer.Write(value.DetailIndex);
		resolver.GetFormatterWithVerify<CreatureTagType>().Serialize(ref writer, value.TagType, options);
		writer.Write(value.Star);
	}

	public ValanceAwakenDataKey Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			throw new InvalidOperationException("typecode is null, struct not supported");
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		int tier = 0;
		int detailIndex = 0;
		CreatureTagType tagType = CreatureTagType.None;
		int star = 0;
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				tier = reader.ReadInt32();
				break;
			case 1:
				detailIndex = reader.ReadInt32();
				break;
			case 2:
				tagType = resolver.GetFormatterWithVerify<CreatureTagType>().Deserialize(ref reader, options);
				break;
			case 3:
				star = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		ValanceAwakenDataKey result = new ValanceAwakenDataKey(tier, detailIndex, tagType, star);
		reader.Depth--;
		return result;
	}
}

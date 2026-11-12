using System;
using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class CreatureStarStatDataKeyFormatter : IMessagePackFormatter<CreatureStarStatDataKey>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, CreatureStarStatDataKey value, MessagePackSerializerOptions options)
	{
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(3);
		writer.Write(value.Star);
		writer.Write(value.Transcended);
		resolver.GetFormatterWithVerify<StatType>().Serialize(ref writer, value.StatType, options);
	}

	public CreatureStarStatDataKey Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			throw new InvalidOperationException("typecode is null, struct not supported");
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		int star = 0;
		int transcended = 0;
		StatType statType = StatType.None;
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				star = reader.ReadInt32();
				break;
			case 1:
				transcended = reader.ReadInt32();
				break;
			case 2:
				statType = resolver.GetFormatterWithVerify<StatType>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		CreatureStarStatDataKey result = new CreatureStarStatDataKey(star, transcended, statType);
		reader.Depth--;
		return result;
	}
}

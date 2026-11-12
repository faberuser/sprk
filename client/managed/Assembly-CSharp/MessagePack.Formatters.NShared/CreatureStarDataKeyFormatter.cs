using System;
using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class CreatureStarDataKeyFormatter : IMessagePackFormatter<CreatureStarDataKey>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, CreatureStarDataKey value, MessagePackSerializerOptions options)
	{
		writer.WriteArrayHeader(2);
		writer.Write(value.Star);
		writer.Write(value.Transcended);
	}

	public CreatureStarDataKey Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			throw new InvalidOperationException("typecode is null, struct not supported");
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		int star = 0;
		int transcended = 0;
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
			default:
				reader.Skip();
				break;
			}
		}
		CreatureStarDataKey result = new CreatureStarDataKey(star, transcended);
		reader.Depth--;
		return result;
	}
}

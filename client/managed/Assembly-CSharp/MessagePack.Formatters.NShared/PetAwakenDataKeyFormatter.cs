using System;
using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class PetAwakenDataKeyFormatter : IMessagePackFormatter<PetAwakenDataKey>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, PetAwakenDataKey value, MessagePackSerializerOptions options)
	{
		writer.WriteArrayHeader(2);
		writer.Write(value.PetIndex);
		writer.Write(value.Star);
	}

	public PetAwakenDataKey Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			throw new InvalidOperationException("typecode is null, struct not supported");
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		int petIndex = 0;
		int star = 0;
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				petIndex = reader.ReadInt32();
				break;
			case 1:
				star = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		PetAwakenDataKey result = new PetAwakenDataKey(petIndex, star);
		reader.Depth--;
		return result;
	}
}

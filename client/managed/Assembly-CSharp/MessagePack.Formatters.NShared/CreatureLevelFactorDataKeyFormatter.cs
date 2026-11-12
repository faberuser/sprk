using System;
using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class CreatureLevelFactorDataKeyFormatter : IMessagePackFormatter<CreatureLevelFactorDataKey>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, CreatureLevelFactorDataKey value, MessagePackSerializerOptions options)
	{
		writer.WriteArrayHeader(2);
		writer.Write(value.FactorIndex);
		writer.Write(value.Level);
	}

	public CreatureLevelFactorDataKey Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			throw new InvalidOperationException("typecode is null, struct not supported");
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		int factorIndex = 0;
		int level = 0;
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				factorIndex = reader.ReadInt32();
				break;
			case 1:
				level = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		CreatureLevelFactorDataKey result = new CreatureLevelFactorDataKey(factorIndex, level);
		reader.Depth--;
		return result;
	}
}

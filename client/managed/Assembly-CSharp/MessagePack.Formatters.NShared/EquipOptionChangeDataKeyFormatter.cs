using System;
using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class EquipOptionChangeDataKeyFormatter : IMessagePackFormatter<EquipOptionChangeDataKey>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, EquipOptionChangeDataKey value, MessagePackSerializerOptions options)
	{
		writer.WriteArrayHeader(3);
		writer.Write(value.OptionIndex);
		writer.Write(value.BeforeTier);
		writer.Write(value.AfterTier);
	}

	public EquipOptionChangeDataKey Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			throw new InvalidOperationException("typecode is null, struct not supported");
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		int optionIndex = 0;
		int beforeTier = 0;
		int afterTier = 0;
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				optionIndex = reader.ReadInt32();
				break;
			case 1:
				beforeTier = reader.ReadInt32();
				break;
			case 2:
				afterTier = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		EquipOptionChangeDataKey result = new EquipOptionChangeDataKey(optionIndex, beforeTier, afterTier);
		reader.Depth--;
		return result;
	}
}

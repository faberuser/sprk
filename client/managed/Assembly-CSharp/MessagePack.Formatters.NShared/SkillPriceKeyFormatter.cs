using System;
using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class SkillPriceKeyFormatter : IMessagePackFormatter<SkillPriceKey>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, SkillPriceKey value, MessagePackSerializerOptions options)
	{
		writer.WriteArrayHeader(2);
		writer.Write(value.SlotIndex);
		writer.Write(value.Level);
	}

	public SkillPriceKey Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			throw new InvalidOperationException("typecode is null, struct not supported");
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		int slotIndex = 0;
		int level = 0;
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				slotIndex = reader.ReadInt32();
				break;
			case 1:
				level = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		SkillPriceKey result = new SkillPriceKey(slotIndex, level);
		reader.Depth--;
		return result;
	}
}

using System;
using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class EquipAutoKeyFormatter : IMessagePackFormatter<EquipAutoKey>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, EquipAutoKey value, MessagePackSerializerOptions options)
	{
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(2);
		resolver.GetFormatterWithVerify<CreatureTagType>().Serialize(ref writer, value.CreatureTagType, options);
		writer.Write(value.EquipSlotIndex);
	}

	public EquipAutoKey Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			throw new InvalidOperationException("typecode is null, struct not supported");
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		CreatureTagType tagType = CreatureTagType.None;
		int slotIndex = 0;
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				tagType = resolver.GetFormatterWithVerify<CreatureTagType>().Deserialize(ref reader, options);
				break;
			case 1:
				slotIndex = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		EquipAutoKey result = new EquipAutoKey(tagType, slotIndex);
		reader.Depth--;
		return result;
	}
}

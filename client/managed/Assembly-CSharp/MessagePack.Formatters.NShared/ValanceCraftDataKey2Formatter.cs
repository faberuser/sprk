using System;
using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class ValanceCraftDataKey2Formatter : IMessagePackFormatter<ValanceCraftDataKey2>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, ValanceCraftDataKey2 value, MessagePackSerializerOptions options)
	{
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(4);
		resolver.GetFormatterWithVerify<EquipItemSetGroupType>().Serialize(ref writer, value.SetGroupType, options);
		resolver.GetFormatterWithVerify<CreatureTagType>().Serialize(ref writer, value.TagType, options);
		resolver.GetFormatterWithVerify<EquipPartType>().Serialize(ref writer, value.PartType, options);
		resolver.GetFormatterWithVerify<EquipItemSubType>().Serialize(ref writer, value.ItemSubType, options);
	}

	public ValanceCraftDataKey2 Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			throw new InvalidOperationException("typecode is null, struct not supported");
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		EquipItemSetGroupType setGroupType = EquipItemSetGroupType.None;
		CreatureTagType tagType = CreatureTagType.None;
		EquipPartType partType = EquipPartType.None;
		EquipItemSubType itemSubType = EquipItemSubType.SpecialWeapon;
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				setGroupType = resolver.GetFormatterWithVerify<EquipItemSetGroupType>().Deserialize(ref reader, options);
				break;
			case 1:
				tagType = resolver.GetFormatterWithVerify<CreatureTagType>().Deserialize(ref reader, options);
				break;
			case 2:
				partType = resolver.GetFormatterWithVerify<EquipPartType>().Deserialize(ref reader, options);
				break;
			case 3:
				itemSubType = resolver.GetFormatterWithVerify<EquipItemSubType>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		ValanceCraftDataKey2 result = new ValanceCraftDataKey2(setGroupType, tagType, partType, itemSubType);
		reader.Depth--;
		return result;
	}
}

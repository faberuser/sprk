using System;
using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class ValanceEquipFilterDataContainer_SortingOrderKey2Formatter : IMessagePackFormatter<ValanceEquipFilterDataContainer.SortingOrderKey2>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, ValanceEquipFilterDataContainer.SortingOrderKey2 value, MessagePackSerializerOptions options)
	{
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(5);
		resolver.GetFormatterWithVerify<EquipItemSetGroupType>().Serialize(ref writer, value.SetType, options);
		resolver.GetFormatterWithVerify<CreatureTagType>().Serialize(ref writer, value.TagType, options);
		resolver.GetFormatterWithVerify<EquipPartType>().Serialize(ref writer, value.PartType, options);
		resolver.GetFormatterWithVerify<EquipItemSubType>().Serialize(ref writer, value.ItemSubType, options);
		writer.Write(value.ExtraSkillIndex);
	}

	public ValanceEquipFilterDataContainer.SortingOrderKey2 Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			throw new InvalidOperationException("typecode is null, struct not supported");
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		EquipItemSetGroupType setType = EquipItemSetGroupType.None;
		CreatureTagType tagType = CreatureTagType.None;
		EquipPartType partType = EquipPartType.None;
		EquipItemSubType itemSubType = EquipItemSubType.SpecialWeapon;
		int extraSkillIndex = 0;
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				setType = resolver.GetFormatterWithVerify<EquipItemSetGroupType>().Deserialize(ref reader, options);
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
			case 4:
				extraSkillIndex = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		ValanceEquipFilterDataContainer.SortingOrderKey2 result = new ValanceEquipFilterDataContainer.SortingOrderKey2(setType, tagType, partType, itemSubType, extraSkillIndex);
		reader.Depth--;
		return result;
	}
}

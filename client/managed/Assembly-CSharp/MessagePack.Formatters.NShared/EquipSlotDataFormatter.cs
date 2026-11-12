using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class EquipSlotDataFormatter : IMessagePackFormatter<EquipSlotData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, EquipSlotData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(8);
		writer.Write(value.Index);
		resolver.GetFormatterWithVerify<EquipPartType>().Serialize(ref writer, value.EquipPartType, options);
		resolver.GetFormatterWithVerify<EquipItemSubType[]>().Serialize(ref writer, value.EquipItemSubType, options);
		writer.Write(value.GroupIndex);
		writer.Write(value.IgnoreStat);
		writer.Write(value.IgnoreFixedOption);
		writer.Write(value.IgnoreRandomOption);
		writer.Write(value.IgnoreEnchantOption);
	}

	public EquipSlotData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		EquipSlotData equipSlotData = new EquipSlotData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				equipSlotData.Index = reader.ReadInt32();
				break;
			case 1:
				equipSlotData.EquipPartType = resolver.GetFormatterWithVerify<EquipPartType>().Deserialize(ref reader, options);
				break;
			case 2:
				equipSlotData.EquipItemSubType = resolver.GetFormatterWithVerify<EquipItemSubType[]>().Deserialize(ref reader, options);
				break;
			case 3:
				equipSlotData.GroupIndex = reader.ReadInt32();
				break;
			case 4:
				equipSlotData.IgnoreStat = reader.ReadBoolean();
				break;
			case 5:
				equipSlotData.IgnoreFixedOption = reader.ReadBoolean();
				break;
			case 6:
				equipSlotData.IgnoreRandomOption = reader.ReadBoolean();
				break;
			case 7:
				equipSlotData.IgnoreEnchantOption = reader.ReadBoolean();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return equipSlotData;
	}
}

using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class EquipAutoDataFormatter : IMessagePackFormatter<EquipAutoData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, EquipAutoData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(10);
		resolver.GetFormatterWithVerify<CreatureTagType>().Serialize(ref writer, value.CreatureTagType, options);
		writer.Write(value.EquipSlotIndex);
		resolver.GetFormatterWithVerify<StatType[]>().Serialize(ref writer, value.MainStatType1, options);
		writer.Write(value.MainStatFactor1);
		resolver.GetFormatterWithVerify<StatType[]>().Serialize(ref writer, value.MainStatType2, options);
		writer.Write(value.MainStatFactor2);
		resolver.GetFormatterWithVerify<StatType[]>().Serialize(ref writer, value.MainStatType3, options);
		writer.Write(value.MainStatFactor3);
		resolver.GetFormatterWithVerify<StatType[]>().Serialize(ref writer, value.ExtraStatType, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.ExtraStatApplyCondition, options);
	}

	public EquipAutoData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		EquipAutoData equipAutoData = new EquipAutoData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				equipAutoData.CreatureTagType = resolver.GetFormatterWithVerify<CreatureTagType>().Deserialize(ref reader, options);
				break;
			case 1:
				equipAutoData.EquipSlotIndex = reader.ReadInt32();
				break;
			case 2:
				equipAutoData.MainStatType1 = resolver.GetFormatterWithVerify<StatType[]>().Deserialize(ref reader, options);
				break;
			case 3:
				equipAutoData.MainStatFactor1 = reader.ReadInt32();
				break;
			case 4:
				equipAutoData.MainStatType2 = resolver.GetFormatterWithVerify<StatType[]>().Deserialize(ref reader, options);
				break;
			case 5:
				equipAutoData.MainStatFactor2 = reader.ReadInt32();
				break;
			case 6:
				equipAutoData.MainStatType3 = resolver.GetFormatterWithVerify<StatType[]>().Deserialize(ref reader, options);
				break;
			case 7:
				equipAutoData.MainStatFactor3 = reader.ReadInt32();
				break;
			case 8:
				equipAutoData.ExtraStatType = resolver.GetFormatterWithVerify<StatType[]>().Deserialize(ref reader, options);
				break;
			case 9:
				equipAutoData.ExtraStatApplyCondition = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return equipAutoData;
	}
}

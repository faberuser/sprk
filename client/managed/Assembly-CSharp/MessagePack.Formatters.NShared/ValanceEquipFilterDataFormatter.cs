using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class ValanceEquipFilterDataFormatter : IMessagePackFormatter<ValanceEquipFilterData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, ValanceEquipFilterData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(10);
		writer.Write(value.Index);
		writer.Write(value.GroupIndex);
		writer.Write(value.SortingOrder);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.LocalKey, options);
		resolver.GetFormatterWithVerify<EquipItemSetGroupType>().Serialize(ref writer, value.SetType, options);
		resolver.GetFormatterWithVerify<CreatureTagType>().Serialize(ref writer, value.CreatureTagType, options);
		resolver.GetFormatterWithVerify<EquipPartType>().Serialize(ref writer, value.EquipPartType, options);
		resolver.GetFormatterWithVerify<EquipItemSubType>().Serialize(ref writer, value.EquipItemSubType, options);
		writer.Write(value.DisplayEquipOptionIndex);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.ExtraEquipOptionIndices, options);
	}

	public ValanceEquipFilterData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		ValanceEquipFilterData valanceEquipFilterData = new ValanceEquipFilterData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				valanceEquipFilterData.Index = reader.ReadInt32();
				break;
			case 1:
				valanceEquipFilterData.GroupIndex = reader.ReadInt32();
				break;
			case 2:
				valanceEquipFilterData.SortingOrder = reader.ReadInt32();
				break;
			case 3:
				valanceEquipFilterData.LocalKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 4:
				valanceEquipFilterData.SetType = resolver.GetFormatterWithVerify<EquipItemSetGroupType>().Deserialize(ref reader, options);
				break;
			case 5:
				valanceEquipFilterData.CreatureTagType = resolver.GetFormatterWithVerify<CreatureTagType>().Deserialize(ref reader, options);
				break;
			case 6:
				valanceEquipFilterData.EquipPartType = resolver.GetFormatterWithVerify<EquipPartType>().Deserialize(ref reader, options);
				break;
			case 7:
				valanceEquipFilterData.EquipItemSubType = resolver.GetFormatterWithVerify<EquipItemSubType>().Deserialize(ref reader, options);
				break;
			case 8:
				valanceEquipFilterData.DisplayEquipOptionIndex = reader.ReadInt32();
				break;
			case 9:
				valanceEquipFilterData.ExtraEquipOptionIndices = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return valanceEquipFilterData;
	}
}

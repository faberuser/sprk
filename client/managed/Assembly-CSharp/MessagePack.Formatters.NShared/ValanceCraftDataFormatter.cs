using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class ValanceCraftDataFormatter : IMessagePackFormatter<ValanceCraftData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, ValanceCraftData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(13);
		resolver.GetFormatterWithVerify<EquipItemSetGroupType>().Serialize(ref writer, value.SetType, options);
		resolver.GetFormatterWithVerify<CreatureTagType>().Serialize(ref writer, value.CreatureTagType, options);
		resolver.GetFormatterWithVerify<EquipPartType>().Serialize(ref writer, value.EquipPartType, options);
		resolver.GetFormatterWithVerify<EquipItemSubType>().Serialize(ref writer, value.EquipItemSubType, options);
		writer.Write(value.MaterialItemIndex);
		writer.Write(value.MaterialItemCount);
		writer.Write(value.ReqGold);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ResultItemGroupCode, options);
		writer.Write(value.NoticeCheck);
		writer.Write(value.AdditionalMaterialItemCount);
		writer.Write(value.SubMaterialItemIndex);
		writer.Write(value.SubMaterialItemCount);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.EquipOptionGroupIndex, options);
	}

	public ValanceCraftData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		ValanceCraftData valanceCraftData = new ValanceCraftData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				valanceCraftData.SetType = resolver.GetFormatterWithVerify<EquipItemSetGroupType>().Deserialize(ref reader, options);
				break;
			case 1:
				valanceCraftData.CreatureTagType = resolver.GetFormatterWithVerify<CreatureTagType>().Deserialize(ref reader, options);
				break;
			case 2:
				valanceCraftData.EquipPartType = resolver.GetFormatterWithVerify<EquipPartType>().Deserialize(ref reader, options);
				break;
			case 3:
				valanceCraftData.EquipItemSubType = resolver.GetFormatterWithVerify<EquipItemSubType>().Deserialize(ref reader, options);
				break;
			case 4:
				valanceCraftData.MaterialItemIndex = reader.ReadInt32();
				break;
			case 5:
				valanceCraftData.MaterialItemCount = reader.ReadInt32();
				break;
			case 6:
				valanceCraftData.ReqGold = reader.ReadInt32();
				break;
			case 7:
				valanceCraftData.ResultItemGroupCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 8:
				valanceCraftData.NoticeCheck = reader.ReadBoolean();
				break;
			case 9:
				valanceCraftData.AdditionalMaterialItemCount = reader.ReadInt32();
				break;
			case 10:
				valanceCraftData.SubMaterialItemIndex = reader.ReadInt32();
				break;
			case 11:
				valanceCraftData.SubMaterialItemCount = reader.ReadInt32();
				break;
			case 12:
				valanceCraftData.EquipOptionGroupIndex = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return valanceCraftData;
	}
}

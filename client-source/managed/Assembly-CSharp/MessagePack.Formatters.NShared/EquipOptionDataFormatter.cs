using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class EquipOptionDataFormatter : IMessagePackFormatter<EquipOptionData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, EquipOptionData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(20);
		writer.Write(value.Index);
		writer.Write(value.ShowOptionIndex);
		resolver.GetFormatterWithVerify<OperationType>().Serialize(ref writer, value.OptionType, options);
		writer.Write(value.OptionValueMin0);
		writer.Write(value.OptionValueMax0);
		writer.Write(value.OptionValueMin1);
		writer.Write(value.OptionValueMax1);
		writer.Write(value.OptionValueMin2);
		writer.Write(value.OptionValueMax2);
		writer.Write(value.OptionValueMin3);
		writer.Write(value.OptionValueMax3);
		writer.Write(value.OptionValueMin4);
		writer.Write(value.OptionValueMax4);
		writer.Write(value.OptionValueMin5);
		writer.Write(value.OptionValueMax5);
		writer.Write(value.OptionValueStep);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Name, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Description, options);
		writer.Write(value.Ratio);
		writer.Write(value.Enchant);
	}

	public EquipOptionData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		EquipOptionData equipOptionData = new EquipOptionData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				equipOptionData.Index = reader.ReadInt32();
				break;
			case 1:
				equipOptionData.ShowOptionIndex = reader.ReadInt32();
				break;
			case 2:
				equipOptionData.OptionType = resolver.GetFormatterWithVerify<OperationType>().Deserialize(ref reader, options);
				break;
			case 3:
				equipOptionData.OptionValueMin0 = reader.ReadInt32();
				break;
			case 4:
				equipOptionData.OptionValueMax0 = reader.ReadInt32();
				break;
			case 5:
				equipOptionData.OptionValueMin1 = reader.ReadInt32();
				break;
			case 6:
				equipOptionData.OptionValueMax1 = reader.ReadInt32();
				break;
			case 7:
				equipOptionData.OptionValueMin2 = reader.ReadInt32();
				break;
			case 8:
				equipOptionData.OptionValueMax2 = reader.ReadInt32();
				break;
			case 9:
				equipOptionData.OptionValueMin3 = reader.ReadInt32();
				break;
			case 10:
				equipOptionData.OptionValueMax3 = reader.ReadInt32();
				break;
			case 11:
				equipOptionData.OptionValueMin4 = reader.ReadInt32();
				break;
			case 12:
				equipOptionData.OptionValueMax4 = reader.ReadInt32();
				break;
			case 13:
				equipOptionData.OptionValueMin5 = reader.ReadInt32();
				break;
			case 14:
				equipOptionData.OptionValueMax5 = reader.ReadInt32();
				break;
			case 15:
				equipOptionData.OptionValueStep = reader.ReadInt32();
				break;
			case 16:
				equipOptionData.Name = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 17:
				equipOptionData.Description = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 18:
				equipOptionData.Ratio = reader.ReadInt32();
				break;
			case 19:
				equipOptionData.Enchant = reader.ReadByte();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return equipOptionData;
	}
}

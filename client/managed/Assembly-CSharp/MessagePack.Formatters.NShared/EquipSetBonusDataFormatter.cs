using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class EquipSetBonusDataFormatter : IMessagePackFormatter<EquipSetBonusData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, EquipSetBonusData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(8);
		writer.Write(value.SetIndex);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Name, options);
		writer.Write(value.OptionIndex1);
		writer.Write(value.OptionIndex2);
		writer.Write(value.OptionIndex3);
		writer.Write(value.OptionIndex4);
		writer.Write(value.OptionIndex5);
		writer.Write(value.OptionIndex6);
	}

	public EquipSetBonusData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		EquipSetBonusData equipSetBonusData = new EquipSetBonusData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				equipSetBonusData.SetIndex = reader.ReadInt32();
				break;
			case 1:
				equipSetBonusData.Name = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 2:
				equipSetBonusData.OptionIndex1 = reader.ReadInt32();
				break;
			case 3:
				equipSetBonusData.OptionIndex2 = reader.ReadInt32();
				break;
			case 4:
				equipSetBonusData.OptionIndex3 = reader.ReadInt32();
				break;
			case 5:
				equipSetBonusData.OptionIndex4 = reader.ReadInt32();
				break;
			case 6:
				equipSetBonusData.OptionIndex5 = reader.ReadInt32();
				break;
			case 7:
				equipSetBonusData.OptionIndex6 = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return equipSetBonusData;
	}
}

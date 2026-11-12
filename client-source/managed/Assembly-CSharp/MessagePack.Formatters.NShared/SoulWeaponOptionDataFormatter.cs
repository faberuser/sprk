using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class SoulWeaponOptionDataFormatter : IMessagePackFormatter<SoulWeaponOptionData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, SoulWeaponOptionData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(7);
		resolver.GetFormatterWithVerify<CreatureTagType>().Serialize(ref writer, value.TagType, options);
		writer.Write(value.OptionIndex1);
		writer.Write(value.OptionIndex2);
		resolver.GetFormatterWithVerify<StatType[]>().Serialize(ref writer, value.OptionStatType1, options);
		resolver.GetFormatterWithVerify<StatType[]>().Serialize(ref writer, value.OptionStatType2, options);
		writer.Write(value.OptionRatioPerValue1);
		writer.Write(value.OptionRatioPerValue2);
	}

	public SoulWeaponOptionData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		SoulWeaponOptionData soulWeaponOptionData = new SoulWeaponOptionData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				soulWeaponOptionData.TagType = resolver.GetFormatterWithVerify<CreatureTagType>().Deserialize(ref reader, options);
				break;
			case 1:
				soulWeaponOptionData.OptionIndex1 = reader.ReadInt32();
				break;
			case 2:
				soulWeaponOptionData.OptionIndex2 = reader.ReadInt32();
				break;
			case 3:
				soulWeaponOptionData.OptionStatType1 = resolver.GetFormatterWithVerify<StatType[]>().Deserialize(ref reader, options);
				break;
			case 4:
				soulWeaponOptionData.OptionStatType2 = resolver.GetFormatterWithVerify<StatType[]>().Deserialize(ref reader, options);
				break;
			case 5:
				soulWeaponOptionData.OptionRatioPerValue1 = reader.ReadInt32();
				break;
			case 6:
				soulWeaponOptionData.OptionRatioPerValue2 = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return soulWeaponOptionData;
	}
}

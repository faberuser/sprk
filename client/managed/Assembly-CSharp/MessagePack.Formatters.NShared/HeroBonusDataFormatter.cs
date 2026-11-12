using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class HeroBonusDataFormatter : IMessagePackFormatter<HeroBonusData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, HeroBonusData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(17);
		writer.Write(value.HeroIndex);
		writer.Write(value.HeroStar);
		writer.Write(value.HeroTranscended);
		resolver.GetFormatterWithVerify<EventBonusType>().Serialize(ref writer, value.BonusType1, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.BonusValue1, options);
		resolver.GetFormatterWithVerify<EventBonusType>().Serialize(ref writer, value.BonusType2, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.BonusValue2, options);
		resolver.GetFormatterWithVerify<EventBonusType>().Serialize(ref writer, value.BonusType3, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.BonusValue3, options);
		resolver.GetFormatterWithVerify<EventBonusType>().Serialize(ref writer, value.BonusType4, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.BonusValue4, options);
		resolver.GetFormatterWithVerify<EventBonusType>().Serialize(ref writer, value.BonusType5, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.BonusValue5, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.BonusNameKey, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.BonusDescKey, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Icon, options);
		writer.Write(value.ShowBoosterIconTooltip);
	}

	public HeroBonusData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		HeroBonusData heroBonusData = new HeroBonusData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				heroBonusData.HeroIndex = reader.ReadInt32();
				break;
			case 1:
				heroBonusData.HeroStar = reader.ReadInt32();
				break;
			case 2:
				heroBonusData.HeroTranscended = reader.ReadInt32();
				break;
			case 3:
				heroBonusData.BonusType1 = resolver.GetFormatterWithVerify<EventBonusType>().Deserialize(ref reader, options);
				break;
			case 4:
				heroBonusData.BonusValue1 = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 5:
				heroBonusData.BonusType2 = resolver.GetFormatterWithVerify<EventBonusType>().Deserialize(ref reader, options);
				break;
			case 6:
				heroBonusData.BonusValue2 = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 7:
				heroBonusData.BonusType3 = resolver.GetFormatterWithVerify<EventBonusType>().Deserialize(ref reader, options);
				break;
			case 8:
				heroBonusData.BonusValue3 = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 9:
				heroBonusData.BonusType4 = resolver.GetFormatterWithVerify<EventBonusType>().Deserialize(ref reader, options);
				break;
			case 10:
				heroBonusData.BonusValue4 = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 11:
				heroBonusData.BonusType5 = resolver.GetFormatterWithVerify<EventBonusType>().Deserialize(ref reader, options);
				break;
			case 12:
				heroBonusData.BonusValue5 = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 13:
				heroBonusData.BonusNameKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 14:
				heroBonusData.BonusDescKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 15:
				heroBonusData.Icon = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 16:
				heroBonusData.ShowBoosterIconTooltip = reader.ReadBoolean();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return heroBonusData;
	}
}

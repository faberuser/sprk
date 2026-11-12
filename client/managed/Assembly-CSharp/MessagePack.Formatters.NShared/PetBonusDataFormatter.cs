using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class PetBonusDataFormatter : IMessagePackFormatter<PetBonusData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, PetBonusData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(12);
		writer.Write(value.Index);
		resolver.GetFormatterWithVerify<EventBonusType>().Serialize(ref writer, value.BonusType, options);
		writer.Write(value.BonusValue0);
		writer.Write(value.BonusValue1);
		writer.Write(value.BonusValue2);
		writer.Write(value.BonusValue3);
		writer.Write(value.BonusValue4);
		writer.Write(value.BonusValue5);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ConditionType, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.ConditionValue, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.BonusNameKey, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.BonusDescKey, options);
	}

	public PetBonusData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		PetBonusData petBonusData = new PetBonusData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				petBonusData.Index = reader.ReadInt32();
				break;
			case 1:
				petBonusData.BonusType = resolver.GetFormatterWithVerify<EventBonusType>().Deserialize(ref reader, options);
				break;
			case 2:
				petBonusData.BonusValue0 = reader.ReadInt32();
				break;
			case 3:
				petBonusData.BonusValue1 = reader.ReadInt32();
				break;
			case 4:
				petBonusData.BonusValue2 = reader.ReadInt32();
				break;
			case 5:
				petBonusData.BonusValue3 = reader.ReadInt32();
				break;
			case 6:
				petBonusData.BonusValue4 = reader.ReadInt32();
				break;
			case 7:
				petBonusData.BonusValue5 = reader.ReadInt32();
				break;
			case 8:
				petBonusData.ConditionType = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 9:
				petBonusData.ConditionValue = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 10:
				petBonusData.BonusNameKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 11:
				petBonusData.BonusDescKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return petBonusData;
	}
}

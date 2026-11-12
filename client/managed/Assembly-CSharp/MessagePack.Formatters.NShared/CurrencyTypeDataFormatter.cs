using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class CurrencyTypeDataFormatter : IMessagePackFormatter<CurrencyTypeData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, CurrencyTypeData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(15);
		resolver.GetFormatterWithVerify<CurrencyType>().Serialize(ref writer, value.CurrencyType, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.StaminaType, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.UserStatType, options);
		writer.Write(value.Width);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.SpriteName, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.EffectCode, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ButtonAction, options);
		writer.Write(value.DailyLimitValue);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Condition1, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Condition2, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Condition3, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Condition4, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Condition5, options);
		writer.Write(value.ExchangeCostValue);
		writer.Write(value.MaxValue);
	}

	public CurrencyTypeData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		CurrencyTypeData currencyTypeData = new CurrencyTypeData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				currencyTypeData.CurrencyType = resolver.GetFormatterWithVerify<CurrencyType>().Deserialize(ref reader, options);
				break;
			case 1:
				currencyTypeData.StaminaType = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 2:
				currencyTypeData.UserStatType = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 3:
				currencyTypeData.Width = reader.ReadInt32();
				break;
			case 4:
				currencyTypeData.SpriteName = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 5:
				currencyTypeData.EffectCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 6:
				currencyTypeData.ButtonAction = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 7:
				currencyTypeData.DailyLimitValue = reader.ReadInt32();
				break;
			case 8:
				currencyTypeData.Condition1 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 9:
				currencyTypeData.Condition2 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 10:
				currencyTypeData.Condition3 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 11:
				currencyTypeData.Condition4 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 12:
				currencyTypeData.Condition5 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 13:
				currencyTypeData.ExchangeCostValue = reader.ReadInt32();
				break;
			case 14:
				currencyTypeData.MaxValue = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return currencyTypeData;
	}
}

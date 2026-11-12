using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class StoreCurrencyUnitDataFormatter : IMessagePackFormatter<StoreCurrencyUnitData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, StoreCurrencyUnitData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(4);
		writer.Write(value.Index);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.From, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.To, options);
		resolver.GetFormatterWithVerify<CurrencyChangeType>().Serialize(ref writer, value.ChangeType, options);
	}

	public StoreCurrencyUnitData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		StoreCurrencyUnitData storeCurrencyUnitData = new StoreCurrencyUnitData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				storeCurrencyUnitData.Index = reader.ReadInt32();
				break;
			case 1:
				storeCurrencyUnitData.From = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 2:
				storeCurrencyUnitData.To = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 3:
				storeCurrencyUnitData.ChangeType = resolver.GetFormatterWithVerify<CurrencyChangeType>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return storeCurrencyUnitData;
	}
}

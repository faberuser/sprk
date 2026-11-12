using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class EventRouletteDataFormatter : IMessagePackFormatter<EventRouletteData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, EventRouletteData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(7);
		writer.Write(value.Index);
		resolver.GetFormatterWithVerify<EventRouletteType>().Serialize(ref writer, value.Type, options);
		resolver.GetFormatterWithVerify<CurrencyType>().Serialize(ref writer, value.CurrencyType, options);
		writer.Write(value.CurrencyValue);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ItemCode, options);
		writer.Write(value.ItemCount);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.DropItemGroupCode, options);
	}

	public EventRouletteData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		EventRouletteData eventRouletteData = new EventRouletteData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				eventRouletteData.Index = reader.ReadInt32();
				break;
			case 1:
				eventRouletteData.Type = resolver.GetFormatterWithVerify<EventRouletteType>().Deserialize(ref reader, options);
				break;
			case 2:
				eventRouletteData.CurrencyType = resolver.GetFormatterWithVerify<CurrencyType>().Deserialize(ref reader, options);
				break;
			case 3:
				eventRouletteData.CurrencyValue = reader.ReadInt32();
				break;
			case 4:
				eventRouletteData.ItemCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 5:
				eventRouletteData.ItemCount = reader.ReadInt32();
				break;
			case 6:
				eventRouletteData.DropItemGroupCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return eventRouletteData;
	}
}

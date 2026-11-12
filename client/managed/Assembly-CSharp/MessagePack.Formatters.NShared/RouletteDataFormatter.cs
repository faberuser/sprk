using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class RouletteDataFormatter : IMessagePackFormatter<RouletteData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, RouletteData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(3);
		writer.Write(value.Index);
		resolver.GetFormatterWithVerify<RouletteType>().Serialize(ref writer, value.RouletteType, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.RouletteGuide, options);
	}

	public RouletteData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		RouletteData rouletteData = new RouletteData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				rouletteData.Index = reader.ReadInt32();
				break;
			case 1:
				rouletteData.RouletteType = resolver.GetFormatterWithVerify<RouletteType>().Deserialize(ref reader, options);
				break;
			case 2:
				rouletteData.RouletteGuide = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return rouletteData;
	}
}

using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class XCodeLocalizationDataFormatter : IMessagePackFormatter<XCodeLocalizationData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, XCodeLocalizationData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(3);
		writer.Write(value.Index);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Country, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.CountryCode, options);
	}

	public XCodeLocalizationData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		XCodeLocalizationData xCodeLocalizationData = new XCodeLocalizationData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				xCodeLocalizationData.Index = reader.ReadInt32();
				break;
			case 1:
				xCodeLocalizationData.Country = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 2:
				xCodeLocalizationData.CountryCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return xCodeLocalizationData;
	}
}

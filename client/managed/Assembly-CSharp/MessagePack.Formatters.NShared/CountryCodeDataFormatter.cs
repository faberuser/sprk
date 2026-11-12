using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class CountryCodeDataFormatter : IMessagePackFormatter<CountryCodeData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, CountryCodeData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(4);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Index, options);
		writer.Write(value.GroupIndex);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.CountryName, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.IconPath, options);
	}

	public CountryCodeData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		CountryCodeData countryCodeData = new CountryCodeData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				countryCodeData.Index = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 1:
				countryCodeData.GroupIndex = reader.ReadInt32();
				break;
			case 2:
				countryCodeData.CountryName = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 3:
				countryCodeData.IconPath = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return countryCodeData;
	}
}

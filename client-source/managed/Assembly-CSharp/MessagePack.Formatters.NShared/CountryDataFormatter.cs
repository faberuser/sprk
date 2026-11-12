using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class CountryDataFormatter : IMessagePackFormatter<CountryData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, CountryData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(7);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Code, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Name, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.Agreement, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.AgreementCondition, options);
		writer.Write(value.Version);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.PrivacyURL, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.TeamOfServiceURL, options);
	}

	public CountryData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		CountryData countryData = new CountryData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				countryData.Code = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 1:
				countryData.Name = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 2:
				countryData.Agreement = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 3:
				countryData.AgreementCondition = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 4:
				countryData.Version = reader.ReadInt32();
				break;
			case 5:
				countryData.PrivacyURL = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 6:
				countryData.TeamOfServiceURL = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return countryData;
	}
}

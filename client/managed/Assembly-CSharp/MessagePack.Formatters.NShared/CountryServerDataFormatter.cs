using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class CountryServerDataFormatter : IMessagePackFormatter<CountryServerData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, CountryServerData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(4);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ServerName, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.Law, options);
		writer.Write(value.IsPaidRubyPrint);
		writer.Write(value.IsAdultCheck);
	}

	public CountryServerData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		CountryServerData countryServerData = new CountryServerData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				countryServerData.ServerName = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 1:
				countryServerData.Law = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 2:
				countryServerData.IsPaidRubyPrint = reader.ReadBoolean();
				break;
			case 3:
				countryServerData.IsAdultCheck = reader.ReadBoolean();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return countryServerData;
	}
}

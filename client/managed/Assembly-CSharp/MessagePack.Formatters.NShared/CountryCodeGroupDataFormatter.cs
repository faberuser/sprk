using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class CountryCodeGroupDataFormatter : IMessagePackFormatter<CountryCodeGroupData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, CountryCodeGroupData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(2);
		writer.Write(value.Index);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.LocalTextPath, options);
	}

	public CountryCodeGroupData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		CountryCodeGroupData countryCodeGroupData = new CountryCodeGroupData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				countryCodeGroupData.Index = reader.ReadInt32();
				break;
			case 1:
				countryCodeGroupData.LocalTextPath = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return countryCodeGroupData;
	}
}

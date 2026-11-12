using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class LanguageDataFormatter : IMessagePackFormatter<LanguageData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, LanguageData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(1);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Language, options);
	}

	public LanguageData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		LanguageData languageData = new LanguageData();
		for (int i = 0; i < num; i++)
		{
			if (i == 0)
			{
				languageData.Language = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
			}
			else
			{
				reader.Skip();
			}
		}
		reader.Depth--;
		return languageData;
	}
}

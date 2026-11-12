using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class LanguagePatternDataFormatter : IMessagePackFormatter<LanguagePatternData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, LanguagePatternData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(3);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ServerName, options);
		writer.Write(value.Index);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.PatternRange, options);
	}

	public LanguagePatternData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		LanguagePatternData languagePatternData = new LanguagePatternData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				languagePatternData.ServerName = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 1:
				languagePatternData.Index = reader.ReadInt32();
				break;
			case 2:
				languagePatternData.PatternRange = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return languagePatternData;
	}
}

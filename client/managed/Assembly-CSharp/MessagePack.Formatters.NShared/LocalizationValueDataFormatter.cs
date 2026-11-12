using NShared;
using NShared.NLocalization;

namespace MessagePack.Formatters.NShared;

public sealed class LocalizationValueDataFormatter : IMessagePackFormatter<LocalizationValueData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, LocalizationValueData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(8);
		resolver.GetFormatterWithVerify<LanguageType>().Serialize(ref writer, value.Language, options);
		writer.Write(value.Open);
		writer.Write(value.Voice);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.CsEmail, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.AppName, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.PluginLanguageName, options);
		writer.Write(value.CountryFlagIndex);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.CultureInfoName, options);
	}

	public LocalizationValueData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		LocalizationValueData localizationValueData = new LocalizationValueData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				localizationValueData.Language = resolver.GetFormatterWithVerify<LanguageType>().Deserialize(ref reader, options);
				break;
			case 1:
				localizationValueData.Open = reader.ReadBoolean();
				break;
			case 2:
				localizationValueData.Voice = reader.ReadBoolean();
				break;
			case 3:
				localizationValueData.CsEmail = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 4:
				localizationValueData.AppName = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 5:
				localizationValueData.PluginLanguageName = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 6:
				localizationValueData.CountryFlagIndex = reader.ReadInt32();
				break;
			case 7:
				localizationValueData.CultureInfoName = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return localizationValueData;
	}
}

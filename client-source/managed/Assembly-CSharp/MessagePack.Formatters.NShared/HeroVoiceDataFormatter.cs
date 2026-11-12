using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class HeroVoiceDataFormatter : IMessagePackFormatter<HeroVoiceData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, HeroVoiceData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(3);
		writer.Write(value.HeroIndex);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.VoiceNameText, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.VoiceCode, options);
	}

	public HeroVoiceData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		HeroVoiceData heroVoiceData = new HeroVoiceData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				heroVoiceData.HeroIndex = reader.ReadInt32();
				break;
			case 1:
				heroVoiceData.VoiceNameText = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 2:
				heroVoiceData.VoiceCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return heroVoiceData;
	}
}

using System;
using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class HeroVoiceDataKeyFormatter : IMessagePackFormatter<HeroVoiceDataKey>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, HeroVoiceDataKey value, MessagePackSerializerOptions options)
	{
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(2);
		writer.Write(value.HeroIndex);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.VoiceCode, options);
	}

	public HeroVoiceDataKey Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			throw new InvalidOperationException("typecode is null, struct not supported");
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		int heroIndex = 0;
		string voiceCode = null;
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				heroIndex = reader.ReadInt32();
				break;
			case 1:
				voiceCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		HeroVoiceDataKey result = new HeroVoiceDataKey(heroIndex, voiceCode);
		reader.Depth--;
		return result;
	}
}

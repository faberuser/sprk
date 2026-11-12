using System;
using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class LanguagePatternKeyFormatter : IMessagePackFormatter<LanguagePatternKey>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, LanguagePatternKey value, MessagePackSerializerOptions options)
	{
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(2);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ServerName, options);
		writer.Write(value.Index);
	}

	public LanguagePatternKey Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			throw new InvalidOperationException("typecode is null, struct not supported");
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		string serverName = null;
		int index = 0;
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				serverName = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 1:
				index = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		LanguagePatternKey result = new LanguagePatternKey(serverName, index);
		reader.Depth--;
		return result;
	}
}

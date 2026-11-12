using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class CensoredWordDataFormatter : IMessagePackFormatter<CensoredWordData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, CensoredWordData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(5);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Word, options);
		writer.Write(value.AllowChat);
		writer.Write(value.MatchWholeWord);
		writer.Write(value.SimilarCheck);
		writer.Write(value.SensitiveWord);
	}

	public CensoredWordData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		CensoredWordData censoredWordData = new CensoredWordData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				censoredWordData.Word = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 1:
				censoredWordData.AllowChat = reader.ReadBoolean();
				break;
			case 2:
				censoredWordData.MatchWholeWord = reader.ReadBoolean();
				break;
			case 3:
				censoredWordData.SimilarCheck = reader.ReadBoolean();
				break;
			case 4:
				censoredWordData.SensitiveWord = reader.ReadBoolean();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return censoredWordData;
	}
}

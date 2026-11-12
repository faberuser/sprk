using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class SimilarAlphabetDataFormatter : IMessagePackFormatter<SimilarAlphabetData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, SimilarAlphabetData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(2);
		writer.Write(value.Alphabet);
		resolver.GetFormatterWithVerify<char[]>().Serialize(ref writer, value.SimilarAlphabet, options);
	}

	public SimilarAlphabetData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		SimilarAlphabetData similarAlphabetData = new SimilarAlphabetData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				similarAlphabetData.Alphabet = reader.ReadChar();
				break;
			case 1:
				similarAlphabetData.SimilarAlphabet = resolver.GetFormatterWithVerify<char[]>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return similarAlphabetData;
	}
}

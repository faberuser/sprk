using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class CSMailDataFormatter : IMessagePackFormatter<CSMailData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, CSMailData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(3);
		writer.Write(value.Index);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.CSMailTitle, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.CSMailDesc, options);
	}

	public CSMailData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		CSMailData cSMailData = new CSMailData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				cSMailData.Index = reader.ReadInt32();
				break;
			case 1:
				cSMailData.CSMailTitle = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 2:
				cSMailData.CSMailDesc = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return cSMailData;
	}
}

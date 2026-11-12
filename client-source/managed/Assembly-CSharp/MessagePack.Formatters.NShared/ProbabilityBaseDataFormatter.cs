using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class ProbabilityBaseDataFormatter : IMessagePackFormatter<ProbabilityBaseData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, ProbabilityBaseData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(5);
		writer.WriteNil();
		writer.Write(value.SubIndex);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ComponentTitle, options);
		writer.Write(value.ComponentCount);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ComponentRate, options);
	}

	public ProbabilityBaseData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		ProbabilityBaseData probabilityBaseData = new ProbabilityBaseData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 1:
				probabilityBaseData.SubIndex = reader.ReadInt32();
				break;
			case 2:
				probabilityBaseData.ComponentTitle = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 3:
				probabilityBaseData.ComponentCount = reader.ReadInt32();
				break;
			case 4:
				probabilityBaseData.ComponentRate = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return probabilityBaseData;
	}
}

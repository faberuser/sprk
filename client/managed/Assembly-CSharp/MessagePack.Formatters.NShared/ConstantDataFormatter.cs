using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class ConstantDataFormatter : IMessagePackFormatter<ConstantData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, ConstantData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(2);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Key, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Value, options);
	}

	public ConstantData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		ConstantData constantData = new ConstantData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				constantData.Key = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 1:
				constantData.Value = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return constantData;
	}
}

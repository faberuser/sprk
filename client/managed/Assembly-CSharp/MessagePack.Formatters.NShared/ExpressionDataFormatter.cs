using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class ExpressionDataFormatter : IMessagePackFormatter<ExpressionData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, ExpressionData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(3);
		writer.Write(value.Index);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Name, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Expression, options);
	}

	public ExpressionData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		ExpressionData expressionData = new ExpressionData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				expressionData.Index = reader.ReadInt32();
				break;
			case 1:
				expressionData.Name = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 2:
				expressionData.Expression = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return expressionData;
	}
}

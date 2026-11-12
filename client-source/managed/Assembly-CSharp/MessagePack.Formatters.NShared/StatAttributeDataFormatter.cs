using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class StatAttributeDataFormatter : IMessagePackFormatter<StatAttributeData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, StatAttributeData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(5);
		resolver.GetFormatterWithVerify<StatType>().Serialize(ref writer, value.StatType, options);
		writer.Write(value.Positive);
		writer.Write(value.Negative);
		writer.Write(value.Percent);
		writer.Write(value.AllowNegative);
	}

	public StatAttributeData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		StatAttributeData statAttributeData = new StatAttributeData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				statAttributeData.StatType = resolver.GetFormatterWithVerify<StatType>().Deserialize(ref reader, options);
				break;
			case 1:
				statAttributeData.Positive = reader.ReadBoolean();
				break;
			case 2:
				statAttributeData.Negative = reader.ReadBoolean();
				break;
			case 3:
				statAttributeData.Percent = reader.ReadBoolean();
				break;
			case 4:
				statAttributeData.AllowNegative = reader.ReadBoolean();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return statAttributeData;
	}
}

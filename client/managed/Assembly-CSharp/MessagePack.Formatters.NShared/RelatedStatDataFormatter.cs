using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class RelatedStatDataFormatter : IMessagePackFormatter<RelatedStatData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, RelatedStatData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(3);
		resolver.GetFormatterWithVerify<StatType>().Serialize(ref writer, value.CurrentStatType, options);
		resolver.GetFormatterWithVerify<StatType>().Serialize(ref writer, value.RelatedStatType, options);
		resolver.GetFormatterWithVerify<OperationType>().Serialize(ref writer, value.ChangedStatType, options);
	}

	public RelatedStatData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		RelatedStatData relatedStatData = new RelatedStatData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				relatedStatData.CurrentStatType = resolver.GetFormatterWithVerify<StatType>().Deserialize(ref reader, options);
				break;
			case 1:
				relatedStatData.RelatedStatType = resolver.GetFormatterWithVerify<StatType>().Deserialize(ref reader, options);
				break;
			case 2:
				relatedStatData.ChangedStatType = resolver.GetFormatterWithVerify<OperationType>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return relatedStatData;
	}
}

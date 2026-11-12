using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class UnderPrisonDataFormatter : IMessagePackFormatter<UnderPrisonData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, UnderPrisonData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(6);
		writer.Write(value.ChapterIndex);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Name, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Desc, options);
		resolver.GetFormatterWithVerify<UnderPrisonType>().Serialize(ref writer, value.Type, options);
		writer.Write(value.DailyEnterMaxCount);
		writer.Write(value.IsSelectReward);
	}

	public UnderPrisonData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		UnderPrisonData underPrisonData = new UnderPrisonData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				underPrisonData.ChapterIndex = reader.ReadInt32();
				break;
			case 1:
				underPrisonData.Name = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 2:
				underPrisonData.Desc = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 3:
				underPrisonData.Type = resolver.GetFormatterWithVerify<UnderPrisonType>().Deserialize(ref reader, options);
				break;
			case 4:
				underPrisonData.DailyEnterMaxCount = reader.ReadInt32();
				break;
			case 5:
				underPrisonData.IsSelectReward = reader.ReadBoolean();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return underPrisonData;
	}
}

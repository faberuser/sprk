using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class ThreatDataFormatter : IMessagePackFormatter<ThreatData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, ThreatData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(4);
		writer.Write(value.Index);
		resolver.GetFormatterWithVerify<CreatureTagType>().Serialize(ref writer, value.ClassType, options);
		resolver.GetFormatterWithVerify<ThreatType>().Serialize(ref writer, value.ThreatType, options);
		writer.Write(value.Multiplier);
	}

	public ThreatData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		ThreatData threatData = new ThreatData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				threatData.Index = reader.ReadInt32();
				break;
			case 1:
				threatData.ClassType = resolver.GetFormatterWithVerify<CreatureTagType>().Deserialize(ref reader, options);
				break;
			case 2:
				threatData.ThreatType = resolver.GetFormatterWithVerify<ThreatType>().Deserialize(ref reader, options);
				break;
			case 3:
				threatData.Multiplier = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return threatData;
	}
}

using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class FrameControlDataFormatter : IMessagePackFormatter<FrameControlData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, FrameControlData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(6);
		writer.Write(value.Index);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Name, options);
		resolver.GetFormatterWithVerify<EContentType>().Serialize(ref writer, value.ContentType, options);
		resolver.GetFormatterWithVerify<EDelayType>().Serialize(ref writer, value.DelayType, options);
		writer.Write(value.Auto);
		writer.Write(value.AutoPlus);
	}

	public FrameControlData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		FrameControlData frameControlData = new FrameControlData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				frameControlData.Index = reader.ReadInt32();
				break;
			case 1:
				frameControlData.Name = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 2:
				frameControlData.ContentType = resolver.GetFormatterWithVerify<EContentType>().Deserialize(ref reader, options);
				break;
			case 3:
				frameControlData.DelayType = resolver.GetFormatterWithVerify<EDelayType>().Deserialize(ref reader, options);
				break;
			case 4:
				frameControlData.Auto = reader.ReadInt32();
				break;
			case 5:
				frameControlData.AutoPlus = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return frameControlData;
	}
}

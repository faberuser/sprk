using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class SubStoryEventDataFormatter : IMessagePackFormatter<SubStoryEventData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, SubStoryEventData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(5);
		writer.Write(value.Index);
		writer.Write(value.Sequence);
		writer.Write(value.ReqIndex);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Command, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.Argument, options);
	}

	public SubStoryEventData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		SubStoryEventData subStoryEventData = new SubStoryEventData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				subStoryEventData.Index = reader.ReadInt32();
				break;
			case 1:
				subStoryEventData.Sequence = reader.ReadInt32();
				break;
			case 2:
				subStoryEventData.ReqIndex = reader.ReadInt32();
				break;
			case 3:
				subStoryEventData.Command = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 4:
				subStoryEventData.Argument = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return subStoryEventData;
	}
}

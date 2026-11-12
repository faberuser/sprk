using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class MainStoryDataFormatter : IMessagePackFormatter<MainStoryData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, MainStoryData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(8);
		writer.Write(value.ChapterIndex);
		writer.Write(value.StoryNo);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Name, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.NameParam, options);
		writer.WriteNil();
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.Condition, options);
		resolver.GetFormatterWithVerify<EventTriggerCommand>().Serialize(ref writer, value.Command, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.Argument, options);
	}

	public MainStoryData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		MainStoryData mainStoryData = new MainStoryData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				mainStoryData.ChapterIndex = reader.ReadInt32();
				break;
			case 1:
				mainStoryData.StoryNo = reader.ReadInt32();
				break;
			case 2:
				mainStoryData.Name = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 3:
				mainStoryData.NameParam = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 5:
				mainStoryData.Condition = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 6:
				mainStoryData.Command = resolver.GetFormatterWithVerify<EventTriggerCommand>().Deserialize(ref reader, options);
				break;
			case 7:
				mainStoryData.Argument = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return mainStoryData;
	}
}

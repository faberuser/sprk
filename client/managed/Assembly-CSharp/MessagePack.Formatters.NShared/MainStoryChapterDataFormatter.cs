using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class MainStoryChapterDataFormatter : IMessagePackFormatter<MainStoryChapterData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, MainStoryChapterData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(4);
		writer.Write(value.StoryIndex);
		writer.Write(value.ChapterIndex);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Name, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.BackgroundAssetPath, options);
	}

	public MainStoryChapterData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		MainStoryChapterData mainStoryChapterData = new MainStoryChapterData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				mainStoryChapterData.StoryIndex = reader.ReadInt32();
				break;
			case 1:
				mainStoryChapterData.ChapterIndex = reader.ReadInt32();
				break;
			case 2:
				mainStoryChapterData.Name = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 3:
				mainStoryChapterData.BackgroundAssetPath = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return mainStoryChapterData;
	}
}

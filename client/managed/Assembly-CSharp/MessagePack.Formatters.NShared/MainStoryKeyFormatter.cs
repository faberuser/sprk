using System;
using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class MainStoryKeyFormatter : IMessagePackFormatter<MainStoryKey>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, MainStoryKey value, MessagePackSerializerOptions options)
	{
		writer.WriteArrayHeader(2);
		writer.Write(value.ChapterIndex);
		writer.Write(value.StoryNo);
	}

	public MainStoryKey Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			throw new InvalidOperationException("typecode is null, struct not supported");
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		int chapterIndex = 0;
		int storyNo = 0;
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				chapterIndex = reader.ReadInt32();
				break;
			case 1:
				storyNo = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		MainStoryKey result = new MainStoryKey(chapterIndex, storyNo);
		reader.Depth--;
		return result;
	}
}

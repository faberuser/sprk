using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class SubStoryDataContainer_ChapterInfoFormatter : IMessagePackFormatter<SubStoryDataContainer.ChapterInfo>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, SubStoryDataContainer.ChapterInfo value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		writer.WriteArrayHeader(2);
		writer.Write(value.CreatureIndex);
		writer.Write(value.IsSubStoryOnly);
	}

	public SubStoryDataContainer.ChapterInfo Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		SubStoryDataContainer.ChapterInfo chapterInfo = new SubStoryDataContainer.ChapterInfo();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				chapterInfo.CreatureIndex = reader.ReadInt32();
				break;
			case 1:
				chapterInfo.IsSubStoryOnly = reader.ReadBoolean();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return chapterInfo;
	}
}

using System;
using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class EventDungeonGroupKeyFormatter : IMessagePackFormatter<EventDungeonGroupKey>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, EventDungeonGroupKey value, MessagePackSerializerOptions options)
	{
		writer.WriteArrayHeader(2);
		writer.Write(value.ChapterIndex);
		writer.Write(value.GroupIndex);
	}

	public EventDungeonGroupKey Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			throw new InvalidOperationException("typecode is null, struct not supported");
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		int chapterIndex = 0;
		int groupIndex = 0;
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				chapterIndex = reader.ReadInt32();
				break;
			case 1:
				groupIndex = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		EventDungeonGroupKey result = new EventDungeonGroupKey(chapterIndex, groupIndex);
		reader.Depth--;
		return result;
	}
}

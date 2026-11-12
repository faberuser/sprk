using System;
using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class SubQuestDataKeyFormatter : IMessagePackFormatter<SubQuestDataKey>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, SubQuestDataKey value, MessagePackSerializerOptions options)
	{
		writer.WriteArrayHeader(2);
		writer.Write(value.QuestIndex);
		writer.Write(value.Step);
	}

	public SubQuestDataKey Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			throw new InvalidOperationException("typecode is null, struct not supported");
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		int chapterIndex = 0;
		int dungeonIndex = 0;
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				chapterIndex = reader.ReadInt32();
				break;
			case 1:
				dungeonIndex = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		SubQuestDataKey result = new SubQuestDataKey(chapterIndex, dungeonIndex);
		reader.Depth--;
		return result;
	}
}

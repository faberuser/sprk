using System;
using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class UnderPrisonDungeonKeyFormatter : IMessagePackFormatter<UnderPrisonDungeonKey>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, UnderPrisonDungeonKey value, MessagePackSerializerOptions options)
	{
		writer.WriteArrayHeader(2);
		writer.Write(value.ChapterIndex);
		writer.Write(value.Diff);
	}

	public UnderPrisonDungeonKey Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			throw new InvalidOperationException("typecode is null, struct not supported");
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		int chapterIndex = 0;
		int diff = 0;
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				chapterIndex = reader.ReadInt32();
				break;
			case 1:
				diff = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		UnderPrisonDungeonKey result = new UnderPrisonDungeonKey(chapterIndex, diff);
		reader.Depth--;
		return result;
	}
}

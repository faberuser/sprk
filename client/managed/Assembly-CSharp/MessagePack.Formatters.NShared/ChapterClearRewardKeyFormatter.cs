using System;
using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class ChapterClearRewardKeyFormatter : IMessagePackFormatter<ChapterClearRewardKey>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, ChapterClearRewardKey value, MessagePackSerializerOptions options)
	{
		writer.WriteArrayHeader(2);
		writer.Write(value.Index);
		writer.Write(value.Step);
	}

	public ChapterClearRewardKey Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			throw new InvalidOperationException("typecode is null, struct not supported");
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		int index = 0;
		int step = 0;
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				index = reader.ReadInt32();
				break;
			case 1:
				step = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		ChapterClearRewardKey result = new ChapterClearRewardKey(index, step);
		reader.Depth--;
		return result;
	}
}

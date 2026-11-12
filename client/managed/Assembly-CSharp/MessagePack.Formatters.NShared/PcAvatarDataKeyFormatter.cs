using System;
using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class PcAvatarDataKeyFormatter : IMessagePackFormatter<PcAvatarDataKey>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, PcAvatarDataKey value, MessagePackSerializerOptions options)
	{
		writer.WriteArrayHeader(2);
		writer.Write(value.ChapterIndex);
		writer.Write(value.Index);
	}

	public PcAvatarDataKey Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			throw new InvalidOperationException("typecode is null, struct not supported");
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		int chapterIndex = 0;
		int index = 0;
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				chapterIndex = reader.ReadInt32();
				break;
			case 1:
				index = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		PcAvatarDataKey result = new PcAvatarDataKey(chapterIndex, index);
		reader.Depth--;
		return result;
	}
}

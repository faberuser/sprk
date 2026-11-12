using System;
using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class GuildRaidDungeonDataKeyFormatter : IMessagePackFormatter<GuildRaidDungeonDataKey>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, GuildRaidDungeonDataKey value, MessagePackSerializerOptions options)
	{
		writer.WriteArrayHeader(3);
		writer.Write(value.ChapterIndex);
		writer.Write(value.DungeonIndex);
		writer.Write(value.Step);
	}

	public GuildRaidDungeonDataKey Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			throw new InvalidOperationException("typecode is null, struct not supported");
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		int chapterIndex = 0;
		int dungeonIndex = 0;
		int step = 0;
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
			case 2:
				step = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		GuildRaidDungeonDataKey result = new GuildRaidDungeonDataKey(chapterIndex, dungeonIndex, step);
		reader.Depth--;
		return result;
	}
}

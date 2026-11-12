using System;
using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class GuildRaidDungeonDataContainer_GuildRaidStepKeyFormatter : IMessagePackFormatter<GuildRaidDungeonDataContainer.GuildRaidStepKey>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, GuildRaidDungeonDataContainer.GuildRaidStepKey value, MessagePackSerializerOptions options)
	{
		writer.WriteArrayHeader(2);
		writer.Write(value.ChapterIndex);
		writer.Write(value.Step);
	}

	public GuildRaidDungeonDataContainer.GuildRaidStepKey Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			throw new InvalidOperationException("typecode is null, struct not supported");
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		int chapterIndex = 0;
		int step = 0;
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				chapterIndex = reader.ReadInt32();
				break;
			case 1:
				step = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		GuildRaidDungeonDataContainer.GuildRaidStepKey result = new GuildRaidDungeonDataContainer.GuildRaidStepKey(chapterIndex, step);
		reader.Depth--;
		return result;
	}
}

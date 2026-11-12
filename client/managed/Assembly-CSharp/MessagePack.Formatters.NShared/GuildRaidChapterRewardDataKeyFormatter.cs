using System;
using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class GuildRaidChapterRewardDataKeyFormatter : IMessagePackFormatter<GuildRaidChapterRewardDataKey>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, GuildRaidChapterRewardDataKey value, MessagePackSerializerOptions options)
	{
		writer.WriteArrayHeader(3);
		writer.Write(value.ChapterIndex);
		writer.Write(value.Difficulty);
		writer.Write(value.TotalDamage);
	}

	public GuildRaidChapterRewardDataKey Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			throw new InvalidOperationException("typecode is null, struct not supported");
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		int chapterIndex = 0;
		int difficulty = 0;
		long totalDamage = 0L;
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				chapterIndex = reader.ReadInt32();
				break;
			case 1:
				difficulty = reader.ReadInt32();
				break;
			case 2:
				totalDamage = reader.ReadInt64();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		GuildRaidChapterRewardDataKey result = new GuildRaidChapterRewardDataKey(chapterIndex, difficulty, totalDamage);
		reader.Depth--;
		return result;
	}
}

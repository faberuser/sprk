using System;
using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class WorldBossDailyAchievemenDataKeyFormatter : IMessagePackFormatter<WorldBossDailyAchievemenDataKey>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, WorldBossDailyAchievemenDataKey value, MessagePackSerializerOptions options)
	{
		writer.WriteArrayHeader(2);
		writer.Write(value.WorldBossIndex);
		writer.Write(value.Index);
	}

	public WorldBossDailyAchievemenDataKey Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			throw new InvalidOperationException("typecode is null, struct not supported");
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		int worldBossIndex = 0;
		int index = 0;
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				worldBossIndex = reader.ReadInt32();
				break;
			case 1:
				index = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		WorldBossDailyAchievemenDataKey result = new WorldBossDailyAchievemenDataKey(worldBossIndex, index);
		reader.Depth--;
		return result;
	}
}

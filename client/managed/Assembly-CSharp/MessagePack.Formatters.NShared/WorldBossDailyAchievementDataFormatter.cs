using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class WorldBossDailyAchievementDataFormatter : IMessagePackFormatter<WorldBossDailyAchievementData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, WorldBossDailyAchievementData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		writer.WriteArrayHeader(6);
		writer.Write(value.WorldBossIndex);
		writer.Write(value.Index);
		writer.Write(value.MinScore);
		writer.Write(value.MaxScore);
		writer.Write(value.WorldBossPoint);
		writer.Write(value.RewardIndex);
	}

	public WorldBossDailyAchievementData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		WorldBossDailyAchievementData worldBossDailyAchievementData = new WorldBossDailyAchievementData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				worldBossDailyAchievementData.WorldBossIndex = reader.ReadInt32();
				break;
			case 1:
				worldBossDailyAchievementData.Index = reader.ReadInt32();
				break;
			case 2:
				worldBossDailyAchievementData.MinScore = reader.ReadInt64();
				break;
			case 3:
				worldBossDailyAchievementData.MaxScore = reader.ReadInt64();
				break;
			case 4:
				worldBossDailyAchievementData.WorldBossPoint = reader.ReadInt32();
				break;
			case 5:
				worldBossDailyAchievementData.RewardIndex = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return worldBossDailyAchievementData;
	}
}

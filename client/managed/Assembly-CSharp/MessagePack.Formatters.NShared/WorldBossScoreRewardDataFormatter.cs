using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class WorldBossScoreRewardDataFormatter : IMessagePackFormatter<WorldBossScoreRewardData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, WorldBossScoreRewardData value, MessagePackSerializerOptions options)
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
		writer.Write(value.RewardIndex);
		writer.Write(value.WorldBossPoint);
	}

	public WorldBossScoreRewardData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		WorldBossScoreRewardData worldBossScoreRewardData = new WorldBossScoreRewardData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				worldBossScoreRewardData.WorldBossIndex = reader.ReadInt32();
				break;
			case 1:
				worldBossScoreRewardData.Index = reader.ReadInt32();
				break;
			case 2:
				worldBossScoreRewardData.MinScore = reader.ReadInt64();
				break;
			case 3:
				worldBossScoreRewardData.MaxScore = reader.ReadInt64();
				break;
			case 4:
				worldBossScoreRewardData.RewardIndex = reader.ReadInt32();
				break;
			case 5:
				worldBossScoreRewardData.WorldBossPoint = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return worldBossScoreRewardData;
	}
}

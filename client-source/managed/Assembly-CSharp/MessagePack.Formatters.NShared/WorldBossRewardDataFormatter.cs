using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class WorldBossRewardDataFormatter : IMessagePackFormatter<WorldBossRewardData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, WorldBossRewardData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(8);
		writer.Write(value.Index);
		writer.Write(value.MinRank);
		writer.Write(value.MaxRank);
		writer.Write(value.Percentage);
		writer.Write(value.RewardIndex);
		resolver.GetFormatterWithVerify<WorldBossServerRewardType>().Serialize(ref writer, value.RewardServerBuffType, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.RewardServerBuffValue, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.RewardServerBuffCode, options);
	}

	public WorldBossRewardData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		WorldBossRewardData worldBossRewardData = new WorldBossRewardData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				worldBossRewardData.Index = reader.ReadInt32();
				break;
			case 1:
				worldBossRewardData.MinRank = reader.ReadInt32();
				break;
			case 2:
				worldBossRewardData.MaxRank = reader.ReadInt32();
				break;
			case 3:
				worldBossRewardData.Percentage = reader.ReadInt32();
				break;
			case 4:
				worldBossRewardData.RewardIndex = reader.ReadInt32();
				break;
			case 5:
				worldBossRewardData.RewardServerBuffType = resolver.GetFormatterWithVerify<WorldBossServerRewardType>().Deserialize(ref reader, options);
				break;
			case 6:
				worldBossRewardData.RewardServerBuffValue = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 7:
				worldBossRewardData.RewardServerBuffCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return worldBossRewardData;
	}
}

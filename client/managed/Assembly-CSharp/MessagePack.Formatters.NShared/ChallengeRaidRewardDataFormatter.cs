using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class ChallengeRaidRewardDataFormatter : IMessagePackFormatter<ChallengeRaidRewardData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, ChallengeRaidRewardData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		writer.WriteArrayHeader(6);
		writer.Write(value.Index);
		writer.Write(value.ChallengeRaidIndex);
		writer.Write(value.MinRank);
		writer.Write(value.MaxRank);
		writer.Write(value.Percentage);
		writer.Write(value.RewardIndex);
	}

	public ChallengeRaidRewardData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		ChallengeRaidRewardData challengeRaidRewardData = new ChallengeRaidRewardData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				challengeRaidRewardData.Index = reader.ReadInt32();
				break;
			case 1:
				challengeRaidRewardData.ChallengeRaidIndex = reader.ReadInt32();
				break;
			case 2:
				challengeRaidRewardData.MinRank = reader.ReadInt32();
				break;
			case 3:
				challengeRaidRewardData.MaxRank = reader.ReadInt32();
				break;
			case 4:
				challengeRaidRewardData.Percentage = reader.ReadInt32();
				break;
			case 5:
				challengeRaidRewardData.RewardIndex = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return challengeRaidRewardData;
	}
}

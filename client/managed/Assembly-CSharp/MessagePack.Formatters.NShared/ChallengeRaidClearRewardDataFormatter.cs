using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class ChallengeRaidClearRewardDataFormatter : IMessagePackFormatter<ChallengeRaidClearRewardData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, ChallengeRaidClearRewardData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(13);
		writer.Write(value.ChallengeRaidIndex);
		writer.Write(value.AchievementIndex);
		writer.Write(value.RaidLevel);
		writer.Write(value.ClearCount);
		resolver.GetFormatterWithVerify<AchievementRewardType>().Serialize(ref writer, value.RewardType, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.RewardCode, options);
		writer.Write(value.RewardValue);
		resolver.GetFormatterWithVerify<AchievementRewardType>().Serialize(ref writer, value.Reward2Type, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Reward2Code, options);
		writer.Write(value.Reward2Value);
		resolver.GetFormatterWithVerify<AchievementRewardType>().Serialize(ref writer, value.Reward3Type, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Reward3Code, options);
		writer.Write(value.Reward3Value);
	}

	public ChallengeRaidClearRewardData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		ChallengeRaidClearRewardData challengeRaidClearRewardData = new ChallengeRaidClearRewardData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				challengeRaidClearRewardData.ChallengeRaidIndex = reader.ReadInt32();
				break;
			case 1:
				challengeRaidClearRewardData.AchievementIndex = reader.ReadInt32();
				break;
			case 2:
				challengeRaidClearRewardData.RaidLevel = reader.ReadInt32();
				break;
			case 3:
				challengeRaidClearRewardData.ClearCount = reader.ReadInt32();
				break;
			case 4:
				challengeRaidClearRewardData.RewardType = resolver.GetFormatterWithVerify<AchievementRewardType>().Deserialize(ref reader, options);
				break;
			case 5:
				challengeRaidClearRewardData.RewardCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 6:
				challengeRaidClearRewardData.RewardValue = reader.ReadInt32();
				break;
			case 7:
				challengeRaidClearRewardData.Reward2Type = resolver.GetFormatterWithVerify<AchievementRewardType>().Deserialize(ref reader, options);
				break;
			case 8:
				challengeRaidClearRewardData.Reward2Code = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 9:
				challengeRaidClearRewardData.Reward2Value = reader.ReadInt32();
				break;
			case 10:
				challengeRaidClearRewardData.Reward3Type = resolver.GetFormatterWithVerify<AchievementRewardType>().Deserialize(ref reader, options);
				break;
			case 11:
				challengeRaidClearRewardData.Reward3Code = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 12:
				challengeRaidClearRewardData.Reward3Value = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return challengeRaidClearRewardData;
	}
}

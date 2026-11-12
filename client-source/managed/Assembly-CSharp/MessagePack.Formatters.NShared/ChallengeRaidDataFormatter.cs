using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class ChallengeRaidDataFormatter : IMessagePackFormatter<ChallengeRaidData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, ChallengeRaidData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(11);
		writer.Write(value.Index);
		writer.Write(value.RaidIndex);
		writer.Write(value.ShopIndex);
		resolver.GetFormatterWithVerify<RaidRewardType>().Serialize(ref writer, value.RewardType, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.RaidSelectImage, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.RankingBossImage, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.GuideBattle, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.GuideRanking, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.RoomTitle, options);
		writer.Write(value.ShowEnterButtonEventImage);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.PortalImagePath, options);
	}

	public ChallengeRaidData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		ChallengeRaidData challengeRaidData = new ChallengeRaidData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				challengeRaidData.Index = reader.ReadInt32();
				break;
			case 1:
				challengeRaidData.RaidIndex = reader.ReadInt32();
				break;
			case 2:
				challengeRaidData.ShopIndex = reader.ReadInt32();
				break;
			case 3:
				challengeRaidData.RewardType = resolver.GetFormatterWithVerify<RaidRewardType>().Deserialize(ref reader, options);
				break;
			case 4:
				challengeRaidData.RaidSelectImage = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 5:
				challengeRaidData.RankingBossImage = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 6:
				challengeRaidData.GuideBattle = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 7:
				challengeRaidData.GuideRanking = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 8:
				challengeRaidData.RoomTitle = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 9:
				challengeRaidData.ShowEnterButtonEventImage = reader.ReadBoolean();
				break;
			case 10:
				challengeRaidData.PortalImagePath = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return challengeRaidData;
	}
}

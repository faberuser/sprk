using System;
using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class ChallengeRaidClearRewardKeyFormatter : IMessagePackFormatter<ChallengeRaidClearRewardKey>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, ChallengeRaidClearRewardKey value, MessagePackSerializerOptions options)
	{
		writer.WriteArrayHeader(2);
		writer.Write(value.ChallengeRaidIndex);
		writer.Write(value.AchievementIndex);
	}

	public ChallengeRaidClearRewardKey Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			throw new InvalidOperationException("typecode is null, struct not supported");
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		int challengeRaidIndex = 0;
		int achievementIndex = 0;
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				challengeRaidIndex = reader.ReadInt32();
				break;
			case 1:
				achievementIndex = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		ChallengeRaidClearRewardKey result = new ChallengeRaidClearRewardKey(challengeRaidIndex, achievementIndex);
		reader.Depth--;
		return result;
	}
}

using MessagePack;
using NShared.NJit;

namespace NShared;

[MessagePackObject(false)]
[JitKey]
public struct ChallengeRaidClearRewardKey
{
	[Key(0)]
	public int ChallengeRaidIndex;

	[Key(1)]
	public int AchievementIndex;

	public ChallengeRaidClearRewardKey(int challengeRaidIndex, int achievementIndex)
	{
		ChallengeRaidIndex = challengeRaidIndex;
		AchievementIndex = achievementIndex;
	}

	public bool Equals(ChallengeRaidClearRewardKey other)
	{
		if (ChallengeRaidIndex == other.ChallengeRaidIndex)
		{
			return AchievementIndex == other.AchievementIndex;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return JitOption.BuildHash(ChallengeRaidIndex, AchievementIndex);
	}

	public override string ToString()
	{
		return $"{GetType().Name}({ChallengeRaidIndex}, {AchievementIndex})";
	}
}

using MessagePack;
using NShared.NJit;

namespace NShared;

[MessagePackObject(false)]
[JitKey]
public struct NewbieRewardBannerUrlKey
{
	[Key(0)]
	public NewbieRewardType RewardType;

	[Key(1)]
	public int ShowIndex;

	[Key(2)]
	public string Language;

	public NewbieRewardBannerUrlKey(NewbieRewardType rewardType, int showIndex, string language)
	{
		RewardType = rewardType;
		ShowIndex = showIndex;
		Language = language;
	}

	public bool Equals(NewbieRewardBannerUrlKey other)
	{
		if (RewardType == other.RewardType && ShowIndex == other.ShowIndex)
		{
			return Language == other.Language;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return JitOption.BuildHash(RewardType, ShowIndex, Language);
	}

	public override string ToString()
	{
		return $"{GetType().Name}({RewardType}, {ShowIndex}, {Language})";
	}
}

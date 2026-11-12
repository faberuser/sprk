using MessagePack;
using NShared.NJit;

namespace NShared;

[MessagePackObject(false)]
[JitKey]
public struct KarmaRewardKey
{
	[Key(0)]
	public int PunishmentGroupIndex;

	[Key(1)]
	public int RewardIndex;

	public KarmaRewardKey(int punishmentGroupIndex, int rewardIndex)
	{
		PunishmentGroupIndex = punishmentGroupIndex;
		RewardIndex = rewardIndex;
	}

	public bool Equals(KarmaRewardKey other)
	{
		if (PunishmentGroupIndex == other.PunishmentGroupIndex)
		{
			return RewardIndex == other.RewardIndex;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return JitOption.BuildHash(PunishmentGroupIndex, RewardIndex);
	}

	public override string ToString()
	{
		return $"{GetType().Name}({PunishmentGroupIndex}, {RewardIndex})";
	}
}

using MessagePack;
using NShared.NJit;

namespace NShared;

[MessagePackObject(false)]
[JitKey]
public struct KarmaRewardExponentialKey
{
	[Key(0)]
	public int PunishmentGroupIndex;

	[Key(1)]
	public int Level;

	public KarmaRewardExponentialKey(int punishmentGroupIndex, int level)
	{
		PunishmentGroupIndex = punishmentGroupIndex;
		Level = level;
	}

	public bool Equals(KarmaRewardExponentialKey other)
	{
		if (PunishmentGroupIndex == other.PunishmentGroupIndex)
		{
			return Level == other.Level;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return JitOption.BuildHash(PunishmentGroupIndex, Level);
	}

	public override string ToString()
	{
		return $"{GetType().Name}({PunishmentGroupIndex}, {Level})";
	}
}

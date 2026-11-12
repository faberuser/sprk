using MessagePack;
using NShared.NJit;

namespace NShared;

[MessagePackObject(false)]
[JitKey]
public struct KarmaGaugeExponentialKey
{
	[Key(0)]
	public int PunishmentGroupIndex;

	[Key(1)]
	public int Wave;

	public KarmaGaugeExponentialKey(int punishmentGroupIndex, int wave)
	{
		PunishmentGroupIndex = punishmentGroupIndex;
		Wave = wave;
	}

	public bool Equals(KarmaGaugeExponentialKey other)
	{
		if (PunishmentGroupIndex == other.PunishmentGroupIndex)
		{
			return Wave == other.Wave;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return JitOption.BuildHash(PunishmentGroupIndex, Wave);
	}

	public override string ToString()
	{
		return $"{GetType().Name}({PunishmentGroupIndex}, {Wave})";
	}
}

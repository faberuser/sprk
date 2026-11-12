using MessagePack;
using NShared.NJit;

namespace NShared;

[MessagePackObject(false)]
[JitKey]
public struct KarmaDungeonDataKey
{
	[Key(0)]
	public int PunishmentGroupIndex;

	[Key(1)]
	public int StageLevel;

	public KarmaDungeonDataKey(int punishmentGroupIndex, int stageLevel)
	{
		PunishmentGroupIndex = punishmentGroupIndex;
		StageLevel = stageLevel;
	}

	public bool Equals(KarmaDungeonDataKey other)
	{
		if (PunishmentGroupIndex == other.PunishmentGroupIndex)
		{
			return StageLevel == other.StageLevel;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return JitOption.BuildHash(PunishmentGroupIndex, StageLevel);
	}

	public override string ToString()
	{
		return $"{GetType().Name}({PunishmentGroupIndex}, {StageLevel})";
	}
}

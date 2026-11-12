using MessagePack;
using NShared.NJit;

namespace NShared;

[MessagePackObject(false)]
[JitKey]
public struct KingsPassMissionKey2
{
	[Key(0)]
	public int SeasonIndex;

	[Key(1)]
	public int MissionIndex;

	public KingsPassMissionKey2(int seasonIndex, int missionIndex)
	{
		SeasonIndex = seasonIndex;
		MissionIndex = missionIndex;
	}

	public bool Equals(KingsPassMissionKey2 other)
	{
		if (SeasonIndex == other.SeasonIndex)
		{
			return MissionIndex == other.MissionIndex;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return JitOption.BuildHash(SeasonIndex, MissionIndex);
	}

	public override string ToString()
	{
		return $"SeasonIndex={SeasonIndex} MissionIndex={MissionIndex}";
	}
}

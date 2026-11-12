using MessagePack;
using NShared.NJit;

namespace NShared;

[MessagePackObject(false)]
[JitKey]
public struct WorldBossDailyAchievemenDataKey
{
	[Key(0)]
	public int WorldBossIndex;

	[Key(1)]
	public int Index;

	public WorldBossDailyAchievemenDataKey(int worldBossIndex, int index)
	{
		WorldBossIndex = worldBossIndex;
		Index = index;
	}

	public bool Equals(WorldBossDailyAchievemenDataKey other)
	{
		if (WorldBossIndex == other.WorldBossIndex)
		{
			return Index == other.Index;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return JitOption.BuildHash(WorldBossIndex, Index);
	}

	public override string ToString()
	{
		return $"{GetType().Name}({WorldBossIndex}, {Index})";
	}
}

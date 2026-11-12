using MessagePack;
using NShared.NJit;

namespace NShared;

[MessagePackObject(false)]
[JitKey]
public struct EventWorldBossDataKey
{
	[Key(0)]
	public int Index;

	[Key(1)]
	public int BossLevel;

	public EventWorldBossDataKey(int index, int bossLevel)
	{
		Index = index;
		BossLevel = bossLevel;
	}

	public bool Equals(EventWorldBossDataKey other)
	{
		if (Index == other.Index)
		{
			return BossLevel == other.BossLevel;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return JitOption.BuildHash(Index, BossLevel);
	}

	public override string ToString()
	{
		return $"{GetType().Name}({Index}, {BossLevel})";
	}
}

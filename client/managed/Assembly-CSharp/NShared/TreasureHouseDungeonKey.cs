using MessagePack;
using NShared.NJit;

namespace NShared;

[MessagePackObject(false)]
[JitKey]
public struct TreasureHouseDungeonKey
{
	[Key(0)]
	public int InfoIndex;

	[Key(1)]
	public int ShowIndex;

	public TreasureHouseDungeonKey(int infoIndex, int showIndex)
	{
		InfoIndex = infoIndex;
		ShowIndex = showIndex;
	}

	public bool Equals(TreasureHouseDungeonKey other)
	{
		if (InfoIndex == other.InfoIndex)
		{
			return ShowIndex == other.ShowIndex;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return JitOption.BuildHash(InfoIndex, ShowIndex);
	}

	public override string ToString()
	{
		return $"{GetType().Name}({InfoIndex}, {ShowIndex})";
	}
}

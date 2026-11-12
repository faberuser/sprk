using MessagePack;
using NShared.NJit;

namespace NShared;

[MessagePackObject(false)]
[JitKey]
public struct TowerFloorKey
{
	[Key(0)]
	public int Index;

	[Key(1)]
	public int Floor;

	public TowerFloorKey(int index, int floor)
	{
		Index = index;
		Floor = floor;
	}

	public bool Equals(TowerFloorKey other)
	{
		if (Index == other.Index)
		{
			return Floor == other.Floor;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return JitOption.BuildHash(Index, Floor);
	}

	public override string ToString()
	{
		return $"{GetType().Name}({Index}, {Floor})";
	}
}

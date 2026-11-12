using MessagePack;
using NShared.NJit;

namespace NShared;

[MessagePackObject(false)]
[JitKey]
public struct DOWDungeonTextKey
{
	[Key(0)]
	public int DungeonIndex;

	[Key(1)]
	public int Diff;

	public DOWDungeonTextKey(int dungeonIndex, int diff)
	{
		DungeonIndex = dungeonIndex;
		Diff = diff;
	}

	public bool Equals(DOWDungeonTextKey other)
	{
		if (DungeonIndex == other.DungeonIndex)
		{
			return Diff == other.Diff;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return JitOption.BuildHash(DungeonIndex, Diff);
	}

	public override string ToString()
	{
		return $"{GetType().Name}({DungeonIndex}, {Diff})";
	}
}

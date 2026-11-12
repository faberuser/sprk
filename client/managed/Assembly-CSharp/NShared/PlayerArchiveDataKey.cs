using MessagePack;
using NShared.NJit;

namespace NShared;

[MessagePackObject(false)]
[JitKey]
public struct PlayerArchiveDataKey
{
	[Key(0)]
	public int MainIndex;

	[Key(1)]
	public int SubIndex;

	public PlayerArchiveDataKey(int mainIndex, int subIndex)
	{
		MainIndex = mainIndex;
		SubIndex = subIndex;
	}

	public bool Equals(PlayerArchiveDataKey other)
	{
		if (MainIndex == other.MainIndex)
		{
			return SubIndex == other.SubIndex;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return JitOption.BuildHash(MainIndex, SubIndex);
	}

	public override string ToString()
	{
		return $"{GetType().Name}({MainIndex}, {SubIndex})";
	}
}

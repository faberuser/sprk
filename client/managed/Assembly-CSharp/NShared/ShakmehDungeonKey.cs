using MessagePack;
using NShared.NJit;

namespace NShared;

[MessagePackObject(false)]
[JitKey]
public struct ShakmehDungeonKey
{
	[Key(0)]
	public int ShakmehIndex;

	[Key(1)]
	public int Level;

	public ShakmehDungeonKey(int shakmehIndex, int level)
	{
		ShakmehIndex = shakmehIndex;
		Level = level;
	}

	public bool Equals(ShakmehDungeonKey other)
	{
		if (ShakmehIndex == other.ShakmehIndex)
		{
			return Level == other.Level;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return JitOption.BuildHash(ShakmehIndex, Level);
	}

	public override string ToString()
	{
		return $"{GetType().Name}({ShakmehIndex}, {Level})";
	}
}

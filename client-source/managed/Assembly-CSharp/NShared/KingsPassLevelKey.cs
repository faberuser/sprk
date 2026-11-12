using MessagePack;
using NShared.NJit;

namespace NShared;

[MessagePackObject(false)]
[JitKey]
public struct KingsPassLevelKey
{
	[Key(0)]
	public int LevelIndex;

	[Key(1)]
	public int Level;

	public KingsPassLevelKey(int levelIndex, int level)
	{
		LevelIndex = levelIndex;
		Level = level;
	}

	public bool Equals(KingsPassLevelKey other)
	{
		if (LevelIndex == other.LevelIndex)
		{
			return Level == other.Level;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return JitOption.BuildHash(LevelIndex, Level);
	}

	public override string ToString()
	{
		return $"{GetType().Name}({LevelIndex}, {Level})";
	}
}

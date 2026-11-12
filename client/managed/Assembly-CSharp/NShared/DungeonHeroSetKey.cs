using MessagePack;
using NShared.NJit;

namespace NShared;

[MessagePackObject(false)]
[JitKey]
public struct DungeonHeroSetKey
{
	[Key(0)]
	public int ChapterIndex;

	[Key(1)]
	public int DungeonIndex;

	[Key(2)]
	public DungeonDifficultyType Difficulty;

	public DungeonHeroSetKey(int chapterIndex, int dungeonIndex, DungeonDifficultyType difficulty)
	{
		ChapterIndex = chapterIndex;
		DungeonIndex = dungeonIndex;
		Difficulty = difficulty;
	}

	public bool Equals(DungeonHeroSetKey other)
	{
		if (ChapterIndex == other.ChapterIndex && DungeonIndex == other.DungeonIndex)
		{
			return Difficulty == other.Difficulty;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return JitOption.BuildHash(ChapterIndex, DungeonIndex, Difficulty);
	}

	public override string ToString()
	{
		return $"{GetType().Name}({ChapterIndex}, {DungeonIndex}, {Difficulty})";
	}
}

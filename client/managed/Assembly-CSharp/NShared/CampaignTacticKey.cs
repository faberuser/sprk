using MessagePack;
using NShared.NJit;

namespace NShared;

[MessagePackObject(false)]
[JitKey]
public struct CampaignTacticKey
{
	[Key(0)]
	public int ChapterIndex;

	[Key(1)]
	public int DungeonIndex;

	[Key(2)]
	public DungeonDifficultyType Difficulty;

	public CampaignTacticKey(int chapterIndex, int dungeonIndex, DungeonDifficultyType difficulty)
	{
		ChapterIndex = chapterIndex;
		DungeonIndex = dungeonIndex;
		Difficulty = difficulty;
	}

	public bool Equals(CampaignTacticKey other)
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
		return $"ChapterIndex={ChapterIndex} DungeonIndex={DungeonIndex} Difficulty={Difficulty}";
	}
}

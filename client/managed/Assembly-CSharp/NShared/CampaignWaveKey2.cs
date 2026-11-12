using NShared.NJit;

namespace NShared;

[JitKey]
public struct CampaignWaveKey2
{
	public int ChapterIndex;

	public int DungeonIndex;

	public DungeonDifficultyType Difficulty;

	public bool Scenario;

	public int WaveIndex;

	public CampaignWaveKey2(int chapterIndex, int dungeonIndex, DungeonDifficultyType difficulty, bool scenario, int waveIndex)
	{
		ChapterIndex = chapterIndex;
		DungeonIndex = dungeonIndex;
		Difficulty = difficulty;
		Scenario = scenario;
		WaveIndex = waveIndex;
	}

	public bool Equals(CampaignWaveKey2 other)
	{
		if (ChapterIndex == other.ChapterIndex && DungeonIndex == other.DungeonIndex && Difficulty == other.Difficulty && Scenario == other.Scenario)
		{
			return WaveIndex == other.WaveIndex;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return JitOption.BuildHash(ChapterIndex, DungeonIndex, Difficulty, Scenario, WaveIndex);
	}
}

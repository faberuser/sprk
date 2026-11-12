namespace NShared;

public class CampaignWaveKey : Tuple<int, int, DungeonDifficultyType, bool, int>
{
	public int ChapterIndex => Item1;

	public int DungeonIndex => Item2;

	public DungeonDifficultyType Difficulty => Item3;

	public bool Scenario => Item4;

	public int WaveIndex => Item5;

	public CampaignWaveKey()
		: base(0, 0, DungeonDifficultyType.Easy, i_item4: false, 0)
	{
	}

	public CampaignWaveKey(int campaignIndex, int dungeonIndex, DungeonDifficultyType difficulty, bool scenario, int waveIndex)
		: base(campaignIndex, dungeonIndex, difficulty, scenario, waveIndex)
	{
	}
}

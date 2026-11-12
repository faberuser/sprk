using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class CampaignTacticData : IDataKeySelector<CampaignTacticKey>
{
	CampaignTacticKey IDataKeySelector<CampaignTacticKey>.Key => new CampaignTacticKey(ChapterIndex, DungeonIndex, Difficulty);

	[Key(0)]
	public int ChapterIndex { get; set; }

	[Key(1)]
	public int DungeonIndex { get; set; }

	[Key(2)]
	public DungeonDifficultyType Difficulty { get; set; }

	[Key(3)]
	public bool ScenarioOnly { get; set; }

	[Key(4)]
	public BattleTacticType TacticType { get; set; }

	[Key(5)]
	public string ComponentName { get; set; }

	[Key(6)]
	public string[] TacticValue { get; set; }

	[Key(7)]
	public bool BossTactic { get; set; }
}

namespace NShared;

[JsonSerializable(Javascript = true)]
public class ChapterDungeonInfo : IAccountAttrKey<ChapterDungeonKey>
{
	ChapterDungeonKey IAccountAttrKey<ChapterDungeonKey>.Key => new ChapterDungeonKey(ChapterIndex, DungeonIndex);

	public int ChapterIndex { get; set; }

	public int DungeonIndex { get; set; }

	public short MaxStar { get; set; }

	public short FirstRewardedDiff { get; set; }

	public short ScenarioComplete { get; set; }

	public string VisitedTime { get; set; }

	public string CompletedTime { get; set; }

	public int DailyCompletedCount { get; set; }

	public int ResetCount { get; set; }
}

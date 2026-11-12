namespace NShared;

[JsonSerializable(Javascript = true)]
public class HideoutDungeonInfo : IAccountAttrKey<int>
{
	int IAccountAttrKey<int>.Key => ChapterIndex;

	public int ChapterIndex { get; set; }

	public string CompletedTime { get; set; }

	public int DailyCompletedCount { get; set; }

	public int ResetCount { get; set; }

	[NonDbSchema]
	public int NextResetRemainTime { get; set; }
}

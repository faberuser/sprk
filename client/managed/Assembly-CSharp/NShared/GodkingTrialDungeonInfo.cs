namespace NShared;

[JsonSerializable(Javascript = true)]
public class GodkingTrialDungeonInfo : IAccountAttrKey<int>
{
	int IAccountAttrKey<int>.Key => ChapterIndex;

	public int ChapterIndex { get; set; }

	public int IsOpen { get; set; }

	public string OpenedTime { get; set; }

	[NonDbSchema]
	public int NextResetRemainTime { get; set; }

	[NonDbSchema]
	public bool IsOpened => IsOpen == 1;
}

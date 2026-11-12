namespace NShared;

[JsonSerializable(Javascript = true)]
public class UnderPrisonInfo : IAccountAttrKey<int>
{
	int IAccountAttrKey<int>.Key => ChapterIndex;

	public int ChapterIndex { get; set; }

	public string LastCompletedTime { get; set; }

	public int CompletedCount { get; set; }

	public int Diff { get; set; }

	[NonDbSchema]
	public int MaxTryCount { get; set; }
}

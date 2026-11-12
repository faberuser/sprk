namespace NShared;

[JsonSerializable(Javascript = true)]
public class EventStepInfo : IAccountAttrKey<int>
{
	int IAccountAttrKey<int>.Key => TotalCount;

	public int DailyStep { get; set; }

	public int DailyCount { get; set; }

	public int TotalCount { get; set; }

	public int MissionCount { get; set; }

	public int MissionStep { get; set; }

	public int GroupTotalCount { get; set; }

	public int GroupTotalStep { get; set; }

	public int GroupCurrentStep { get; set; }

	public int GroupTargetScore { get; set; }

	public int GroupRemainScore { get; set; }
}

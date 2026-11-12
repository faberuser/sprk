namespace NShared;

[JsonSerializable(Javascript = true)]
public class PushRewardInfo
{
	public int Index { get; set; }

	public int DayOfTheWeek { get; set; }

	public int OpenTime { get; set; }

	public int EndTime { get; set; }

	public string TouchAction { get; set; }

	public int RewardIndex { get; set; }

	public string NameKey { get; set; }
}

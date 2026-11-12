namespace NShared;

[JsonSerializable(Javascript = true)]
public class DbRewardData
{
	public RewardType Type { get; set; }

	public int Value { get; set; }

	public int Count { get; set; }

	public int Star { get; set; }
}

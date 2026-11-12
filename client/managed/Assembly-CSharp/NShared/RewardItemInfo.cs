namespace NShared;

[JsonSerializable(Javascript = true)]
public class RewardItemInfo
{
	public string Code { get; set; }

	public int Count { get; set; }

	public int Star { get; set; }

	public bool IsBonus { get; set; }
}

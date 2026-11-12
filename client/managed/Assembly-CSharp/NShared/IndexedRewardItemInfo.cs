namespace NShared;

[JsonSerializable(Javascript = true)]
public class IndexedRewardItemInfo
{
	public int ItemIndex { get; set; }

	public int Star { get; set; }

	public int Count { get; set; }
}

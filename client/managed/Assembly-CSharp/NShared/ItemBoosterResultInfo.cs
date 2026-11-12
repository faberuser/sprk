namespace NShared;

[JsonSerializable(Javascript = true)]
public class ItemBoosterResultInfo
{
	public int RemainCount { get; set; }

	public int MaxCount { get; set; }
}

namespace NShared;

[JsonSerializable(Javascript = true)]
public class ItemUseResultInfo
{
	public int ItemIndex { get; set; }

	public int OutItemIndex { get; set; }

	public int AddCount { get; set; }

	public int NewCount { get; set; }

	public string CompletedTime { get; set; }
}

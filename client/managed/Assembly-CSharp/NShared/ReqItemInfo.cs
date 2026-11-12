namespace NShared;

[JsonSerializable(Javascript = true)]
public class ReqItemInfo
{
	public string Code { get; set; }

	public int Count { get; set; }
}

namespace NShared;

[JsonSerializable(Javascript = true)]
public class GlobalAccountInfo
{
	public string ServerGroup { get; set; }

	public long AccountId { get; set; }
}

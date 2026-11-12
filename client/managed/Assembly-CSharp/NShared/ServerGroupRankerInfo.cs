namespace NShared;

[JsonSerializable(Javascript = true)]
public class ServerGroupRankerInfo
{
	public int Rank { get; set; }

	public string ServerGroup { get; set; }

	public int Score { get; set; }
}

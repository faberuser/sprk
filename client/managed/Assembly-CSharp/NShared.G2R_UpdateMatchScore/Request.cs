namespace NShared.G2R_UpdateMatchScore;

[Request]
public class Request
{
	public Header __Header__ { get; set; }

	public string ServerGroup { get; set; }

	public string AccountId { get; set; }

	public int SeasonWin { get; set; }

	public int SeasonLose { get; set; }

	public int SuccessiveWin { get; set; }

	public int SuccessiveLose { get; set; }

	public int MatchScore { get; set; }

	public int WorldTierIndex { get; set; }

	public string LastMatchTime { get; set; }
}

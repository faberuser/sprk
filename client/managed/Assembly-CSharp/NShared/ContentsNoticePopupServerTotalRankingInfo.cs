namespace NShared;

[JsonSerializable(Javascript = true)]
public class ContentsNoticePopupServerTotalRankingInfo
{
	public int Rank { get; set; }

	public string ServerGroup { get; set; }

	public long Score { get; set; }
}

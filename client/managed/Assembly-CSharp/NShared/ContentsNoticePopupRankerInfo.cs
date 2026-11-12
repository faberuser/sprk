namespace NShared;

[JsonSerializable(Javascript = true)]
public class ContentsNoticePopupRankerInfo
{
	public int Rank { get; set; }

	public string Nick { get; set; }

	public long Score { get; set; }

	public int AvatarHeroIndex { get; set; }

	public string CountryCode { get; set; }

	public string ServerGroup { get; set; }
}

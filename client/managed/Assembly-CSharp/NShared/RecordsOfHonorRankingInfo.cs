namespace NShared;

[JsonSerializable(Javascript = true)]
public class RecordsOfHonorRankingInfo
{
	public RecordsOfHonorContentType ContentType { get; set; }

	public RecordsOfHonorRankingType RankingType { get; set; }

	public int Season { get; set; }

	public int Rank { get; set; }

	public string ServerGroup { get; set; }

	public long Id { get; set; }

	public string Name { get; set; }

	public int Level { get; set; }

	public int PictureIndex { get; set; }

	public string CountryCode { get; set; }

	public long Score { get; set; }

	public string MasterAvatarIndex { get; set; }

	public int Win { get; set; }

	public int Lose { get; set; }

	public int SuccessiveWin { get; set; }

	public DeckHeroInfo[] Deck { get; set; }

	public int Index { get; set; }
}

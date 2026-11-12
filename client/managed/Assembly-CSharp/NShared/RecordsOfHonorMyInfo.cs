namespace NShared;

[JsonSerializable(Javascript = true)]
public class RecordsOfHonorMyInfo
{
	public RecordsOfHonorContentType ContentType { get; set; }

	public RecordsOfHonorRankingType RankingType { get; set; }

	public int Season { get; set; }

	public int Rank { get; set; }
}

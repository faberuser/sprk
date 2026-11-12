namespace NShared;

[JsonSerializable(Javascript = true)]
public class RecordsOfHonorRankerInfo
{
	public RecordsOfHonorContentType ContentType { get; set; }

	public RecordsOfHonorRankingType RankingType { get; set; }

	public int Season { get; set; }

	public RecordsOfHonorRanker[] Ranker { get; set; }
}

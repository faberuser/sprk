namespace NShared;

[JsonSerializable(Javascript = true)]
public class PlayerRecommendPopup : IAccountAttrKey<int>
{
	int IAccountAttrKey<int>.Key => RecommendId;

	public int RecommendId { get; set; }

	public int PeriodCount { get; set; }
}

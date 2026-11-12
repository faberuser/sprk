namespace NShared;

[JsonSerializable(Javascript = true)]
public class RecommendRankInfo : IAccountAttrKey<string>
{
	string IAccountAttrKey<string>.Key => BattleIndex;

	public string BattleIndex { get; set; }

	public RecommendDeckInfo[] RankInfos { get; set; }
}

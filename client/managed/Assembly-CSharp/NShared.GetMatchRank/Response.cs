namespace NShared.GetMatchRank;

[Response]
public class Response : GenericResponse<ResultType>
{
	public PlayerBattleInfo BattleInfo { get; set; }

	public PlayerBattleBanPickInfo BattleBanPickInfo { get; set; }

	public PlayerBattleLuckyInfo BattleLuckyInfo { get; set; }

	public RankResultInfo RankResult { get; set; }

	public int[] RecommendHeroIndices { get; set; }
}

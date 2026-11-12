namespace NShared.WaitMatch;

[Response]
public class Response : GenericResponse<ResultType>
{
	public MatchInfo MatchInfo { get; set; }

	public MatchedAccountInfo MatchedNpcInfo { get; set; }

	public StaminaResultInfo SwordResult { get; set; }

	public int ChapterIndex { get; set; }

	public int DungeonIndex { get; set; }

	public PlayerBattleInfo BattleInfo { get; set; }

	public RankResultInfo RankResult { get; set; }

	public string RelayBattleServerAddress { get; set; }

	public int RelayBattleServerPort { get; set; }
}

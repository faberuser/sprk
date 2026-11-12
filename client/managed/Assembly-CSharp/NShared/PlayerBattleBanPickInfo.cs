namespace NShared;

[JsonSerializable(Javascript = true)]
public class PlayerBattleBanPickInfo : ArenaBattleInfo
{
	public int MatchScore { get; set; }

	public int SeasonWin { get; set; }

	public int SeasonLose { get; set; }

	public int SuccessiveWin { get; set; }

	public int SuccessiveLose { get; set; }

	public int TierIndex { get; set; }

	public int WorldTierIndex { get; set; }

	public string LastMatchTime { get; set; }
}

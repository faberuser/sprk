namespace NShared;

[BattlePacket]
public class BattleBanPickRes : BaseResponse
{
	public enum ResultType
	{
		Success,
		Fail,
		ClientNotFound,
		MatchNotFound,
		InvalidMatchInfo,
		BattleInstanceNotFound,
		NotEnoughClients,
		HeroNotFound,
		NotCorrectState,
		NotCorrectTeamType,
		NotCorrectHero,
		NotCorrectHeroCount
	}

	public ResultType Result { get; set; }

	public int RemainTimeSec { get; set; }

	public MatchBanPickState BanPickState { get; set; }

	public BanPickAccountInfo[] BanPickAccountInfo { get; set; }
}

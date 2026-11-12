namespace NShared;

[BattlePacket]
public class BattleBanPickStartRes : BaseResponse
{
	public enum ResultType
	{
		Success,
		Fail,
		ClientNotFound,
		BanPickStateNotFound,
		MatchNotFound,
		InvalidMatchInfo,
		BattleInstanceNotFound,
		NotEnoughClients
	}

	public ResultType Result { get; set; }
}

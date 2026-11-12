namespace NShared;

[BattlePacket]
public class BattleBanPickGiveUpRes : BaseResponse
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

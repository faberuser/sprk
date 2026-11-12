namespace NShared;

[BattlePacket]
public class BattleBanPickInfoRes : BaseResponse
{
	public enum ResultType
	{
		Success,
		Fail,
		ClientNotFound,
		MatchNotFound,
		InvalidMatchInfo,
		BanPickStateNotFound
	}

	public ResultType Result { get; set; }

	public int RemainTimeSec { get; set; }

	public MatchBanPickState BanPickState { get; set; }

	public BanPickAccountInfo[] BanPickAccountInfo { get; set; }
}

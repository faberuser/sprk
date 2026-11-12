namespace NShared;

[BattlePacket]
public class BattleLoginRes : BaseResponse
{
	public enum ResultType
	{
		Success,
		Fail,
		NotRunning,
		NotJoinable,
		Progressing,
		InternalFail,
		InternalResponseError,
		NotImplemented,
		MasterNotFound,
		MissingPartyInfo,
		MatchNotFound,
		Canceled,
		InvalidMatchInfo,
		MyAccountNotFound,
		RaidDataNotFound
	}

	public ResultType Result { get; set; }
}

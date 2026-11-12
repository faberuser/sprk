namespace NShared;

[BattlePacket]
public class BattleMessageRes : BaseResponse
{
	public enum ResultType
	{
		Success,
		Fail,
		ClientNotFound,
		BattleInstanceNotFound,
		MatchNotFound
	}

	public ResultType Result { get; set; }
}

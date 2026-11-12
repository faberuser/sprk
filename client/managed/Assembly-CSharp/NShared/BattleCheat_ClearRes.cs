namespace NShared;

[BattlePacket]
public class BattleCheat_ClearRes : BaseResponse
{
	public enum ResultType
	{
		Success,
		Fail,
		ClientNotFound,
		BattleInstanceNotFound,
		MatchInfoNotFound,
		NotAllowed
	}

	public ResultType Result { get; set; }
}

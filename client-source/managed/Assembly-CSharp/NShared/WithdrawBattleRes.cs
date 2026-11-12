namespace NShared;

[BattlePacket]
public class WithdrawBattleRes : BaseResponse
{
	public enum ResultType
	{
		Success,
		Fail,
		ClientNotFound,
		BattleInstanceNotFound
	}

	public ResultType Result { get; set; }
}

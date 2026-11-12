namespace NShared;

[BattlePacket]
public class BattleStartRes : BaseResponse
{
	public enum ResultType
	{
		Success,
		Fail,
		ClientNotFound
	}

	public ResultType Result { get; set; }
}

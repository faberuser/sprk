namespace NShared;

[BattlePacket]
public class BattleEnterRes : BaseResponse
{
	public enum ResultType
	{
		Success,
		Fail,
		ClientNotFound
	}

	public ResultType Result { get; set; }
}

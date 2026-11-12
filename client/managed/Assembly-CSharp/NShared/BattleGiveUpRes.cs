namespace NShared;

[BattlePacket]
public class BattleGiveUpRes : BaseResponse
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

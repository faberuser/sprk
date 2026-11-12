namespace NShared;

[BattlePacket]
public class EclipseContinuousBattleRes : BaseResponse
{
	public enum ResultType
	{
		Success,
		Fail,
		ClientNotFound,
		BattleInstanceNotFound,
		WrongModeUpdater
	}

	public ResultType Result { get; set; }
}

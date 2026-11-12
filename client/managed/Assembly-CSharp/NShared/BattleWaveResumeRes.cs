namespace NShared;

[BattlePacket]
public class BattleWaveResumeRes : BaseResponse
{
	public enum ResultType
	{
		Success,
		Fail,
		ClientNotFound
	}

	public ResultType Result { get; set; }
}

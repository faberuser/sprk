namespace NShared;

[BattlePacket]
public class PingRes : BaseResponse
{
	public enum ResultType
	{
		Success,
		Fail,
		ClientNotFound,
		BattleInstanceNotFound
	}

	public ResultType Result { get; set; }

	public long ClientTimeMs { get; set; }

	public long ServerTimeMs { get; set; }
}

namespace NShared;

[BattlePacket]
public class BaseCreatureActionRes : BaseResponse
{
	public enum ResultType
	{
		Success,
		Fail,
		ClientNotFound,
		BattleInstanceNotFound,
		CreatureNotFound,
		InvalidBattlePlayer
	}

	public ResultType Result { get; set; }
}

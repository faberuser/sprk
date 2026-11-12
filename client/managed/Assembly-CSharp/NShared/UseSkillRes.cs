namespace NShared;

[BattlePacket]
public class UseSkillRes : BaseResponse
{
	public enum ResultType
	{
		Success,
		Fail,
		ClientNotFound,
		BattleInstanceNotFound,
		CreatureNotFound,
		SkillDataNotFound,
		CannotUseSkill,
		InvalidBattlePlayer
	}

	public ResultType Result { get; set; }
}

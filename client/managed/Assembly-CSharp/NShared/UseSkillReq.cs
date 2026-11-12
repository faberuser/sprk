namespace NShared;

[BattlePacket]
public class UseSkillReq : BaseRequest
{
	public int CreatureUid { get; set; }

	public int SkillSlotIndex { get; set; }
}

namespace NShared;

[BattleCallback]
public sealed class CancelReservedSkill : BaseBattleCallback
{
	public int CreatureUid { get; set; }

	public int SkillId { get; set; }

	public override bool IsEquals(BaseBattleCallback callbackBase)
	{
		if (!(callbackBase is CancelReservedSkill cancelReservedSkill))
		{
			return false;
		}
		if (base.GlobalTimestamp == cancelReservedSkill.GlobalTimestamp && CreatureUid == cancelReservedSkill.CreatureUid)
		{
			return SkillId == cancelReservedSkill.SkillId;
		}
		return false;
	}
}

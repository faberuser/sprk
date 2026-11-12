namespace NShared;

[BattleCallback]
public sealed class CancelCastingSkill : BaseBattleCallback
{
	public int CreatureUid { get; set; }

	public int SkillId { get; set; }

	public override bool IsEquals(BaseBattleCallback callbackBase)
	{
		if (!(callbackBase is CancelCastingSkill cancelCastingSkill))
		{
			return false;
		}
		if (base.GlobalTimestamp == cancelCastingSkill.GlobalTimestamp && CreatureUid == cancelCastingSkill.CreatureUid)
		{
			return SkillId == cancelCastingSkill.SkillId;
		}
		return false;
	}
}

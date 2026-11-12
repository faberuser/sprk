namespace NShared;

[BattleCallback]
public sealed class EndFireSkill : BaseBattleCallback
{
	public int CreatureUid { get; set; }

	public int SkillId { get; set; }

	public bool IsCancel { get; set; }

	public override bool IsEquals(BaseBattleCallback callbackBase)
	{
		if (!(callbackBase is EndFireSkill endFireSkill))
		{
			return false;
		}
		if (base.GlobalTimestamp == endFireSkill.GlobalTimestamp && CreatureUid == endFireSkill.CreatureUid && SkillId == endFireSkill.SkillId)
		{
			return IsCancel == endFireSkill.IsCancel;
		}
		return false;
	}
}

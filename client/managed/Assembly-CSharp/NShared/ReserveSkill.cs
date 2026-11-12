namespace NShared;

[BattleCallback]
public sealed class ReserveSkill : BaseBattleCallback
{
	public int CreatureUid { get; set; }

	public int SkillId { get; set; }

	public override bool IsEquals(BaseBattleCallback callbackBase)
	{
		if (!(callbackBase is ReserveSkill reserveSkill))
		{
			return false;
		}
		if (base.GlobalTimestamp == reserveSkill.GlobalTimestamp && CreatureUid == reserveSkill.CreatureUid)
		{
			return SkillId == reserveSkill.SkillId;
		}
		return false;
	}
}

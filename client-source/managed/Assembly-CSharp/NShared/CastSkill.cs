namespace NShared;

[BattleCallback]
public sealed class CastSkill : BaseBattleCallback
{
	public int CreatureUid { get; set; }

	public int SkillId { get; set; }

	public int SkillLevel { get; set; }

	public int[] TargetCreatureUids { get; set; }

	public override bool IsEquals(BaseBattleCallback callbackBase)
	{
		if (!(callbackBase is CastSkill castSkill))
		{
			return false;
		}
		if (base.GlobalTimestamp == castSkill.GlobalTimestamp && CreatureUid == castSkill.CreatureUid && SkillId == castSkill.SkillId && SkillLevel == castSkill.SkillLevel)
		{
			return BaseBattleCallback.ArrayIsEquals(TargetCreatureUids, castSkill.TargetCreatureUids);
		}
		return false;
	}
}

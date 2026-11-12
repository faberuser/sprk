using NShared.NUtil;

namespace NShared;

[BattleCallback]
public sealed class BeginFireSkill : BaseBattleCallback
{
	public int CreatureUid { get; set; }

	public int SkillId { get; set; }

	public int FiredSkillId { get; set; }

	public int FirstTargetCreatureUid { get; set; }

	public Vector3 FirePosition { get; set; }

	public Vector3 TargetPosition { get; set; }

	public int DurationTimeMs { get; set; }

	public bool IsIgnoreSoloTime { get; set; }

	public int SkillSlotIndex { get; set; }

	public override bool IsEquals(BaseBattleCallback callbackBase)
	{
		if (!(callbackBase is BeginFireSkill beginFireSkill))
		{
			return false;
		}
		if (base.GlobalTimestamp == beginFireSkill.GlobalTimestamp && CreatureUid == beginFireSkill.CreatureUid && SkillId == beginFireSkill.SkillId && FiredSkillId == beginFireSkill.FiredSkillId && FirstTargetCreatureUid == beginFireSkill.FirstTargetCreatureUid && FirePosition == beginFireSkill.FirePosition && TargetPosition == beginFireSkill.TargetPosition && DurationTimeMs == beginFireSkill.DurationTimeMs && IsIgnoreSoloTime == beginFireSkill.IsIgnoreSoloTime)
		{
			return SkillSlotIndex == beginFireSkill.SkillSlotIndex;
		}
		return false;
	}
}

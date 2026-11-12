namespace NShared.NOperationRuntime;

public class SetSkill : ModSkill
{
	protected override SkillModifier.ModifyType GetModifyType()
	{
		return SkillModifier.ModifyType.Set;
	}
}

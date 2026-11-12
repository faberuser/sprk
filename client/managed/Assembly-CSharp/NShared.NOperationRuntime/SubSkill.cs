namespace NShared.NOperationRuntime;

public class SubSkill : ModSkill
{
	protected override SkillModifier.ModifyType GetModifyType()
	{
		return SkillModifier.ModifyType.Sub;
	}
}

namespace NShared.NOperationRuntime;

public class SetSkillInt : ModSkillInt
{
	protected override SkillModifier.ModifyType GetModifyType()
	{
		return SkillModifier.ModifyType.Set;
	}
}

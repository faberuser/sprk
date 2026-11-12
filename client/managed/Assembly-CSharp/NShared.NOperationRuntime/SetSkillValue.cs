namespace NShared.NOperationRuntime;

public class SetSkillValue : ModSkillValue
{
	protected override SkillModifier.ModifyType GetModifyType()
	{
		return SkillModifier.ModifyType.Set;
	}
}

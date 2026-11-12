namespace NShared.NOperationRuntime;

public class SetSkillPower : ModSkillPower
{
	protected override SkillModifier.ModifyType GetModifyType()
	{
		return SkillModifier.ModifyType.Set;
	}
}

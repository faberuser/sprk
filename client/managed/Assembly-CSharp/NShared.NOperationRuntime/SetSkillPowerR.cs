namespace NShared.NOperationRuntime;

public class SetSkillPowerR : ModSkillPowerR
{
	protected override SkillModifier.ModifyType GetModifyType()
	{
		return SkillModifier.ModifyType.Set;
	}
}

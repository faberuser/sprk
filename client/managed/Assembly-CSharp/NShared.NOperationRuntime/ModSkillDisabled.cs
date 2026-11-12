namespace NShared.NOperationRuntime;

public class ModSkillDisabled : ModSkillEnabled
{
	protected override OperationFieldType GetModifyFieldType()
	{
		return OperationFieldType.Disabled;
	}
}

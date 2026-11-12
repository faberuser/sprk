namespace NShared.NOperationRuntime;

public class ModSkillEnabled : ModSkillBase
{
	protected override SkillModifier.ModifyType GetModifyType()
	{
		return SkillModifier.ModifyType.Set;
	}

	protected override int GetModifySkillIndex()
	{
		return base.OperationInfo.GetValueInt(0);
	}

	protected override int GetModifyOperationNo()
	{
		return base.OperationInfo.GetValueInt(1);
	}

	protected override OperationFieldType GetModifyFieldType()
	{
		return OperationFieldType.Enabled;
	}

	protected override int GetModifyFieldIndex()
	{
		return 0;
	}

	protected override string GetModifyValue()
	{
		return true.ToString();
	}
}

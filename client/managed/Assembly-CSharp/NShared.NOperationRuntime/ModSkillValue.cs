namespace NShared.NOperationRuntime;

public class ModSkillValue : ModSkillBase
{
	protected override SkillModifier.ModifyType GetModifyType()
	{
		return SkillModifier.ModifyType.Add;
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
		return OperationFieldType.Value;
	}

	protected override int GetModifyFieldIndex()
	{
		return base.OperationInfo.GetValueInt(2);
	}

	protected override string GetModifyValue()
	{
		return base.OperationInfo.GetValueString(3);
	}
}

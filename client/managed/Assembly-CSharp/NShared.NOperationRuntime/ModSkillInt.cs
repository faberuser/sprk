namespace NShared.NOperationRuntime;

public class ModSkillInt : ModSkillBase
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
		return base.OperationInfo.GetValueEnum<OperationFieldType>(2);
	}

	protected override int GetModifyFieldIndex()
	{
		return base.OperationInfo.GetValueInt(4);
	}

	protected override string GetModifyValue()
	{
		int valueInt = base.OperationInfo.GetValueInt(3);
		return MultiplyStackCount(valueInt).ToString();
	}
}

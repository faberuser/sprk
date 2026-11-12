namespace NShared.NOperationRuntime;

public class ModSkillPower : ModSkillBase
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
		return OperationFieldType.SkillPower;
	}

	protected override int GetModifyFieldIndex()
	{
		return 0;
	}

	protected override string GetModifyValue()
	{
		int valueInt = base.OperationInfo.GetValueInt(2);
		return MultiplyStackCount(valueInt).ToString();
	}
}

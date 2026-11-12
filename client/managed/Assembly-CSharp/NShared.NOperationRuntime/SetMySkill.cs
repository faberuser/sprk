namespace NShared.NOperationRuntime;

public class SetMySkill : BaseOperationRuntime
{
	public enum SkillChangeType
	{
		DurationTimeMs
	}

	protected override void OnBegin()
	{
		base.OnBegin();
		SkillChangeType valueEnum = base.OperationInfo.GetValueEnum<SkillChangeType>(0);
		string[] array = null;
		if (base.OperationInfo.OperationData != null && base.OperationInfo.OperationData.Values != null && base.OperationInfo.OperationData.Values.Length >= 2)
		{
			array = new string[base.OperationInfo.OperationData.Values.Length - 1];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = base.OperationInfo.OperationData.Values[i + 1];
			}
		}
		UsingSkillInfo usingSkillInfo = base.OperationInfo.Creature.SkillController.UsingSkillInfo;
		if (usingSkillInfo != null)
		{
			Change(usingSkillInfo, valueEnum, array);
		}
	}

	protected virtual void Change(UsingSkillInfo skillInfo, SkillChangeType changeType, string[] values)
	{
		if (changeType == SkillChangeType.DurationTimeMs)
		{
			skillInfo.DurationTimeMs = ArgParser.GetValueInt(values, 0);
		}
	}
}

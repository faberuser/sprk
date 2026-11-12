namespace NShared.NOperationRuntime;

public class ModMySkill : SetMySkill
{
	protected override void Change(UsingSkillInfo skillInfo, SkillChangeType changeType, string[] values)
	{
		if (changeType == SkillChangeType.DurationTimeMs)
		{
			skillInfo.DurationTimeMs += ArgParser.GetValueInt(values, 0);
		}
	}
}

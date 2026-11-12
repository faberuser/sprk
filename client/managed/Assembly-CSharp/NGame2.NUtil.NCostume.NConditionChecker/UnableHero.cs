using NShared;
using NVespa;

namespace NGame2.NUtil.NCostume.NConditionChecker;

public class UnableHero : ConditionChecker.ConditionBase
{
	public override bool Check(CreatureData creatureData, string[] values)
	{
		if (__Extension_Method_ICollection__.COUNT(values) < 1)
		{
			return false;
		}
		for (int i = 1; i < __Extension_Method_ICollection__.COUNT(values); i++)
		{
			if (!int.TryParse(values[i], out var result))
			{
				VespaDebug.LogError("ConditionChecker.UnableHero Values Parsing Fail / " + i);
				return false;
			}
			if (result == creatureData.Index)
			{
				return false;
			}
		}
		return true;
	}
}

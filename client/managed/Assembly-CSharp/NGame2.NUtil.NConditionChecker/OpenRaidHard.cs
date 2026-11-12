using NGame2.NUI.NWindow;

namespace NGame2.NUtil.NConditionChecker;

public class OpenRaidHard : ConditionChecker.ConditionBase
{
	public override bool Check(string[] values)
	{
		if (__Extension_Method_ICollection__.COUNT(values) > 2)
		{
			return false;
		}
		if (!bool.TryParse(values[1], out var result))
		{
			return false;
		}
		return RaidHelper.CanEnterHardMode() && result;
	}
}

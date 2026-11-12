using NGame2.NUI.NWindow.NBattle;

namespace NGame2.NUtil.NConditionChecker;

public class AutoRepeatBattle : ConditionChecker.ConditionBase
{
	public override bool Check(string[] values)
	{
		if (__Extension_Method_ICollection__.COUNT(values) < 2)
		{
			return false;
		}
		bool result = false;
		if (!bool.TryParse(values[1], out result))
		{
			return false;
		}
		if (NGame2.NUI.NWindow.NBattle.AutoRepeatBattle.IsEnabled() == result)
		{
			return true;
		}
		return false;
	}
}

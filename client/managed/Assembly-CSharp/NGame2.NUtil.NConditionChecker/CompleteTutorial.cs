using NGame2.NAccount;
using NVespa.NSingleton;

namespace NGame2.NUtil.NConditionChecker;

public class CompleteTutorial : ConditionChecker.ConditionBase
{
	public override bool Check(string[] values)
	{
		if (__Extension_Method_ICollection__.COUNT(values) < 3)
		{
			return false;
		}
		for (int i = 1; i + 1 < __Extension_Method_ICollection__.COUNT(values); i += 2)
		{
			if (!int.TryParse(values[i], out var result) || !bool.TryParse(values[i + 1], out var result2))
			{
				return false;
			}
			if (GlobalAutoBehaviour<AccountManager>.instance.TutorialManager != null)
			{
				bool flag = GlobalAutoBehaviour<AccountManager>.instance.TutorialManager.IsComplete(result);
				if (result2 != flag)
				{
					return false;
				}
			}
		}
		return true;
	}

	public bool ContainsTutorial(string[] values, int targetTutorial)
	{
		if (__Extension_Method_ICollection__.COUNT(values) < 3)
		{
			return false;
		}
		for (int i = 1; i + 1 < __Extension_Method_ICollection__.COUNT(values); i += 2)
		{
			if (!int.TryParse(values[i], out var result) || !bool.TryParse(values[i + 1], out var _))
			{
				return false;
			}
			if (targetTutorial == result)
			{
				return true;
			}
		}
		return false;
	}
}

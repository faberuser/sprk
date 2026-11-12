using NGame2.NAccount;
using NVespa.NSingleton;

namespace NGame2.NUtil.NModifyChecker;

public class ClearTutorial : ModifyChecker.ConditionBase
{
	public override bool Check(string[] values)
	{
		if (__Extension_Method_ICollection__.COUNT(values) < 2)
		{
			return false;
		}
		if (!int.TryParse(values[1], out var result))
		{
			return false;
		}
		if (GlobalAutoBehaviour<AccountManager>.instance.TutorialManager != null)
		{
			return GlobalAutoBehaviour<AccountManager>.instance.TutorialManager.IsComplete(result);
		}
		return false;
	}
}

using NGame2.NDropBonusEvent;
using NVespa.NSingleton;

namespace NGame2.NUtil.NConditionChecker;

public class CheckEventBuffCondition : ConditionChecker.ConditionBase
{
	public override bool Check(string[] values)
	{
		if (__Extension_Method_ICollection__.COUNT(values) > 2)
		{
			return false;
		}
		string text = values[1];
		if (String_Extension_Method.IsNullOrEmpty(text))
		{
			return false;
		}
		if (GlobalAutoBehaviour<DropBonusEventManager>.instance.GetEffectiveEvent(text) != null)
		{
			return true;
		}
		return false;
	}
}

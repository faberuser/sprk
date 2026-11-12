using NGame2.NParty;
using NVespa.NSingleton;

namespace NGame2.NUtil.NConditionChecker;

public class CheckMultiplay : ConditionChecker.ConditionBase
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
		if (GlobalAutoBehaviour<PartyManager>.instance.IsValidParty() != result)
		{
			return false;
		}
		return true;
	}
}

using NGame2.NUI.NHelper.NPortal;

namespace NGame2.NUtil.NConditionChecker;

public class EventShop : ConditionChecker.ConditionBase
{
	public override bool Check(string[] values)
	{
		if (__Extension_Method_ICollection__.COUNT(values) > 2)
		{
			return false;
		}
		return PortalHelper.CheckEventCondition(values);
	}
}

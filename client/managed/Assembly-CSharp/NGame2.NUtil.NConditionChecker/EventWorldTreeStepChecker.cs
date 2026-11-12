using NGame2.NDropBonusEvent;
using NGame2.NUI.NHelper.NEventGrowWorldTree;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NUtil.NConditionChecker;

public class EventWorldTreeStepChecker : ConditionChecker.ConditionBase
{
	public override bool Check(string[] values)
	{
		if (__Extension_Method_ICollection__.COUNT(values) > 3)
		{
			return false;
		}
		if (!int.TryParse(values[1], out var result) || !int.TryParse(values[2], out var result2))
		{
			return false;
		}
		if (!GlobalAutoBehaviour<DropBonusEventManager>.isValidInstance || GlobalAutoBehaviour<DropBonusEventManager>.instance == null)
		{
			return false;
		}
		if (__Extension_Method_ICollection__.IsNullOrEmpty(GlobalAutoBehaviour<DropBonusEventManager>.instance.GetEffectiveEvents(EventBonusType.GrowWorldTree)))
		{
			return false;
		}
		int currentStep = EventGrowWorldTreeHelper.GetCurrentStep(EventStepType.Group);
		int eventStepTypeDataCount = EventGrowWorldTreeHelper.GetEventStepTypeDataCount(EventStepType.Group);
		currentStep = ((currentStep >= eventStepTypeDataCount) ? eventStepTypeDataCount : currentStep);
		if (currentStep >= result)
		{
			return currentStep <= result2;
		}
		return false;
	}
}

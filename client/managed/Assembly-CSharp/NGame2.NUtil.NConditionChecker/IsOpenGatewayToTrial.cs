using NGame2.NUI.NWindow.NEntryMenu;
using NGame2.NUI.NWindow.NEntryMenu.NCondition;
using NVespa;

namespace NGame2.NUtil.NConditionChecker;

public class IsOpenGatewayToTrial : ConditionChecker.ConditionBase
{
	public override bool Check(string[] values)
	{
		return SingletonAuto<ConditionContainer>.instance.GetCondition(typeof(NGame2.NUI.NWindow.NEntryMenu.NCondition.IsOpenGatewayToTrial))?.Check(null, values) ?? false;
	}
}

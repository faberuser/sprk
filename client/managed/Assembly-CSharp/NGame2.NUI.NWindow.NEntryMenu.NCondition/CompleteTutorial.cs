using NGame2.NAccount;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow.NEntryMenu.NCondition;

public class CompleteTutorial : ICondition
{
	bool ICondition.Check(CampaignNodeData nodeData, string[] value)
	{
		int valueInt = ArgParser.GetValueInt(value, 1);
		return GlobalAutoBehaviour<AccountManager>.instance.TutorialManager.IsComplete(valueInt);
	}
}

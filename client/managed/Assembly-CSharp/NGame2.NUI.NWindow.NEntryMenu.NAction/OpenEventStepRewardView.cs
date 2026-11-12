using System.Collections.Generic;
using NGame2.NUI.NManager;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow.NEntryMenu.NAction;

public class OpenEventStepRewardView : IAction
{
	void IAction.Run(CampaignNodeData nodeData, NShared.EntryMenu menu, List<object> values)
	{
		string[] entryMenuButton = menu.EntryMenuButton;
		int valueInt = ArgParser.GetValueInt(entryMenuButton, 3, 3);
		string valueString = ArgParser.GetValueString(entryMenuButton, 4);
		int valueInt2 = ArgParser.GetValueInt(entryMenuButton, 5);
		SceneAutoBehaviour<EventStepRewardManagement>.instance.OpenEventStepRewardView(nodeData.EntryNPCCode, valueInt, valueString, valueInt2);
	}

	void IAction.Run(NShared.EntryMenu menu, List<object> values)
	{
		if (values != null && __Extension_Method_ICollection__.COUNT(values) > 0)
		{
			string[] entryMenuButton = menu.EntryMenuButton;
			int valueInt = ArgParser.GetValueInt(entryMenuButton, 3, 3);
			string valueString = ArgParser.GetValueString(entryMenuButton, 4);
			int valueInt2 = ArgParser.GetValueInt(entryMenuButton, 5);
			SceneAutoBehaviour<EventStepRewardManagement>.instance.OpenEventStepRewardView(values[0].ToString(), valueInt, valueString, valueInt2);
		}
	}
}

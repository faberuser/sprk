using System.Collections.Generic;
using NGame2.NUI.NManager;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow.NEntryMenu.NAction;

public class OpenPhaseStepRewardInfoPopup : IAction
{
	void IAction.Run(CampaignNodeData nodeData, NShared.EntryMenu menu, List<object> values)
	{
		string[] entryMenuButton = menu.EntryMenuButton;
		int valueInt = ArgParser.GetValueInt(entryMenuButton, 2);
		int valueInt2 = ArgParser.GetValueInt(entryMenuButton, 3);
		SceneAutoBehaviour<EventDungeonManagement>.instance.OpenPhaseStepRewardInfoPopup(valueInt, valueInt2);
	}

	void IAction.Run(NShared.EntryMenu menu, List<object> values)
	{
		if (values != null && __Extension_Method_ICollection__.COUNT(values) > 0)
		{
			Vespa_Common_Class_Extension_Method.Interface((IAction)this).Run(null, menu, values);
		}
	}
}

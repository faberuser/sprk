using System.Collections.Generic;
using NGame2.NUI.NManager;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow.NEntryMenu.NAction;

internal class OpenValanceIdentifiedInven : IAction
{
	void IAction.Run(CampaignNodeData nodeData, NShared.EntryMenu menu, List<object> values)
	{
		EquipItemInfo selectedEquipItemInfo = null;
		if (values != null && values.Count > 1)
		{
			selectedEquipItemInfo = (EquipItemInfo)values[1];
		}
		SceneAutoBehaviour<ValanceManagement>.instance.OpenIdentifiedInven(selectedEquipItemInfo);
	}

	void IAction.Run(NShared.EntryMenu menu, List<object> values)
	{
		Vespa_Common_Class_Extension_Method.Interface((IAction)this).Run(null, menu, values);
	}
}

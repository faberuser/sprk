using System.Collections.Generic;
using NGame2.NUI.NManager;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow.NEntryMenu.NAction;

internal class OpenValanceRenewInven : IAction
{
	void IAction.Run(CampaignNodeData nodeData, NShared.EntryMenu menu, List<object> values)
	{
		EquipItemInfo itemInfo = null;
		if (values != null && values.Count > 1)
		{
			itemInfo = (EquipItemInfo)values[1];
		}
		SceneAutoBehaviour<ValanceManagement>.instance.OpenRenewInven(itemInfo);
	}

	void IAction.Run(NShared.EntryMenu menu, List<object> values)
	{
		Vespa_Common_Class_Extension_Method.Interface((IAction)this).Run(null, menu, values);
	}
}

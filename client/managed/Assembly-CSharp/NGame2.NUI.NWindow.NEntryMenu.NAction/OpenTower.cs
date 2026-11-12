using System.Collections.Generic;
using NGame2.NUI.NManager;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow.NEntryMenu.NAction;

public class OpenTower : IAction
{
	void IAction.Run(CampaignNodeData nodeData, NShared.EntryMenu menu, List<object> values)
	{
		int result = 0;
		if (menu != null)
		{
			result = ArgParser.GetValueInt(menu.EntryMenuButton, 2);
		}
		else if (values.Count >= 2)
		{
			int.TryParse(values[1].ToString(), out result);
		}
		SceneAutoBehaviour<TowerManagement>.instance.OpenTower(result);
	}

	void IAction.Run(NShared.EntryMenu menu, List<object> values)
	{
		Vespa_Common_Class_Extension_Method.Interface((IAction)this).Run(null, menu, values);
	}
}

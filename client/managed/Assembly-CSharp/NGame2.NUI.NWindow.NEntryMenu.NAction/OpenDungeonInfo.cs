using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NManager.NLobby;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow.NEntryMenu.NAction;

public class OpenDungeonInfo : IAction
{
	void IAction.Run(CampaignNodeData nodeData, NShared.EntryMenu menu, List<object> values)
	{
		int result = 0;
		int result2 = 0;
		if (menu != null)
		{
			string[] entryMenuButton = menu.EntryMenuButton;
			result = ArgParser.GetValueInt(entryMenuButton, 1);
			result2 = ArgParser.GetValueInt(entryMenuButton, 2);
		}
		else
		{
			if (values.Count >= 3)
			{
				int.TryParse(values[1].ToString(), out result);
			}
			int.TryParse(values[2].ToString(), out result2);
		}
		int linkedDungeonIndex = GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.GetLinkedDungeonIndex(result, result2);
		SceneAutoBehaviour<LobbyCampaignDungeon>.instance.OpenDungeonInfo(result, result2, linkedDungeonIndex);
	}

	void IAction.Run(NShared.EntryMenu menu, List<object> values)
	{
		Vespa_Common_Class_Extension_Method.Interface((IAction)this).Run(null, menu, values);
	}
}

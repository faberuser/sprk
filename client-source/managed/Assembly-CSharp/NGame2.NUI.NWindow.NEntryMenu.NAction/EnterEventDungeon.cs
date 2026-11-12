using System.Collections.Generic;
using NGame2.NUI.NManager.NLobby;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow.NEntryMenu.NAction;

public class EnterEventDungeon : IAction
{
	void IAction.Run(CampaignNodeData nodeData, NShared.EntryMenu menu, List<object> values)
	{
		string[] entryMenuButton = menu.EntryMenuButton;
		int valueInt = ArgParser.GetValueInt(entryMenuButton, 2);
		int valueInt2 = ArgParser.GetValueInt(entryMenuButton, 3);
		SceneAutoBehaviour<LobbyCampaignDungeon>.instance.OpenBattleDungeonInfo(valueInt, valueInt2);
	}

	void IAction.Run(NShared.EntryMenu menu, List<object> values)
	{
	}
}

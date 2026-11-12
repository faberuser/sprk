using System.Collections.Generic;
using NGame2.NEventTrigger;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow.NBattle;
using NShared;
using NShared.NEvent;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow.NEntryMenu.NAction;

public class OpenGatewaytoTrial : IAction
{
	void IAction.Run(CampaignNodeData nodeData, NShared.EntryMenu menu, List<object> values)
	{
		string[] entryMenuButton = menu.EntryMenuButton;
		int valueInt = ArgParser.GetValueInt(entryMenuButton, 2);
		int valueInt2 = ArgParser.GetValueInt(entryMenuButton, 3);
		EnterEntryMenu enterEntryMenu = new EnterEntryMenu();
		enterEntryMenu.ChapterIndex = valueInt;
		enterEntryMenu.DungeonIndex = valueInt2;
		enterEntryMenu.SkipTrigger = AutoNextBattle.IsEnabled() || AutoRepeatBattle.IsEnabled();
		SingletonAuto<EventTriggerManager>.instance.RaiseEvent(enterEntryMenu);
		SceneAutoBehaviour<TowerManagement>.instance.OpenGatewayUI(null);
	}

	void IAction.Run(NShared.EntryMenu menu, List<object> values)
	{
		Vespa_Common_Class_Extension_Method.Interface((IAction)this).Run(null, menu, values);
	}
}

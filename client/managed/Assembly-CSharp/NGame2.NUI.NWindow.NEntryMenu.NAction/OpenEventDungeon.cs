using System.Collections.Generic;
using NGame2.NBattleContext;
using NGame2.NEventTrigger;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow.NBattle;
using NShared;
using NShared.NEvent;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow.NEntryMenu.NAction;

public class OpenEventDungeon : IAction
{
	void IAction.Run(CampaignNodeData nodeData, NShared.EntryMenu menu, List<object> values)
	{
		string[] entryMenuButton = menu.EntryMenuButton;
		int valueInt = ArgParser.GetValueInt(entryMenuButton, 2);
		int valueInt2 = ArgParser.GetValueInt(entryMenuButton, 3);
		bool valueBool = ArgParser.GetValueBool(entryMenuButton, 4);
		SceneAutoBehaviour<EventDungeonManagement>.instance.SetSkipDungeonList(valueBool);
		if (!valueBool)
		{
			EnterEntryMenu enterEntryMenu = new EnterEntryMenu();
			enterEntryMenu.ChapterIndex = valueInt;
			enterEntryMenu.DungeonIndex = valueInt2;
			enterEntryMenu.SkipTrigger = AutoNextBattle.IsEnabled() || AutoRepeatBattle.IsEnabled();
			if (SingletonAuto<EventTriggerManager>.instance.RaiseEvent(enterEntryMenu))
			{
				SceneAutoBehaviour<EventDungeonManagement>.instance.OpenEventDungeonList(valueInt);
			}
			else
			{
				SceneAutoBehaviour<EventDungeonManagement>.instance.OpenEventDungeonList(valueInt);
			}
			return;
		}
		EventDungeonData data = GameRoot.Instance.EventDungeonDataContainer.GetData(valueInt, valueInt2);
		int linkedDungeon = valueInt2;
		EnterDungeonData enterDungeonData = new EnterDungeonData(valueInt, valueInt2, linkedDungeon);
		if (data.PrefabName == "PartyBattleDungeonInfo")
		{
			SceneAutoBehaviour<EventDungeonManagement>.instance.GetInstance<PartyBattleDungeonInfo>().Open(enterDungeonData, SceneAutoBehaviour<EventDungeonManagement>.instance);
		}
		else if (data.PrefabName == "BattleDungeonInfo")
		{
			SceneAutoBehaviour<EventDungeonManagement>.instance.OpenBattleDungeonInfo(enterDungeonData);
		}
		else
		{
			SceneAutoBehaviour<EventDungeonManagement>.instance.GetInstance<PartyBattleDungeonInfo>().Open(enterDungeonData, SceneAutoBehaviour<EventDungeonManagement>.instance);
		}
	}

	void IAction.Run(NShared.EntryMenu menu, List<object> values)
	{
		Vespa_Common_Class_Extension_Method.Interface((IAction)this).Run(null, menu, values);
	}
}

using System.Collections.Generic;
using NGame2.NEventTrigger;
using NGame2.NUI.NManager;
using NShared;
using NShared.NEvent;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow.NEntryMenu.NAction;

public class OpenEventDungeonSelect : IAction
{
	void IAction.Run(CampaignNodeData nodeData, NShared.EntryMenu menu, List<object> values)
	{
		SceneAutoBehaviour<EventDungeonManagement>.instance.SetSkipDungeonList(isSkip: false);
		string[] entryMenuButton = menu.EntryMenuButton;
		bool valueBool = ArgParser.GetValueBool(entryMenuButton, 2);
		int valueInt = ArgParser.GetValueInt(entryMenuButton, 3);
		int valueInt2 = ArgParser.GetValueInt(entryMenuButton, 4);
		int valueInt3 = ArgParser.GetValueInt(entryMenuButton, 5);
		int valueInt4 = ArgParser.GetValueInt(entryMenuButton, 6);
		int valueInt5 = ArgParser.GetValueInt(entryMenuButton, 7);
		if (!valueBool)
		{
			SceneAutoBehaviour<EventDungeonManagement>.instance.OpenEventDungeonSelect(valueInt, valueInt2, valueInt3, valueInt4, valueInt5);
			return;
		}
		EnterEventDungeonSelect enterEventDungeonSelect = new EnterEventDungeonSelect();
		enterEventDungeonSelect.ChapterIndex = valueInt;
		enterEventDungeonSelect.DungeonIndex = valueInt2;
		if (SingletonAuto<EventTriggerManager>.instance.RaiseEvent(enterEventDungeonSelect))
		{
			SceneAutoBehaviour<EventDungeonManagement>.instance.OpenEventDungeonSelect(valueInt, valueInt2, valueInt3, valueInt4, valueInt5);
		}
		else
		{
			SceneAutoBehaviour<EventDungeonManagement>.instance.OpenEventDungeonSelect(valueInt, valueInt2, valueInt3, valueInt4, valueInt5);
		}
	}

	void IAction.Run(NShared.EntryMenu menu, List<object> values)
	{
		Vespa_Common_Class_Extension_Method.Interface((IAction)this).Run(null, menu, values);
	}
}

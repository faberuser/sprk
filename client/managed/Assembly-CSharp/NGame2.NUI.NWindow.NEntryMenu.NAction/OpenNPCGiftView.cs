using System.Collections.Generic;
using NGame2.NUI.NManager;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow.NEntryMenu.NAction;

public class OpenNPCGiftView : IAction
{
	void IAction.Run(CampaignNodeData nodeData, NShared.EntryMenu menu, List<object> values)
	{
		string valueString = ArgParser.GetValueString(menu.EntryMenuButton, 0);
		NpcFriendlyType valueEnum = ArgParser.GetValueEnum(menu.EntryMenuButton, 2, NpcFriendlyType.None);
		string valueString2 = ArgParser.GetValueString(menu.EntryMenuButton, 3);
		int valueInt = ArgParser.GetValueInt(menu.EntryMenuButton, 4);
		string[] entryMenuCondtion = null;
		if (!String_Extension_Method.IsNullOrEmpty(menu.EntryMenuCondition))
		{
			entryMenuCondtion = menu.EntryMenuCondition.Split(',');
		}
		SceneAutoBehaviour<NPCRecruitManagement>.instance.OpenNPCGiftView(nodeData.EntryNPCCode, valueString, valueEnum, entryMenuCondtion, valueString2, valueInt);
	}

	void IAction.Run(NShared.EntryMenu menu, List<object> values)
	{
		string valueString = ArgParser.GetValueString(menu.EntryMenuButton, 0);
		NpcFriendlyType valueEnum = ArgParser.GetValueEnum(menu.EntryMenuButton, 2, NpcFriendlyType.None);
		string valueString2 = ArgParser.GetValueString(menu.EntryMenuButton, 3);
		int valueInt = ArgParser.GetValueInt(menu.EntryMenuButton, 4);
		string[] entryMenuCondtion = null;
		if (!String_Extension_Method.IsNullOrEmpty(menu.EntryMenuCondition))
		{
			entryMenuCondtion = menu.EntryMenuCondition.Split(',');
		}
		if (values != null && __Extension_Method_ICollection__.COUNT(values) > 0)
		{
			SceneAutoBehaviour<NPCRecruitManagement>.instance.OpenNPCGiftView(values[0].ToString(), valueString, valueEnum, entryMenuCondtion, valueString2, valueInt);
		}
	}
}

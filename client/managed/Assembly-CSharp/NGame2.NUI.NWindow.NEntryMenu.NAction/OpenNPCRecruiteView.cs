using System.Collections.Generic;
using NGame2.NUI.NManager;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow.NEntryMenu.NAction;

public class OpenNPCRecruiteView : IAction
{
	void IAction.Run(CampaignNodeData nodeData, NShared.EntryMenu menu, List<object> values)
	{
		NpcFriendlyType valueEnum = ArgParser.GetValueEnum(menu.EntryMenuButton, 3, NpcFriendlyType.None);
		NpcRecruitType valueEnum2 = ArgParser.GetValueEnum(menu.EntryMenuButton, 4, NpcRecruitType.Default);
		SceneAutoBehaviour<NPCRecruitManagement>.instance.OpenNPCRecruitView(nodeData.EntryNPCCode, valueEnum, valueEnum2);
	}

	void IAction.Run(NShared.EntryMenu menu, List<object> values)
	{
		if (values != null && __Extension_Method_ICollection__.COUNT(values) > 0)
		{
			NpcFriendlyType valueEnum = ArgParser.GetValueEnum(menu.EntryMenuButton, 3, NpcFriendlyType.None);
			NpcRecruitType valueEnum2 = ArgParser.GetValueEnum(menu.EntryMenuButton, 4, NpcRecruitType.Default);
			SceneAutoBehaviour<NPCRecruitManagement>.instance.OpenNPCRecruitView(values[0].ToString(), valueEnum, valueEnum2);
		}
	}
}

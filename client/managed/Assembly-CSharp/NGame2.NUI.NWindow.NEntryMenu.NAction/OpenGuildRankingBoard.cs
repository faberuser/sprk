using System.Collections.Generic;
using NGame2.NUI.NManager;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow.NEntryMenu.NAction;

public class OpenGuildRankingBoard : IAction
{
	void IAction.Run(CampaignNodeData nodeData, NShared.EntryMenu menu, List<object> values)
	{
		if (menu == null)
		{
			RestoredPortal.OpenPortalGuildRankings();
			return;
		}
		GuildRankingBoardType valueEnum = ArgParser.GetValueEnum(menu.EntryMenuButton, 2, GuildRankingBoardType.None);
		SceneAutoBehaviour<GuildManagement2>.instance.OpenGuildRankingBoard(valueEnum);
	}

	void IAction.Run(NShared.EntryMenu menu, List<object> values)
	{
		Vespa_Common_Class_Extension_Method.Interface((IAction)this).Run(null, menu, values);
	}
}

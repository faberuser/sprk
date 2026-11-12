using System.Collections.Generic;
using NGame2.NUI.NManager.NLobby;
using NShared;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow.NEntryMenu.NAction;

public class OpenMonthlyHero : IAction
{
	void IAction.Run(CampaignNodeData nodeData, NShared.EntryMenu menu, List<object> values)
	{
		LobbyManagement instance = SceneAutoBehaviour<LobbyManagement>.instance;
		if (instance == null)
		{
			VespaDebug.LogError("LobbyManagement.instance == null");
		}
		else
		{
			instance.OpenMonthlyHeroWindow();
		}
	}

	void IAction.Run(NShared.EntryMenu menu, List<object> values)
	{
		Vespa_Common_Class_Extension_Method.Interface((IAction)this).Run(null, menu, values);
	}
}

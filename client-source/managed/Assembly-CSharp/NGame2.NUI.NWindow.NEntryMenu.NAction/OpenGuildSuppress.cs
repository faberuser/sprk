using System.Collections.Generic;
using NGame2.NEventTrigger;
using NGame2.NUI.NManager.NLobby;
using NShared;
using NShared.NEvent;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow.NEntryMenu.NAction;

public class OpenGuildSuppress : IAction
{
	void IAction.Run(CampaignNodeData nodeData, NShared.EntryMenu menu, List<object> values)
	{
		SceneAutoBehaviour<GuildSuppressManagement>.instance.OpenGuildSuppressRanking(raiseEvent());
	}

	private bool raiseEvent()
	{
		NShared.NEvent.OpenGuildSuppress triggerEvent = new NShared.NEvent.OpenGuildSuppress();
		return SingletonAuto<EventTriggerManager>.instance.RaiseEvent(triggerEvent);
	}

	void IAction.Run(NShared.EntryMenu menu, List<object> values)
	{
		Vespa_Common_Class_Extension_Method.Interface((IAction)this).Run(null, menu, values);
	}
}

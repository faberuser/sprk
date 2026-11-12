using System.Collections.Generic;
using NGame2.NEventTrigger;
using NGame2.NUI.NManager;
using NShared;
using NShared.NEvent;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow.NEntryMenu.NAction;

public class GuildConstruction : IAction
{
	void IAction.Run(CampaignNodeData nodeData, NShared.EntryMenu menu, List<object> values)
	{
		SceneAutoBehaviour<GuildManagement2>.instance.OpenGuildConstruction(OpenComplete);
	}

	private void OpenComplete()
	{
		OpenGuildConstruction triggerEvent = new OpenGuildConstruction();
		SingletonAuto<EventTriggerManager>.instance.RaiseEvent(triggerEvent);
	}

	void IAction.Run(NShared.EntryMenu menu, List<object> values)
	{
		Vespa_Common_Class_Extension_Method.Interface((IAction)this).Run(null, menu, values);
	}
}

using System;
using NShared;
using NShared.NCampaign;

namespace NGame2.NBattleThread.NThreadAction;

public class CampaignUpdaterAction : IThreadAction
{
	private Action<CampaignUpdater> _action;

	public bool IsWaiting { get; set; }

	public CampaignUpdaterAction(Action<CampaignUpdater> action)
	{
		_action = action;
	}

	void IThreadAction.Action(BattleInstance battleInstance, CampaignUpdater campaignUpdater)
	{
		if (campaignUpdater != null)
		{
			Vespa_System_Action_Extension_Method.fire(_action, campaignUpdater);
		}
	}
}

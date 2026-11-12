using System;
using NShared;
using NShared.NCampaign;

namespace NGame2.NBattleThread.NThreadAction;

public class CampaignUpdaterActionT<TCampaignUpdater> : IThreadAction where TCampaignUpdater : CampaignUpdater
{
	private Action<TCampaignUpdater> _action;

	public bool IsWaiting { get; set; }

	public CampaignUpdaterActionT(Action<TCampaignUpdater> action)
	{
		_action = action;
	}

	void IThreadAction.Action(BattleInstance battleInstance, CampaignUpdater campaignUpdater)
	{
		if (campaignUpdater is TCampaignUpdater i_argA)
		{
			Vespa_System_Action_Extension_Method.fire(_action, i_argA);
		}
	}
}

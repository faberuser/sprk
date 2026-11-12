using System;
using NShared;
using NShared.NCampaign;

namespace NGame2.NBattleThread.NThreadAction;

public class CommonAction : IThreadAction
{
	private Action<BattleInstance> _action;

	public bool IsWaiting { get; set; }

	public CommonAction(Action<BattleInstance> action)
	{
		_action = action;
	}

	void IThreadAction.Action(BattleInstance battleInstance, CampaignUpdater campaignUpdater)
	{
		Vespa_System_Action_Extension_Method.fire(_action, battleInstance);
	}
}

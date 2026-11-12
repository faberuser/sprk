using NVespa;

namespace NShared;

public class LocalBattleEventRaiser : IBattleEventRaiser
{
	private BattleEventRouter _router;

	public LocalBattleEventRaiser(BattleEventRouter router)
	{
		_router = router;
	}

	void IBattleEventRaiser.RaiseBattleEvent<TBattleEventServiceContext, TData>(TBattleEventServiceContext context, TData data)
	{
		if (_router == null)
		{
			VespaDebug.LogError("packet router is null");
		}
		else
		{
			_router.Handle(context, data);
		}
	}

	void IBattleEventRaiser.QueueBattleEvent<TBattleEventServiceContext, TData>(TBattleEventServiceContext context, TData data)
	{
	}
}

using NShared;

namespace NGame2.NBattleThread.NEventService;

public class GenericEventService<TBattleCallback> : IBattleEventService<ThreadInstance, TBattleCallback> where TBattleCallback : BaseBattleCallback
{
	void IBattleEventService<ThreadInstance, TBattleCallback>.Handle(ThreadInstance threadInstance, TBattleCallback callback)
	{
		if (!threadInstance.ISVALID())
		{
			JsonMarshaler.EncodeJsonString(callback);
		}
		else
		{
			threadInstance.WorkerThreadContext.BattleCallbackQueue.Enqueue(callback);
		}
	}
}

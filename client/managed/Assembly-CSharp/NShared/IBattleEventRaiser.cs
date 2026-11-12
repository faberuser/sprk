namespace NShared;

public interface IBattleEventRaiser
{
	void RaiseBattleEvent<TContext, TData>(TContext context, TData data) where TContext : IBattleEventServiceContext where TData : BaseBattleCallback;

	void QueueBattleEvent<TContext, TData>(TContext context, TData data) where TContext : IBattleEventServiceContext where TData : BaseBattleCallback;
}

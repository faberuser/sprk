namespace NShared;

public interface IBattleEventService<IContext, TData> where IContext : IBattleEventServiceContext
{
	void Handle(IContext context, TData data);
}

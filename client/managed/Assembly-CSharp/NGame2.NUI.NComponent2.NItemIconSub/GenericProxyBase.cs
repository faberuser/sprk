using NVespa;

namespace NGame2.NUI.NComponent2.NItemIconSub;

public abstract class GenericProxyBase<T> : ProxyBase where T : GenericProxyBase<T>, new()
{
	public override void DestroyThis()
	{
		SingletonAuto<ReservePool<T>>.instance.Release(this as T);
	}
}

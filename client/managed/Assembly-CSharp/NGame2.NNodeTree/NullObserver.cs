using NGameAsset;

namespace NGame2.NNodeTree;

public class NullObserver : IObserver
{
	public static NullObserver Default = new NullObserver();

	void IObserver.OnCreate(IAsyncLoad<IEffect> effectAsync)
	{
	}

	void IObserver.OnRelease(IAsyncLoad<IEffect> effectAsync)
	{
	}

	void IObserver.OnCreate(ObjectBase objectBase)
	{
	}

	void IObserver.OnRelease(ObjectBase objectBase)
	{
	}
}

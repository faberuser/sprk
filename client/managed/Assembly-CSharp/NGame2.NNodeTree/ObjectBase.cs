using NVespa;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NNodeTree;

public abstract class ObjectBase : LinkFactoryComponent
{
	public long Uid;

	public long LockCount;

	public IObserver Observer { get; private set; }

	public abstract Transform FindTransform(string nodeName);

	public abstract void ChainUpdate();

	public abstract void Visit(IVisitor visitor);

	protected abstract void OnCreateDerived();

	protected abstract void OnReleaseDerived();

	public void OnCreate(IObserver observer)
	{
		OnCreateDerived();
		Observer = observer;
		Observer.OnCreate(this);
	}

	public void OnRelease()
	{
		Observer.OnRelease(this);
		OnReleaseDerived();
	}
}

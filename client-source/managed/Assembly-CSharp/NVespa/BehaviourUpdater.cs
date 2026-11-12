using NVespa.NSingleton;
using UnityEngine;

namespace NVespa;

public abstract class BehaviourUpdater : MonoBehaviour, IUpdater, ILateUpdater
{
	protected virtual void Awake()
	{
		GlobalUpdateBehaviour.AddUpdater(this);
	}

	void IUpdater.ChainUpdate()
	{
		if (base.gameObject.activeInHierarchy)
		{
			ChainUpdate();
		}
	}

	public virtual void ChainUpdate()
	{
	}

	void ILateUpdater.ChainLateUpdate()
	{
		if (base.gameObject.activeInHierarchy)
		{
			ChainLateUpdate();
		}
	}

	public virtual void ChainLateUpdate()
	{
	}
}

namespace NVespa.NSingleton;

public class SceneUpdateBehaviour<T> : SceneAutoBehaviour<T>, IUpdater, ILateUpdater where T : SceneUpdateBehaviour<T>
{
	protected override void Awake()
	{
		base.Awake();
		GlobalUpdateBehaviour.AddUpdater(this);
	}

	void IUpdater.ChainUpdate()
	{
		if (base.gameObject.activeInHierarchy)
		{
			ChainUpdate();
		}
	}

	protected virtual void ChainUpdate()
	{
	}

	void ILateUpdater.ChainLateUpdate()
	{
		if (base.gameObject.activeInHierarchy)
		{
			ChainLateUpdate();
		}
	}

	protected virtual void ChainLateUpdate()
	{
	}
}

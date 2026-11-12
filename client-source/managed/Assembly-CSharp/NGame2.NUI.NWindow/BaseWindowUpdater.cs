using UnityEngine;

namespace NGame2.NUI.NWindow;

public class BaseWindowUpdater : BaseWindow
{
	[SerializeField]
	private float updatePeriod = 0.5f;

	protected float _lastUpdateTime;

	protected void SetUpdatePeriod(float period)
	{
		updatePeriod = period;
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		UpdatePerFrame();
		if (Time.time - _lastUpdateTime > updatePeriod)
		{
			_lastUpdateTime = Time.time;
			UpdatePeriodically();
		}
	}

	public virtual void UpdatePerFrame()
	{
	}

	public virtual void UpdatePeriodically()
	{
	}
}

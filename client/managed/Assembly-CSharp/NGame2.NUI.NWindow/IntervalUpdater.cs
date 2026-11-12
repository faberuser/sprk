using System;
using UnityEngine;

namespace NGame2.NUI.NWindow;

internal class IntervalUpdater
{
	private float _intervalTime = 1f;

	private float _sumIntervalTime;

	private Action _updateAction;

	public IntervalUpdater(Action updateAction)
	{
		_updateAction = updateAction;
	}

	public IntervalUpdater(Action updateAction, float interval)
	{
		_updateAction = updateAction;
		_intervalTime = interval;
	}

	public void SetInterval(float interval)
	{
		_intervalTime = interval;
	}

	public void Update()
	{
		if (_updateAction != null)
		{
			_sumIntervalTime += GetDeltaTime();
			if (!(_sumIntervalTime < _intervalTime))
			{
				_sumIntervalTime = 0f;
				Vespa_System_Action_Extension_Method.fire(_updateAction);
			}
		}
	}

	public void ForcedUpdate()
	{
		Vespa_System_Action_Extension_Method.fire(_updateAction);
	}

	private float GetDeltaTime()
	{
		return Time.deltaTime;
	}
}

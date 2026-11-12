using System;
using UnityEngine;

public class DelegateUpdater : MonoBehaviour
{
	private bool _opened;

	private float _lastUpdatTime;

	private WeakReference _owner;

	public Action OnUpdate { get; private set; }

	public float Interval { get; private set; }

	public void Open(object owner, Action onUpdate, float interval = 0f)
	{
		_owner = new WeakReference(owner);
		_lastUpdatTime = Time.time;
		_opened = true;
		OnUpdate = onUpdate;
		Interval = interval;
	}

	private void Reset()
	{
		if (_owner != null)
		{
			_owner.Target = null;
		}
		_owner = null;
		_lastUpdatTime = 0f;
		_opened = false;
	}

	private void Update()
	{
		if (_opened)
		{
			if (_owner == null || !_owner.IsAlive)
			{
				Reset();
			}
			else if (Time.time - _lastUpdatTime > Interval)
			{
				_lastUpdatTime = Time.time;
				Vespa_System_Action_Extension_Method.fire(OnUpdate);
			}
		}
	}
}

using System;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NDevice;

public class SleepModeManager : SingletonAuto<SleepModeManager>, IUpdater
{
	private DateTime _touchedTime;

	private int _sleepTimeout;

	public DateTime TouchedTime => _touchedTime;

	public SleepModeManager()
	{
		GlobalUpdateBehaviour.AddUpdater(this);
		GlobalAutoBehaviour<GuiManager>.instance.Add("SleepMode", this, delegate
		{
			GUILayout.Label("Screen.sleepTimeout : " + Screen.sleepTimeout);
			GUILayout.Label("Timeout setting : " + _sleepTimeout);
			GUILayout.Label("Remain : " + GetRemainSleepTimeout());
			if (GUILayout.Button("Never"))
			{
				DisableSleepTimeout();
			}
			if (GUILayout.Button("1Minute"))
			{
				SetSleepTimeout(60);
			}
			if (GUILayout.Button("2Minute"))
			{
				SetSleepTimeout(120);
			}
			if (GUILayout.Button("5Minute"))
			{
				SetSleepTimeout(300);
			}
			if (GUILayout.Button("10Minute"))
			{
				SetSleepTimeout(600);
			}
		});
	}

	void IUpdater.ChainUpdate()
	{
		UpdateTouchedTime();
	}

	public void Touch()
	{
		_touchedTime = DateTime.UtcNow;
	}

	public void SetSleepTimeout(int timeout)
	{
		_sleepTimeout = timeout;
		Screen.sleepTimeout = -1;
		Touch();
	}

	public void DisableSleepTimeout()
	{
		SetSleepTimeout(0);
	}

	public int GetRemainSleepTimeout()
	{
		int num = (int)(DateTime.UtcNow - _touchedTime).TotalSeconds;
		return Mathf.Max(_sleepTimeout - num, 0);
	}

	private void UpdateTouchedTime()
	{
		if (_sleepTimeout > 0 && GetRemainSleepTimeout() <= 0)
		{
			_touchedTime = DateTime.UtcNow.AddDays(1.0);
			Screen.sleepTimeout = _sleepTimeout;
		}
	}
}

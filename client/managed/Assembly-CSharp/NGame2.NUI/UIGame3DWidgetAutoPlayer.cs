using System.Collections.Generic;
using NVespa;
using NVespa.NUtil;
using UnityEngine;

namespace NGame2.NUI;

[RequireComponent(typeof(UIGame3DWidget))]
public class UIGame3DWidgetAutoPlayer : MonoBehaviour
{
	private int _lastEnableFrameCount = -1;

	private int _lastPlayedFrameCount = -1;

	public float Delay;

	public List<EventDelegate> OnStart = new List<EventDelegate>();

	public List<EventDelegate> OnComplete = new List<EventDelegate>();

	private void OnEnable()
	{
		if (_lastEnableFrameCount != Time.frameCount)
		{
			_lastEnableFrameCount = Time.frameCount;
			SingletonUpdater<Executer>.instance.DelayRun(Delay, this, delegate
			{
				EventDelegate.Execute(OnStart);
				GetComponent<UIGame3DWidget>().Play(OnPlayed);
			});
		}
	}

	private void OnPlayed()
	{
		if (_lastPlayedFrameCount != Time.frameCount)
		{
			_lastPlayedFrameCount = Time.frameCount;
			EventDelegate.Execute(OnComplete);
		}
	}
}

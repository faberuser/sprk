using System.Collections;
using NGame2.NTutorial;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NLobby;

public class ActivityManager : SceneAutoBehaviour<ActivityManager>
{
	private IActivity _pendingActivity;

	private IActivity _currentActivity;

	private Coroutine _currentCoroutine;

	public static void RequestStop()
	{
		if (SceneAutoBehaviour<ActivityManager>.isValidInstance)
		{
			SceneAutoBehaviour<ActivityManager>.instance.StopActivity();
		}
	}

	private void Start()
	{
	}

	private void Update()
	{
		if (_pendingActivity != null)
		{
			StopActivity();
			_currentActivity = _pendingActivity;
			_pendingActivity = null;
			VespaCoroutine.Start(this, coStartActivity());
		}
		if (SingletonUpdater<TutorialManager>.instance.IsProgressing)
		{
			StopActivity();
		}
	}

	private IEnumerator coStartActivity()
	{
		_currentCoroutine = VespaCoroutine.Start(this, _currentActivity.coStart());
		yield return _currentCoroutine;
		_currentCoroutine = null;
		_currentActivity = null;
	}

	public bool Run(string taskType, string[] taskValues)
	{
		return Run(SingletonAuto<ActivityFactory>.instance.Create(taskType, taskValues));
	}

	public bool Run(IActivity activity)
	{
		if (activity == null)
		{
			return false;
		}
		_pendingActivity = activity;
		return true;
	}

	public void StopActivity()
	{
		if (_currentCoroutine != null)
		{
			VespaCoroutine.Stop(this, _currentCoroutine);
			_currentCoroutine = null;
		}
		if (_currentActivity != null)
		{
			_currentActivity.OnRequestStop();
			_currentActivity = null;
		}
	}

	public bool IsRunning()
	{
		return _currentCoroutine != null;
	}
}

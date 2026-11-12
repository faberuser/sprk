using System;
using System.Collections;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NLobby;

public class ActivityTrigger : MonoBehaviour
{
	[Serializable]
	public class ActivityInfo
	{
		public string ActivityType;

		public string[] ActivityValues;

		public bool StopPrevActivity;
	}

	public ActivityInfo[] Activities;

	public void Run()
	{
		VespaCoroutine.Start(coRun());
	}

	private IEnumerator coRun()
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(Activities))
		{
			yield break;
		}
		ActivityInfo[] activities = Activities;
		foreach (ActivityInfo info in activities)
		{
			if (String_Extension_Method.IsNullOrEmpty(info.ActivityType))
			{
				continue;
			}
			if (!info.StopPrevActivity)
			{
				while (SceneAutoBehaviour<ActivityManager>.instance.IsRunning())
				{
					yield return null;
				}
			}
			SceneAutoBehaviour<ActivityManager>.instance.Run(info.ActivityType, info.ActivityValues);
			yield return null;
		}
	}
}

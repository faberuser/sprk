using System.Collections;
using NGame2.NUI.NWindow.NTutorial;
using NShared;
using NShared.NEvent;
using NVespa;
using UnityEngine;

namespace NGame2.NTutorial.NRuntime;

public class SkipTime : ITutorialRuntime, IEventRuntime, ICustomTutorialSkippable
{
	bool ITutorialRuntime.OnEvent(IEvent triggerEvent)
	{
		return false;
	}

	IEnumerator ITutorialRuntime.Run(TutorialWindow window, TutorialData tutorialData)
	{
		yield break;
	}

	IEnumerator ICustomTutorialSkippable.Skip(TutorialWindow window, TutorialData tutorialData)
	{
		float skipDurationTime = TutorialUtility.GetArgFloat(tutorialData, 0, 1f);
		float startedTime = Time.time;
		object skipTimeScaler = new object();
		UnityTimeScaler.SetScale(skipTimeScaler, 100f);
		while (Time.time < startedTime + skipDurationTime)
		{
			yield return null;
		}
		UnityTimeScaler.SetScale(skipTimeScaler, 1f);
	}
}

using System.Collections;
using NGame2.NUI.NWindow.NTutorial;
using NShared;
using NShared.NEvent;
using UnityEngine;

namespace NGame2.NTutorial.NRuntime;

public class WaitTime : ITutorialRuntime, IEventRuntime
{
	bool ITutorialRuntime.OnEvent(IEvent triggerEvent)
	{
		return false;
	}

	IEnumerator ITutorialRuntime.Run(TutorialWindow window, TutorialData tutorialData)
	{
		float duration = TutorialUtility.GetArgFloat(tutorialData, 0);
		float beginTime = Time.realtimeSinceStartup;
		do
		{
			yield return null;
		}
		while (!(Time.realtimeSinceStartup - beginTime > duration));
	}
}

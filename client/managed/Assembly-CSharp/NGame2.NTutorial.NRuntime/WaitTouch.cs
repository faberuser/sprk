using System;
using System.Collections;
using NGame2.NUI.NWindow.NTutorial;
using NShared;
using NShared.NEvent;

namespace NGame2.NTutorial.NRuntime;

public class WaitTouch : ITutorialRuntime, IEventRuntime, ICustomTutorialSkippable
{
	bool ITutorialRuntime.OnEvent(IEvent triggerEvent)
	{
		return false;
	}

	private void InitTouchCallback(TutorialWindow window, TutorialData tutorialData, Action onTouch)
	{
		int argInt = TutorialUtility.GetArgInt(tutorialData, 0);
		int argInt2 = TutorialUtility.GetArgInt(tutorialData, 1);
		int argInt3 = TutorialUtility.GetArgInt(tutorialData, 2);
		int argInt4 = TutorialUtility.GetArgInt(tutorialData, 3);
		bool argBool = TutorialUtility.GetArgBool(tutorialData, 4);
		window.SetTouchCallback(onTouch, oneshot: true, argInt, argInt2, argInt3, argInt4, argBool, processBack: false, null);
	}

	IEnumerator ITutorialRuntime.Run(TutorialWindow window, TutorialData tutorialData)
	{
		bool touched = false;
		InitTouchCallback(window, tutorialData, delegate
		{
			touched = true;
		});
		while (!touched)
		{
			yield return null;
		}
	}

	IEnumerator ICustomTutorialSkippable.Skip(TutorialWindow window, TutorialData tutorialData)
	{
		InitTouchCallback(window, tutorialData, null);
		window.ForceRunTouchAction();
		yield break;
	}
}

using System.Collections;
using NGame2.NUI.NWindow.NTutorial;
using NShared;
using NShared.NEvent;
using UnityEngine;

namespace NGame2.NTutorial.NRuntime;

public class ShowTooltipArrow : ITutorialRuntimeImmediate, ITutorialRuntime, IEventRuntime
{
	bool ITutorialRuntime.OnEvent(IEvent triggerEvent)
	{
		return false;
	}

	IEnumerator ITutorialRuntime.Run(TutorialWindow window, TutorialData tutorialData)
	{
		Vespa_Common_Class_Extension_Method.Interface((ITutorialRuntimeImmediate)this).RunImmediate(window, tutorialData);
		yield break;
	}

	void ITutorialRuntimeImmediate.RunImmediate(TutorialWindow window, TutorialData tutorialData)
	{
		float argFloat = TutorialUtility.GetArgFloat(tutorialData, 0);
		float argFloat2 = TutorialUtility.GetArgFloat(tutorialData, 1);
		float argFloat3 = TutorialUtility.GetArgFloat(tutorialData, 2);
		float argFloat4 = TutorialUtility.GetArgFloat(tutorialData, 3);
		window.ShowTooltipArrow(new Vector2(argFloat, argFloat2), new Vector2(argFloat3, argFloat4));
	}
}

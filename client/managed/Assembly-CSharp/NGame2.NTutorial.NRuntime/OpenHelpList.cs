using System.Collections;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NTutorial;
using NShared;
using NShared.NEvent;

namespace NGame2.NTutorial.NRuntime;

public class OpenHelpList : ITutorialRuntimeImmediate, ITutorialRuntime, IEventRuntime
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
		EventGuideListPopup.Open(ArgParser.GetValueString(tutorialData.Argument, 0));
	}
}

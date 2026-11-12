using System.Collections;
using NGame2.NUI.NWindow.NTutorial;
using NShared;
using NShared.NEvent;

namespace NGame2.NTutorial.NRuntime;

public class FocusAreaEffect : ITutorialRuntimeImmediate, ITutorialRuntime, IEventRuntime
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
		string arg = TutorialUtility.GetArg(tutorialData, 0);
		int argInt = TutorialUtility.GetArgInt(tutorialData, 1);
		int argInt2 = TutorialUtility.GetArgInt(tutorialData, 2);
		float argFloat = TutorialUtility.GetArgFloat(tutorialData, 3);
		float argFloat2 = TutorialUtility.GetArgFloat(tutorialData, 4);
		bool argBool = TutorialUtility.GetArgBool(tutorialData, 5);
		window.FocusEffectByUiPosition(argInt, argInt2, argFloat, argFloat2, arg, argBool);
	}
}

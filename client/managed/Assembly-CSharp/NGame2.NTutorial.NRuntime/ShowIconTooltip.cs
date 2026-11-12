using System.Collections;
using NGame2.NUI.NWindow.NTutorial;
using NShared;
using NShared.NEvent;
using NVespa.NGlobal;

namespace NGame2.NTutorial.NRuntime;

public class ShowIconTooltip : ITutorialRuntimeImmediate, ITutorialRuntime, IEventRuntime
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
		int argInt = TutorialUtility.GetArgInt(tutorialData, 0);
		int argInt2 = TutorialUtility.GetArgInt(tutorialData, 1);
		int argInt3 = TutorialUtility.GetArgInt(tutorialData, 2);
		int argInt4 = TutorialUtility.GetArgInt(tutorialData, 3);
		string arg = TutorialUtility.GetArg(tutorialData, 4);
		string arg2 = TutorialUtility.GetArg(tutorialData, 5);
		bool argBool = TutorialUtility.GetArgBool(tutorialData, 6);
		string stringDirect = LocalizationManager.GetStringDirect(arg2);
		window.ShowTooltip(argInt, argInt2, argInt3, argInt4, stringDirect, arg, argBool);
	}
}

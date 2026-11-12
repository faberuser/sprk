using System.Collections;
using NGame2.NUI.NWindow.NTutorial;
using NShared;
using NShared.NEvent;
using NVespa;
using NVespa.NGlobal;

namespace NGame2.NTutorial.NRuntime;

public class ShowTooltip2 : ITutorialRuntimeImmediate, ITutorialRuntime, IEventRuntime
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
		int argInt3 = TutorialUtility.GetArgInt(tutorialData, 3);
		int argInt4 = TutorialUtility.GetArgInt(tutorialData, 4);
		string arg2 = TutorialUtility.GetArg(tutorialData, 5);
		bool argBool = TutorialUtility.GetArgBool(tutorialData, 6);
		string stringDirect = LocalizationManager.GetStringDirect(arg2);
		UIWidget.Pivot pivot = EnumTools.Parse(arg, UIWidget.Pivot.TopLeft);
		window.ShowTooltip2(pivot, argInt, argInt2, argInt3, argInt4, stringDirect, string.Empty, argBool);
	}
}

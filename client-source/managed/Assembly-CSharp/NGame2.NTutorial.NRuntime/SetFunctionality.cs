using System.Collections;
using NGame2.NFunctionality;
using NGame2.NUI.NWindow.NTutorial;
using NShared;
using NShared.NEvent;
using NVespa.NSingleton;

namespace NGame2.NTutorial.NRuntime;

public class SetFunctionality : ITutorialRuntimeImmediate, ITutorialRuntime, IEventRuntime
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
		FunctionalityType argEnum = TutorialUtility.GetArgEnum<FunctionalityType>(tutorialData, 0);
		string arg = TutorialUtility.GetArg(tutorialData, 1);
		GlobalAutoBehaviour<FunctionalityManager>.instance.SetValue(argEnum, arg);
	}
}

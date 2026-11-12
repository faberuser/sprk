using System.Collections;
using NGame2.NUI.NWindow.NTutorial;
using NShared;
using NShared.NEvent;
using NVespa;
using UnityEngine;

namespace NGame2.NTutorial.NRuntime;

public class FocusWidgetEffect : ITutorialRuntimeImmediate, ITutorialRuntime, IEventRuntime
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
		string arg2 = TutorialUtility.GetArg(tutorialData, 2);
		int argInt2 = TutorialUtility.GetArgInt(tutorialData, 3);
		int argInt3 = TutorialUtility.GetArgInt(tutorialData, 4);
		float argFloat = TutorialUtility.GetArgFloat(tutorialData, 5);
		float argFloat2 = TutorialUtility.GetArgFloat(tutorialData, 6);
		bool argBool = TutorialUtility.GetArgBool(tutorialData, 7);
		Transform transform = Vespa_Unity_Transform_Extension_Method.FindFlexible(GameUIRoot.instance.transform, arg, argInt);
		if (transform == null)
		{
			VespaDebug.LogError(Define.LogPrefix + " FocusWidgetEffect: Failed to find widget " + arg);
			return;
		}
		UIWidget component = transform.GetComponent<UIWidget>();
		if (component != null)
		{
			window.FocusEffectByUiWidget(component, arg2, argInt2, argInt3, argFloat, argFloat2, argBool);
			return;
		}
		BoxCollider component2 = transform.GetComponent<BoxCollider>();
		if (component2 != null)
		{
			window.FocusEffectByBoxCollider(component2, arg2, argInt2, argInt3, argFloat, argFloat2, argBool);
		}
		else
		{
			VespaDebug.LogError(Define.LogPrefix + " FocusWidgetEffect: Widget component is null (" + arg);
		}
	}
}

using System.Collections;
using NGame2.NUI.NWindow.NTutorial;
using NShared;
using NShared.NEvent;
using NVespa;
using UnityEngine;

namespace NGame2.NTutorial.NRuntime;

public class FocusWidget : ITutorialRuntimeImmediate, ITutorialRuntime, IEventRuntime
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
		int argInt5 = TutorialUtility.GetArgInt(tutorialData, 5);
		bool argBool = TutorialUtility.GetArgBool(tutorialData, 6);
		Transform transform = Vespa_Unity_Transform_Extension_Method.FindFlexible(GameUIRoot.instance.transform, arg, argInt);
		if (transform == null)
		{
			VespaDebug.LogError(Define.LogPrefix + " Failed to find widget : " + arg);
			return;
		}
		UIWidget component = transform.GetComponent<UIWidget>();
		if (component != null)
		{
			window.FocusByUiWidget(component, argInt2, argInt3, argInt4, argInt5, argBool);
			return;
		}
		BoxCollider component2 = transform.GetComponent<BoxCollider>();
		if (component2 != null)
		{
			window.FocusByBoxCollider(component2, argInt2, argInt3, argInt4, argInt5, argBool);
		}
		else
		{
			VespaDebug.LogError(Define.LogPrefix + " Widget component is null : " + arg);
		}
	}
}

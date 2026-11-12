using System.Collections;
using NGame2.NUI.NWindow.NTutorial;
using NShared;
using NShared.NEvent;
using UnityEngine;

namespace NGame2.NTutorial.NRuntime;

public class AnimatorSetBool : ITutorialRuntimeImmediate, ITutorialRuntime, IEventRuntime
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
		string arg2 = TutorialUtility.GetArg(tutorialData, 1);
		bool argBool = TutorialUtility.GetArgBool(tutorialData, 2);
		if (String_Extension_Method.IsNullOrEmpty(arg))
		{
			return;
		}
		Transform transform = Vespa_Unity_Transform_Extension_Method.FindFlexible(GameUIRoot.instance.transform, arg);
		if (transform != null)
		{
			Animator component = transform.gameObject.GetComponent<Animator>();
			if (component != null)
			{
				component.SetBool(arg2, argBool);
			}
		}
	}
}

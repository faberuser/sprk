using System.Collections;
using NGame2.NUI.NWindow.NTutorial;
using NShared;
using NShared.NEvent;
using UnityEngine;

namespace NGame2.NTutorial.NRuntime;

public class MoveScrollFocus2 : ITutorialRuntimeImmediate, ITutorialRuntime, IEventRuntime
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
		float argFloat = TutorialUtility.GetArgFloat(tutorialData, 1);
		Transform transform = Vespa_Unity_Transform_Extension_Method.FindFlexible(GameUIRoot.instance.transform, arg);
		if (transform == null)
		{
			return;
		}
		UIScrollView component = transform.GetComponent<UIScrollView>();
		if (!(component == null))
		{
			switch (component.movement)
			{
			case UIScrollView.Movement.Horizontal:
				component.SetDragAmount(argFloat, 0f, updateScrollbars: false);
				break;
			case UIScrollView.Movement.Vertical:
				component.SetDragAmount(0f, argFloat, updateScrollbars: false);
				break;
			case UIScrollView.Movement.Custom:
				break;
			case UIScrollView.Movement.Unrestricted:
				break;
			}
		}
	}
}

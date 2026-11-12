using System.Collections;
using System.Collections.Generic;
using NGame2.NUI.NWindow.NTutorial;
using NShared;
using NShared.NEvent;
using NVespa.NNGUIExtension;
using UnityEngine;

namespace NGame2.NTutorial.NRuntime;

public class MoveScrollFocus : ITutorialRuntimeImmediate, ITutorialRuntime, IEventRuntime
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
		Transform transform = Vespa_Unity_Transform_Extension_Method.FindFlexible(GameUIRoot.instance.transform, arg, argInt);
		if (transform == null)
		{
			return;
		}
		UIScrollGrid component = transform.GetComponent<UIScrollGrid>();
		if (!(component == null) && !(component.Grid == null))
		{
			List<Transform> childList = component.Grid.GetChildList();
			if (!__Extension_Method_ICollection__.IsNullOrEmpty(childList) && childList.Count > argInt2)
			{
				component.ScrollTo(childList[argInt2], 0f, 0f);
			}
		}
	}
}

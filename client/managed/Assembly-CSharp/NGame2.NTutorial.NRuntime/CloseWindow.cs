using System.Collections;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NTutorial;
using NShared;
using NShared.NEvent;
using NVespa;
using UnityEngine;

namespace NGame2.NTutorial.NRuntime;

public class CloseWindow : ITutorialRuntimeImmediate, ITutorialRuntime, IEventRuntime
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
		Transform transform = Vespa_Unity_Transform_Extension_Method.FindFlexible(GameUIRoot.instance.transform, arg);
		if (transform == null)
		{
			VespaDebug.LogError(Define.LogPrefix + " Failed to find window : " + arg);
			return;
		}
		BaseWindow component = transform.GetComponent<BaseWindow>();
		if (component != null)
		{
			component.Close();
		}
	}
}

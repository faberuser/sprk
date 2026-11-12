using System.Collections;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NTutorial;
using NShared;
using NShared.NEvent;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NTutorial.NRuntime;

public class CloseEquipAwakenScene : ITutorialRuntimeImmediate, ITutorialRuntime, IEventRuntime
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
		if (SceneAutoBehaviour<BlacksmithManagement>.instance.IsOpened<EquipAwakenScene>())
		{
			if (__Extension_Method_ICollection__.IsNullOrEmpty(tutorialData.Argument))
			{
				SceneAutoBehaviour<BlacksmithManagement>.instance.GetInstance<EquipAwakenScene>().OnClickClose();
				return;
			}
			float argFloat = TutorialUtility.GetArgFloat(tutorialData, 0);
			VespaCoroutine.Start(coCloseScene(argFloat));
		}
	}

	private IEnumerator coCloseScene(float waitTime)
	{
		float beginTime = Time.realtimeSinceStartup;
		do
		{
			yield return null;
		}
		while (!(Time.realtimeSinceStartup - beginTime >= waitTime));
		SceneAutoBehaviour<BlacksmithManagement>.instance.GetInstance<EquipAwakenScene>().OnClickClose();
	}
}

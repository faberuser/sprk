using System.Collections;
using NGame2.NUI.NWindow.NTutorial;
using NShared;
using NShared.NEvent;
using UnityEngine;

namespace NGame2.NTutorial.NRuntime;

public class WaitHideObject : ITutorialRuntime, IEventRuntime, ICustomTutorialSkippable
{
	bool ITutorialRuntime.OnEvent(IEvent triggerEvent)
	{
		return false;
	}

	IEnumerator ITutorialRuntime.Run(TutorialWindow window, TutorialData tutorialData)
	{
		string objectName = TutorialUtility.GetArg(tutorialData, 0);
		float argFloat = TutorialUtility.GetArgFloat(tutorialData, 1);
		float afterDelayTime = TutorialUtility.GetArgFloat(tutorialData, 2);
		float timeout = TutorialUtility.GetArgFloat(tutorialData, 2);
		yield return new WaitForSeconds(argFloat);
		if (!String_Extension_Method.IsNullOrEmpty(objectName))
		{
			Transform objTransform = Vespa_Unity_Transform_Extension_Method.FindFlexible(GameUIRoot.instance.transform, objectName);
			float beginTime = Time.realtimeSinceStartup;
			while (objTransform != null && objTransform.gameObject.activeInHierarchy)
			{
				float num = Time.realtimeSinceStartup - beginTime;
				if (timeout > Mathf.Epsilon && num > timeout)
				{
					break;
				}
				yield return null;
			}
		}
		yield return new WaitForSeconds(afterDelayTime);
	}

	IEnumerator ICustomTutorialSkippable.Skip(TutorialWindow window, TutorialData tutorialData)
	{
		yield return VespaCoroutine.Start(Vespa_Common_Class_Extension_Method.Interface((ITutorialRuntime)this).Run(window, tutorialData));
	}
}

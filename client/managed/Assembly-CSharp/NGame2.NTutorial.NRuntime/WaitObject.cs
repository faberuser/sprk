using System.Collections;
using NGame2.NUI.NWindow.NTutorial;
using NGame2.NUI.NWorldMap.NMapLayout;
using NShared;
using NShared.NEvent;
using UnityEngine;

namespace NGame2.NTutorial.NRuntime;

public class WaitObject : ITutorialRuntime, IEventRuntime, ICustomTutorialSkippable
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
		float timeout = TutorialUtility.GetArgFloat(tutorialData, 3);
		yield return new WaitForSeconds(argFloat);
		if (!String_Extension_Method.IsNullOrEmpty(objectName))
		{
			float beginTime = Time.realtimeSinceStartup;
			while (true)
			{
				Transform transform = Vespa_Unity_Transform_Extension_Method.FindFlexible(GameUIRoot.instance.transform, objectName);
				if (transform != null)
				{
					MapNode component = transform.gameObject.GetComponent<MapNode>();
					if (!(component != null) || component.IsEffectActivated())
					{
						break;
					}
				}
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

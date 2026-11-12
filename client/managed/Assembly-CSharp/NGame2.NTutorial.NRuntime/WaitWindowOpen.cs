using System;
using System.Collections;
using NGame2.NUI;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NTutorial;
using NShared;
using NShared.NEvent;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NTutorial.NRuntime;

public class WaitWindowOpen : ITutorialRuntime, IEventRuntime, ICustomTutorialSkippable
{
	bool ITutorialRuntime.OnEvent(IEvent triggerEvent)
	{
		return false;
	}

	IEnumerator ITutorialRuntime.Run(TutorialWindow window, TutorialData tutorialData)
	{
		string windowName = TutorialUtility.GetArg(tutorialData, 0);
		BaseWindow foundWindow = null;
		GlobalAutoBehaviour<GlobalManagement>.instance.ForEach(delegate(IManagementWrapper management)
		{
			if (!(foundWindow != null))
			{
				PoolingList<PrefabWrapper> openedList = management.GetOpenedList();
				for (int i = 0; i < openedList.COUNT(); i++)
				{
					IWindow window2 = openedList[i].Window;
					if (window2 != null && string.Equals(window2.GetType().Name, windowName, StringComparison.InvariantCultureIgnoreCase))
					{
						foundWindow = window2 as BaseWindow;
						break;
					}
				}
			}
		});
		if (foundWindow == null)
		{
			VespaDebug.LogError(Define.LogPrefix + " Cannot find opened window : " + windowName);
			yield break;
		}
		while (foundWindow.IsOpeningWindowAnimating())
		{
			yield return null;
		}
	}

	IEnumerator ICustomTutorialSkippable.Skip(TutorialWindow window, TutorialData tutorialData)
	{
		yield return VespaCoroutine.Start(Vespa_Common_Class_Extension_Method.Interface((ITutorialRuntime)this).Run(window, tutorialData));
	}
}

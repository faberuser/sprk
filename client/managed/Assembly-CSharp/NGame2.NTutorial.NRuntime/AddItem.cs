using System.Collections;
using NGame2.NAccount;
using NGame2.NUI.NWindow.NTutorial;
using NShared;
using NShared.NEvent;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NTutorial.NRuntime;

public class AddItem : ITutorialRuntimeImmediate, ITutorialRuntime, IEventRuntime
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
		int argInt = TutorialUtility.GetArgInt(tutorialData, 0);
		int argInt2 = TutorialUtility.GetArgInt(tutorialData, 1);
		ItemInfo itemInfo = GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Get(argInt);
		if (itemInfo != null)
		{
			itemInfo.Count = Mathf.Max(itemInfo.Count + argInt2, 0);
			if (itemInfo.Count == 0)
			{
				GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Remove(argInt);
				GlobalAutoBehaviour<AccountManager>.instance.ItemManager.AnnounceToObservers(itemInfo);
			}
		}
		else if (argInt2 > 0)
		{
			itemInfo = new ItemInfo();
			itemInfo.ItemIndex = argInt;
			itemInfo.Count = argInt2;
			GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Add(itemInfo);
			GlobalAutoBehaviour<AccountManager>.instance.ItemManager.AnnounceToObservers(itemInfo);
		}
	}
}

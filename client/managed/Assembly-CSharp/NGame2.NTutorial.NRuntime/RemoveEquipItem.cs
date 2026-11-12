using System.Collections;
using NGame2.NAccount;
using NGame2.NUI.NWindow.NTutorial;
using NShared;
using NShared.NEvent;
using NVespa.NSingleton;

namespace NGame2.NTutorial.NRuntime;

public class RemoveEquipItem : ITutorialRuntimeImmediate, ITutorialRuntime, IEventRuntime
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
		int num = TutorialUtility.GetArgInt(tutorialData, 1);
		if (num == 0)
		{
			num = 9999;
		}
		EquipItemInfo equipItemInfo = GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.Get(num);
		if (equipItemInfo != null && equipItemInfo.ItemIndex == argInt)
		{
			GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.Remove(equipItemInfo.SlotIndex);
		}
	}
}

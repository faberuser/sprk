using System.Collections;
using NGame2.NAccount;
using NGame2.NUI.NWindow.NTutorial;
using NShared;
using NShared.NEvent;
using NVespa.NSingleton;

namespace NGame2.NTutorial.NRuntime;

public class AddEquipItem : ITutorialRuntimeImmediate, ITutorialRuntime, IEventRuntime
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
		int argInt3 = TutorialUtility.GetArgInt(tutorialData, 2);
		int argInt4 = TutorialUtility.GetArgInt(tutorialData, 3);
		EquipItemInfo equipItemInfo = new EquipItemInfo();
		equipItemInfo.ItemIndex = argInt;
		equipItemInfo.Star = ((argInt2 > 0) ? argInt2 : 0);
		equipItemInfo.Level = argInt3;
		if (argInt4 == 0)
		{
			equipItemInfo.SlotIndex = 9999;
		}
		else
		{
			equipItemInfo.SlotIndex = argInt4;
		}
		GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.Apply(equipItemInfo);
	}
}

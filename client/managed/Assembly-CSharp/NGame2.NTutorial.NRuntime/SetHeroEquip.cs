using System.Collections;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow.NInventory;
using NGame2.NUI.NWindow.NTutorial;
using NShared;
using NShared.NEvent;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NTutorial.NRuntime;

public class SetHeroEquip : ITutorialRuntimeImmediate, ITutorialRuntime, IEventRuntime
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
		CachedHeroInfo cachedHeroInfo = GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(argInt);
		if (cachedHeroInfo == null)
		{
			VespaDebug.LogError("SetHeroEquip : Hero not found. heroIndex=" + argInt);
			return;
		}
		List<EquipItemInfo> equipItemInfoList = cachedHeroInfo.GetEquipItemInfoList();
		EquipItemInfo equipItemInfo = null;
		foreach (EquipItemInfo item in GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager)
		{
			if (item.ItemIndex == argInt3 && !equipItemInfoList.Contains(item))
			{
				equipItemInfo = item;
				break;
			}
		}
		if (equipItemInfo == null)
		{
			VespaDebug.LogError("SetHeroEquip : Equip not found. equipItemIndex=" + argInt3);
			return;
		}
		cachedHeroInfo.SetEquipItemInfo(argInt2, equipItemInfo);
		GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.ApplyEquippedHeroIndex(equipItemInfo, 0, argInt);
		InvenEquipInfoUIContainer instance = SceneAutoBehaviour<MyHeroManagement>.instance.GetInstance<InvenEquipInfoUIContainer>();
		if (instance != null && instance.IsOpen())
		{
			instance.Close();
		}
	}
}

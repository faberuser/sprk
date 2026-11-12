using System.Collections;
using NGame2.NAccount;
using NGame2.NUI.NHelper.NEquip;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow.NTutorial;
using NShared;
using NShared.NEvent;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NTutorial.NRuntime;

public class SetHeroEquipAuto : ITutorialRuntimeImmediate, ITutorialRuntime, IEventRuntime
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
		if (!SceneAutoBehaviour<MyHeroManagement>.isValidInstance)
		{
			VespaDebug.LogError("SetHeroEquipAuto : MyHeroManagement.isValidInstance == false");
			return;
		}
		CachedHeroInfo cachedHeroInfo = GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(argInt);
		if (cachedHeroInfo == null)
		{
			VespaDebug.LogError("SetHeroEquipAuto : Hero not found. heroIndex=" + argInt);
			return;
		}
		AutoEquip autoEquip = new AutoEquip(cachedHeroInfo);
		autoEquip.CollectEquipment();
		if (autoEquip.HasEquipment())
		{
			SceneAutoBehaviour<MyHeroManagement>.instance.RequestSetEquip(autoEquip.EquipItemSlotIndexList.ToArray(), argInt, autoEquip.PartIndexList.ToArray(), isPing: true);
		}
	}
}

using System.Collections;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NTutorial;
using NShared;
using NShared.NEvent;
using NVespa.NSingleton;

namespace NGame2.NTutorial.NRuntime;

public class AwakeHeroEquip : ITutorialRuntimeImmediate, ITutorialRuntime, IEventRuntime
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
		EquipItemInfo equipItemInfo = new EquipItemInfo();
		equipItemInfo.ItemIndex = argInt;
		equipItemInfo.Star = ((argInt2 > 0) ? argInt2 : 0);
		equipItemInfo.Level = argInt3;
		equipItemInfo.SlotIndex = 9999;
		HeroEquipAwaken instance = SceneAutoBehaviour<BlacksmithManagement>.instance.GetInstance<HeroEquipAwaken>();
		if (instance != null)
		{
			instance.ResetHeroEquipAwaken();
			instance.Open(SceneAutoBehaviour<BlacksmithManagement>.instance);
		}
		HeroEquipAwaken instance2 = SceneAutoBehaviour<BlacksmithManagement>.instance.GetInstance<HeroEquipAwaken>();
		if (instance2 != null)
		{
			SceneAutoBehaviour<BlacksmithManagement>.instance.GetInstance<EquipAwakenScene>().Open(equipItemInfo, instance2, EquipAwakenScene.CinemaType.Equip_Awaken_Tutorial, isSuccess: true, null);
		}
	}
}

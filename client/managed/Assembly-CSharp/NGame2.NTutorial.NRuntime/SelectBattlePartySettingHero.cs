using System.Collections;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NTutorial;
using NShared;
using NShared.NEvent;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NTutorial.NRuntime;

public class SelectBattlePartySettingHero : ITutorialRuntimeImmediate, ITutorialRuntime, IEventRuntime
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
		int[] array = new int[4] { argInt, argInt2, argInt3, argInt4 };
		BattlePartySetting window2 = GlobalAutoBehaviour<GlobalManagement>.instance.GetWindow((BattlePartySetting e) => e.IsOpen());
		if ((bool)window2 && window2.IsOpen())
		{
			int[] array2 = array;
			foreach (int heroIndex in array2)
			{
				HeroPortraitComponent deckHeroPortrait = window2.GetDeckHeroPortrait(heroIndex, 0L);
				if (!(deckHeroPortrait == null))
				{
					window2.InsertSquardHero(deckHeroPortrait, isMainSquard: true);
				}
			}
		}
		else
		{
			VespaDebug.LogError("BattlePartySetting window not found");
		}
	}
}

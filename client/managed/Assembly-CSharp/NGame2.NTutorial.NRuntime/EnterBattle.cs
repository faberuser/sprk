using System.Collections;
using NGame2.NBattleContext;
using NGame2.NUI.NWindow.NTutorial;
using NShared;
using NShared.NEvent;
using NVespa;

namespace NGame2.NTutorial.NRuntime;

public class EnterBattle : ITutorialRuntimeImmediate, ITutorialRuntime, IEventRuntime
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
		DungeonDifficultyType argEnum = TutorialUtility.GetArgEnum<DungeonDifficultyType>(tutorialData, 2);
		SingletonAuto<BattleContext>.instance.SetCustomCampaign(argInt, argInt2, argEnum);
		SingletonAuto<CampaignContext>.instance.SetCustomCampaign(argInt, argInt2, argEnum);
		SingletonAuto<BattleContext>.instance.MoveToBattleScene();
	}
}

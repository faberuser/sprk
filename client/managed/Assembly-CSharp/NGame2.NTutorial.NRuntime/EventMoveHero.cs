using System.Collections;
using NGame2.NBattleField;
using NGame2.NBattleView;
using NGame2.NBattleView.NCreature.NHandler;
using NGame2.NUI.NWindow.NTutorial;
using NShared;
using NShared.NEvent;
using NVespa;

namespace NGame2.NTutorial.NRuntime;

public class EventMoveHero : ITutorialRuntimeImmediate, ITutorialRuntime, IEventRuntime
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
		TeamType teamType = EnumTools.Parse(TutorialUtility.GetArg(tutorialData, 0), TeamType.Left);
		int argInt = TutorialUtility.GetArgInt(tutorialData, 1);
		float argFloat = TutorialUtility.GetArgFloat(tutorialData, 2);
		Creature creature = null;
		for (int i = 0; i < SingletonAuto<BattleFacade>.instance.GetCreatureCount(); i++)
		{
			BattleFacade.CreatureCacheData creatureCacheByIndex = SingletonAuto<BattleFacade>.instance.GetCreatureCacheByIndex(i);
			if (creatureCacheByIndex.TeamType == teamType && creatureCacheByIndex.CreatureData.Index == argInt)
			{
				creature = BattleViewGenericSingleton<CreatureManager>.instance.GetCreature(creatureCacheByIndex.Uid);
				break;
			}
		}
		if (!(creature == null))
		{
			creature.NodeTreeController.SetExternalHandler(new TutorialEventMove(creature, argFloat));
		}
	}
}

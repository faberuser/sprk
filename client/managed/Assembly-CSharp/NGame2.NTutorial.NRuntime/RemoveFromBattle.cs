using System.Collections;
using NGame2.NBattleField;
using NGame2.NUI.NWindow.NTutorial;
using NShared;
using NShared.NEvent;
using NVespa;

namespace NGame2.NTutorial.NRuntime;

public class RemoveFromBattle : ITutorialRuntimeImmediate, ITutorialRuntime, IEventRuntime
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
		if (!SingletonAuto<BattleFacade>.instance.IsValidBattle())
		{
			return;
		}
		int num = 0;
		for (int i = 0; i < SingletonAuto<BattleFacade>.instance.GetCreatureCount(); i++)
		{
			BattleFacade.CreatureCacheData creatureCacheByIndex = SingletonAuto<BattleFacade>.instance.GetCreatureCacheByIndex(i);
			if (creatureCacheByIndex.TeamType == teamType && creatureCacheByIndex.CreatureData.Index == argInt)
			{
				num = creatureCacheByIndex.Uid;
				break;
			}
		}
		if (num != 0)
		{
			SingletonAuto<BattleFacade>.instance.EventRemoveCreatureFromBattle(num);
		}
	}
}

using System.Collections;
using System.Collections.Generic;
using NGame2.NBattleField;
using NGame2.NUI.NWindow.NTutorial;
using NShared;
using NShared.NEvent;
using NVespa;

namespace NGame2.NTutorial.NRuntime;

public class UseTeamSkill : ITutorialRuntimeImmediate, ITutorialRuntime, IEventRuntime
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
		TeamType argEnum = TutorialUtility.GetArgEnum<TeamType>(tutorialData, 0);
		int argInt = TutorialUtility.GetArgInt(tutorialData, 1);
		int argInt2 = TutorialUtility.GetArgInt(tutorialData, 2);
		if (!SingletonAuto<BattleFacade>.instance.IsValidBattle())
		{
			return;
		}
		List<int> list = new List<int>();
		for (int i = 0; i < SingletonAuto<BattleFacade>.instance.GetCreatureCount(); i++)
		{
			BattleFacade.CreatureCacheData creatureCacheByIndex = SingletonAuto<BattleFacade>.instance.GetCreatureCacheByIndex(i);
			if (creatureCacheByIndex.TeamType == argEnum && (argInt == 0 || creatureCacheByIndex.CreatureData.Index == argInt))
			{
				list.Add(creatureCacheByIndex.Uid);
			}
		}
		foreach (int item in list)
		{
			SingletonAuto<BattleFacade>.instance.FireEventSkillBattleLog(item, argInt2, 0);
		}
	}
}

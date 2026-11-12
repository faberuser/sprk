using System.Collections;
using NGame2.NBattleField;
using NGame2.NUI.NWindow.NTutorial;
using NShared;
using NShared.NEvent;
using NVespa;
using UnityEngine;

namespace NGame2.NTutorial.NRuntime;

public class WaitDefeatAllEnemy : ITutorialRuntime, IEventRuntime
{
	bool ITutorialRuntime.OnEvent(IEvent triggerEvent)
	{
		return false;
	}

	private bool CheckAllDead(TeamType teamType)
	{
		for (int i = 0; i < SingletonAuto<BattleFacade>.instance.GetCreatureCount(); i++)
		{
			BattleFacade.CreatureCacheData creatureCacheByIndex = SingletonAuto<BattleFacade>.instance.GetCreatureCacheByIndex(i);
			if (creatureCacheByIndex.TeamType == teamType && !creatureCacheByIndex.IsDeadOrResting())
			{
				return false;
			}
		}
		return true;
	}

	IEnumerator ITutorialRuntime.Run(TutorialWindow window, TutorialData tutorialData)
	{
		float afterDelayTime = TutorialUtility.GetArgFloat(tutorialData, 0);
		if (SingletonAuto<BattleFacade>.instance.IsValidBattle())
		{
			while (!CheckAllDead(TeamType.Right))
			{
				yield return null;
			}
			yield return new WaitForSeconds(afterDelayTime);
		}
	}
}

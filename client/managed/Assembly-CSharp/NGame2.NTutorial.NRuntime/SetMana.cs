using System.Collections;
using NGame2.NBattleField;
using NGame2.NUI.NWindow.NTutorial;
using NShared;
using NShared.NEvent;
using NVespa;

namespace NGame2.NTutorial.NRuntime;

public class SetMana : ITutorialRuntimeImmediate, ITutorialRuntime, IEventRuntime
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
		TutorialUtility.GetArgInt(tutorialData, 1);
		int mp = TutorialUtility.GetArgInt(tutorialData, 2);
		if (!SingletonAuto<BattleFacade>.instance.IsValidBattle())
		{
			return;
		}
		int creatureUid = 0;
		for (int i = 0; i < SingletonAuto<BattleFacade>.instance.GetCreatureCount(); i++)
		{
			BattleFacade.CreatureCacheData creatureCacheByIndex = SingletonAuto<BattleFacade>.instance.GetCreatureCacheByIndex(i);
			if (creatureCacheByIndex.Player.IsMe && creatureCacheByIndex.CreatureData.Index == argInt)
			{
				creatureUid = creatureCacheByIndex.Uid;
				break;
			}
		}
		if (creatureUid == 0)
		{
			return;
		}
		SingletonAuto<BattleFacade>.instance.AddThreadCommonAction(delegate(BattleInstance battleInstance)
		{
			BattleCreature creature = battleInstance.CreatureContainer.GetCreature(creatureUid);
			if (creature == null)
			{
				VespaDebug.LogError(Define.LogPrefix + " Cannot find creature : " + creatureUid);
			}
			else
			{
				creature.StatController.SetMp(mp, raiseEvent: true);
			}
		});
	}
}

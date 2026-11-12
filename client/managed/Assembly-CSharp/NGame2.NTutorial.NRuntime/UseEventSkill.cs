using System.Collections;
using NGame2.NBattleField;
using NGame2.NUI.NWindow.NTutorial;
using NShared;
using NShared.NEvent;
using NVespa;

namespace NGame2.NTutorial.NRuntime;

public class UseEventSkill : ITutorialRuntimeImmediate, ITutorialRuntime, IEventRuntime
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
		int argInt2 = TutorialUtility.GetArgInt(tutorialData, 2);
		TeamType teamType2 = EnumTools.Parse(TutorialUtility.GetArg(tutorialData, 3), TeamType.Left);
		int argInt3 = TutorialUtility.GetArgInt(tutorialData, 4);
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
		if (num == 0)
		{
			return;
		}
		int manualTargetUid = 0;
		for (int j = 0; j < SingletonAuto<BattleFacade>.instance.GetCreatureCount(); j++)
		{
			BattleFacade.CreatureCacheData creatureCacheByIndex2 = SingletonAuto<BattleFacade>.instance.GetCreatureCacheByIndex(j);
			if (creatureCacheByIndex2.TeamType == teamType2 && creatureCacheByIndex2.CreatureData.Index == argInt3)
			{
				manualTargetUid = creatureCacheByIndex2.Uid;
				break;
			}
		}
		SingletonAuto<BattleFacade>.instance.FireEventSkillBattleLog(num, argInt2, manualTargetUid);
	}
}

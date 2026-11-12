using System.Collections;
using NGame2.NAccount;
using NGame2.NBattleField;
using NGame2.NBattleThread.NThreadAction;
using NGame2.NUI.NWindow.NTutorial;
using NShared;
using NShared.NEvent;
using NVespa;

namespace NGame2.NTutorial.NRuntime;

public class WaitCanUseSkill : ITutorialRuntime, IEventRuntime
{
	bool ITutorialRuntime.OnEvent(IEvent triggerEvent)
	{
		return false;
	}

	IEnumerator ITutorialRuntime.Run(TutorialWindow window, TutorialData tutorialData)
	{
		int argInt = TutorialUtility.GetArgInt(tutorialData, 0);
		int argInt2 = TutorialUtility.GetArgInt(tutorialData, 1);
		if (!SingletonAuto<BattleFacade>.instance.IsValidBattle())
		{
			yield break;
		}
		int num = 0;
		CachedHeroInfo heroInfo = null;
		for (int i = 0; i < SingletonAuto<BattleFacade>.instance.GetCreatureCount(); i++)
		{
			BattleFacade.CreatureCacheData creatureCacheByIndex = SingletonAuto<BattleFacade>.instance.GetCreatureCacheByIndex(i);
			if (creatureCacheByIndex.Player.IsMe && creatureCacheByIndex.CreatureData.Index == argInt)
			{
				num = creatureCacheByIndex.Uid;
				heroInfo = creatureCacheByIndex.HeroInfo;
				break;
			}
		}
		if (num == 0)
		{
			yield break;
		}
		Helper.SkillBasicInfo skillBasicInfo = Helper.GetSkillBasicInfo(heroInfo, argInt2);
		SkillUseOption skillUseOption = new SkillUseOption(checkOnly: true, checkLevel: false);
		GetResultOfCanUseSkill syncCanUseSkill = new GetResultOfCanUseSkill(num, skillBasicInfo.TableData, skillUseOption);
		do
		{
			SingletonAuto<BattleFacade>.instance.AddThreadSync(syncCanUseSkill);
			while (syncCanUseSkill.IsWaiting)
			{
				yield return null;
			}
		}
		while (syncCanUseSkill.SkillUseResult != SkillUseResult.Success);
	}
}

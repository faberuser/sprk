using System.Collections;
using NGame2.NBattleField;
using NGame2.NUI.NWindow.NTutorial;
using NShared;
using NShared.NEvent;
using NVespa;

namespace NGame2.NTutorial.NRuntime;

public class CancelTeamSkill : ITutorialRuntimeImmediate, ITutorialRuntime, IEventRuntime
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
		TeamType teamType = TutorialUtility.GetArgEnum<TeamType>(tutorialData, 0);
		int heroIndex = TutorialUtility.GetArgInt(tutorialData, 1);
		int skillIndex = TutorialUtility.GetArgInt(tutorialData, 2);
		if (!SingletonAuto<BattleFacade>.instance.IsValidBattle())
		{
			return;
		}
		SingletonAuto<BattleFacade>.instance.AddThreadCommonAction(delegate(BattleInstance battleInstance)
		{
			BattleCreatureList allCreaturesOfTeam = battleInstance.CreatureContainer.GetAllCreaturesOfTeam((int)teamType);
			if (allCreaturesOfTeam != null && allCreaturesOfTeam.Count != 0)
			{
				for (int i = 0; i < allCreaturesOfTeam.Count; i++)
				{
					BattleCreature battleCreature = allCreaturesOfTeam[i];
					if (heroIndex == 0 || battleCreature.CreatureData.Index == heroIndex)
					{
						if (battleCreature.SkillController.UsingSkillInfo != null && battleCreature.SkillController.UsingSkillInfo.SkillData.Index == skillIndex)
						{
							battleCreature.SkillController.StopUsingDurationalSkill(isCancel: true);
						}
						battleCreature.CancelMyOperation(battleCreature, passiveOnly: false, skillIndex);
					}
				}
			}
		});
	}
}

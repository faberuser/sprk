using System.Collections;
using NGame2.NBattleField;
using NGame2.NUI.NWindow.NTutorial;
using NShared;
using NShared.NEvent;
using NVespa;

namespace NGame2.NTutorial.NRuntime;

public class ResetCooltime : ITutorialRuntimeImmediate, ITutorialRuntime, IEventRuntime
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
		int heroIndex = TutorialUtility.GetArgInt(tutorialData, 0);
		int skillSlotIndex = TutorialUtility.GetArgInt(tutorialData, 1);
		if (!SingletonAuto<BattleFacade>.instance.IsValidBattle())
		{
			return;
		}
		int creatureUid = 0;
		for (int i = 0; i < SingletonAuto<BattleFacade>.instance.GetCreatureCount(); i++)
		{
			BattleFacade.CreatureCacheData creatureCacheByIndex = SingletonAuto<BattleFacade>.instance.GetCreatureCacheByIndex(i);
			if (creatureCacheByIndex.Player.IsMe && creatureCacheByIndex.CreatureData.Index == heroIndex)
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
				int skillIndexBySlot = creature.CreatureData.GetSkillIndexBySlot(skillSlotIndex);
				if (skillIndexBySlot == 0)
				{
					VespaDebug.LogError(Define.LogPrefix + " SkillIndex is 0. heroIndex=" + heroIndex + " skillSlotIndex=" + skillSlotIndex);
				}
				else
				{
					creature.SkillController.CooltimeManager.RemoveCooltimeTimer(skillIndexBySlot);
					VespaDebug.LogError(Define.LogPrefix + " ClearCooltime. heroIndex=" + heroIndex + " skillSlotIndex=" + skillSlotIndex);
				}
			}
		});
	}
}

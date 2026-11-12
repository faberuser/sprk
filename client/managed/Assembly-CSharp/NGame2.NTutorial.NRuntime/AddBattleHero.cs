using System.Collections;
using NGame2.NAccount;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NTutorial;
using NShared;
using NShared.NEvent;
using NShared.NParty;
using NVespa.NSingleton;

namespace NGame2.NTutorial.NRuntime;

public class AddBattleHero : ITutorialRuntimeImmediate, ITutorialRuntime, IEventRuntime
{
	private RaidPartySetting raidWindow;

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
		int argInt3 = TutorialUtility.GetArgInt(tutorialData, 2);
		int argInt4 = TutorialUtility.GetArgInt(tutorialData, 3);
		raidWindow = SceneAutoBehaviour<RaidManagement>.instance.GetInstance<RaidPartySetting>();
		if (!(raidWindow == null))
		{
			MemberInfo memberInfo = new MemberInfo();
			memberInfo.AccountId = -1L;
			memberInfo.AvatarHeroIndex = 1;
			memberInfo.IsBattleReady = false;
			memberInfo.IsMaster = false;
			memberInfo.IsOnline = true;
			memberInfo.Nick = "Tutorial NPC";
			memberInfo.TeamLevel = 100;
			CachedHeroInfo cachedHeroInfo = new CachedHeroInfo
			{
				HeroIndex = argInt,
				Star = (sbyte)argInt2,
				Level = (short)argInt4,
				SkillLevel1 = (short)((argInt3 == 0 || argInt3 >= 1) ? argInt4 : 0),
				SkillLevel2 = (short)((argInt3 == 0 || argInt3 >= 2) ? argInt4 : 0),
				SkillLevel3 = (short)((argInt3 == 0 || argInt3 >= 3) ? argInt4 : 0),
				SkillLevel4 = (short)((argInt3 == 0 || argInt3 >= 4) ? argInt4 : 0),
				SkillLevel5 = (short)((argInt3 == 0 || argInt3 >= 5) ? argInt4 : 0)
			};
			memberInfo.DeckHeros.Add(cachedHeroInfo.HeroIndex, cachedHeroInfo);
			raidWindow.OnPartyMemberDeckHeroChangedForTutorial(memberInfo, cachedHeroInfo.HeroIndex, cachedHeroInfo);
			if (raidWindow.CreatureCreateInfoList != null)
			{
				raidWindow.CreatureCreateInfoList.Add(new BattleCreatureCreateInfo
				{
					HeroInfo = cachedHeroInfo,
					StatData = SceneAutoBehaviour<StatDataCache>.instance.GetCachedHeroStat(cachedHeroInfo),
					IsAi = true
				});
			}
		}
	}
}

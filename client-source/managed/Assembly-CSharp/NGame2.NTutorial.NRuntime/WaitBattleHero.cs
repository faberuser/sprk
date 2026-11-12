using System.Collections;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NTutorial;
using NShared;
using NShared.NEvent;
using NVespa.NSingleton;

namespace NGame2.NTutorial.NRuntime;

public class WaitBattleHero : ITutorialRuntime, IEventRuntime
{
	private RaidPartySetting raidWindow;

	bool ITutorialRuntime.OnEvent(IEvent triggerEvent)
	{
		return false;
	}

	IEnumerator ITutorialRuntime.Run(TutorialWindow window, TutorialData tutorialData)
	{
		window.EnableGameUi = true;
		int argInt = TutorialUtility.GetArgInt(tutorialData, 0);
		raidWindow = SceneAutoBehaviour<RaidManagement>.instance.GetInstance<RaidPartySetting>();
		if ((bool)raidWindow && raidWindow.IsOpen())
		{
			int maxHeroCount = raidWindow.RaidData.DeckCountPerPlayer;
			if (argInt < maxHeroCount)
			{
				maxHeroCount = argInt;
			}
			while (raidWindow.GetPartyMemberListCount() != maxHeroCount)
			{
				yield return null;
			}
			SetMyDeckHero();
			window.EnableGameUi = false;
		}
	}

	public void SetMyDeckHero()
	{
		List<Common.HeroPortraitAnim> allSquardHeroPortraitAnimList = raidWindow.GetAllSquardHeroPortraitAnimList();
		for (int i = 0; i < allSquardHeroPortraitAnimList.Count; i++)
		{
			CachedHeroInfo heroInfo = allSquardHeroPortraitAnimList[i].Component.HeroInfo;
			if (heroInfo != null)
			{
				CachedHeroInfo cachedHeroInfo = new CachedHeroInfo
				{
					HeroIndex = heroInfo.HeroIndex,
					Star = heroInfo.Star,
					Level = heroInfo.Level,
					SkillLevel1 = heroInfo.SkillLevel1,
					SkillLevel2 = heroInfo.SkillLevel2,
					SkillLevel3 = heroInfo.SkillLevel3,
					SkillLevel4 = heroInfo.SkillLevel4,
					SkillLevel5 = heroInfo.SkillLevel5
				};
				raidWindow.CreatureCreateInfoList.Add(new BattleCreatureCreateInfo
				{
					HeroInfo = cachedHeroInfo,
					StatData = SceneAutoBehaviour<StatDataCache>.instance.GetCachedHeroStat(cachedHeroInfo)
				});
				continue;
			}
			break;
		}
	}
}

using System.Collections;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NBattleContext;
using NGame2.NUI.NWindow.NTutorial;
using NShared;
using NShared.NEvent;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NTutorial.NRuntime;

public class InitBattleHero2 : ITutorialRuntimeImmediate, ITutorialRuntime, IEventRuntime
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
		BattlePlayerCreateInfo playerCreateInfo = new BattlePlayerCreateInfo
		{
			Id = GlobalAutoBehaviour<AccountManager>.instance.UserManager.Id,
			TeamType = TeamType.Left
		};
		List<BattleCreatureCreateInfo> list = new List<BattleCreatureCreateInfo>();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(tutorialData.Argument) / 4; i++)
		{
			int argInt = TutorialUtility.GetArgInt(tutorialData, i * 4);
			int argInt2 = TutorialUtility.GetArgInt(tutorialData, i * 4 + 1);
			int argInt3 = TutorialUtility.GetArgInt(tutorialData, i * 4 + 2);
			int argInt4 = TutorialUtility.GetArgInt(tutorialData, i * 4 + 3);
			argInt2 = ((argInt2 > 0) ? argInt2 : GameRoot.Instance.ConstantDataContainer.GetValueInt("HeroMaxStar"));
			argInt4 = ((argInt4 > 0) ? argInt4 : GameRoot.Instance.CreatureLevelDataContainer.MaxLevel);
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
			list.Add(new BattleCreatureCreateInfo
			{
				HeroInfo = cachedHeroInfo,
				StatData = SceneAutoBehaviour<StatDataCache>.instance.GetCachedHeroStat(cachedHeroInfo)
			});
		}
		PlayerEnterBattleLog playerEnterBattleLog = new PlayerEnterBattleLog();
		playerEnterBattleLog.PlayerCreateInfo = playerCreateInfo;
		playerEnterBattleLog.CreatureCreateInfos = list.ToArray();
		SingletonAuto<CustomCampaignContext>.instance.SetEnterPlayerBattleLog(playerEnterBattleLog);
	}
}

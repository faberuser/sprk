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

public class InitBattleHero : ITutorialRuntimeImmediate, ITutorialRuntime, IEventRuntime
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
		int argInt2 = TutorialUtility.GetArgInt(tutorialData, 1);
		int argInt3 = TutorialUtility.GetArgInt(tutorialData, 2);
		int argInt4 = TutorialUtility.GetArgInt(tutorialData, 3);
		int[] obj = new int[4] { argInt, argInt2, argInt3, argInt4 };
		int argInt5 = TutorialUtility.GetArgInt(tutorialData, 4);
		int argInt6 = TutorialUtility.GetArgInt(tutorialData, 5);
		int argInt7 = TutorialUtility.GetArgInt(tutorialData, 6);
		argInt5 = ((argInt5 > 0) ? argInt5 : GameRoot.Instance.ConstantDataContainer.GetValueInt("HeroMaxStar"));
		argInt7 = ((argInt7 > 0) ? argInt7 : GameRoot.Instance.CreatureLevelDataContainer.MaxLevel);
		BattlePlayerCreateInfo playerCreateInfo = new BattlePlayerCreateInfo
		{
			Id = GlobalAutoBehaviour<AccountManager>.instance.UserManager.Id,
			TeamType = TeamType.Left
		};
		List<BattleCreatureCreateInfo> list = new List<BattleCreatureCreateInfo>();
		int[] array = obj;
		foreach (int heroIndex in array)
		{
			CachedHeroInfo cachedHeroInfo = new CachedHeroInfo
			{
				HeroIndex = heroIndex,
				Star = (sbyte)argInt5,
				Level = (short)argInt7,
				SkillLevel1 = (short)((argInt6 == 0 || argInt6 >= 1) ? argInt7 : 0),
				SkillLevel2 = (short)((argInt6 == 0 || argInt6 >= 2) ? argInt7 : 0),
				SkillLevel3 = (short)((argInt6 == 0 || argInt6 >= 3) ? argInt7 : 0),
				SkillLevel4 = (short)((argInt6 == 0 || argInt6 >= 4) ? argInt7 : 0),
				SkillLevel5 = (short)((argInt6 == 0 || argInt6 >= 5) ? argInt7 : 0)
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

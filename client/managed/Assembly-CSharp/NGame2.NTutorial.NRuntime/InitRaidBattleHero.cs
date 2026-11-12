using System.Collections;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NBattleContext;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NTutorial;
using NShared;
using NShared.NEvent;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NTutorial.NRuntime;

public class InitRaidBattleHero : ITutorialRuntimeImmediate, ITutorialRuntime, IEventRuntime
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
		RaidPartySetting instance = SceneAutoBehaviour<RaidManagement>.instance.GetInstance<RaidPartySetting>();
		List<BattleCreatureCreateInfo> creatureCreateInfoList = instance.CreatureCreateInfoList;
		if (creatureCreateInfoList == null || creatureCreateInfoList.Count <= 0)
		{
			return;
		}
		for (int i = 0; i < creatureCreateInfoList.Count; i++)
		{
			if (i > instance.RaidData.DeckCount)
			{
				creatureCreateInfoList.RemoveAt(i);
			}
		}
		BattlePlayerCreateInfo playerCreateInfo = new BattlePlayerCreateInfo
		{
			Id = GlobalAutoBehaviour<AccountManager>.instance.UserManager.Id,
			TeamType = TeamType.Left
		};
		PlayerEnterBattleLog playerEnterBattleLog = new PlayerEnterBattleLog();
		playerEnterBattleLog.PlayerCreateInfo = playerCreateInfo;
		playerEnterBattleLog.CreatureCreateInfos = creatureCreateInfoList.ToArray();
		SingletonAuto<CustomCampaignContext>.instance.SetEnterPlayerBattleLog(playerEnterBattleLog);
	}
}

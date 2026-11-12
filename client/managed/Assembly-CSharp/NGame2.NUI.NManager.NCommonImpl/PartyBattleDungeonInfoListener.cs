using System;
using NGame2.NBattleContext;
using NGame2.NDispatchBattle;
using NGame2.NUI.NWindow;
using NShared;

namespace NGame2.NUI.NManager.NCommonImpl;

public class PartyBattleDungeonInfoListener : PartyBattleDungeonInfo.Listener
{
	public EnterDungeonData EnterDungeon;

	public BossMonsterInfo BossMonsterInfo;

	public Action OnResetEventDungeon;

	public Action<EnterDungeonData> OnReadyBattle;

	public Action<EnterDungeonData> OnMoveEnter;

	public Action<EnterDungeonData> OnClickPartyPlayPopup;

	void PartyBattleDungeonInfo.Listener.OnReadyBattle(EnterDungeonData enterDungeon)
	{
		Vespa_System_Action_Extension_Method.fire(OnReadyBattle, enterDungeon);
	}

	void PartyBattleDungeonInfo.Listener.ReOpen()
	{
	}

	void PartyBattleDungeonInfo.Listener.OnResetEventDungeon()
	{
		Vespa_System_Action_Extension_Method.fire(OnResetEventDungeon);
	}

	void PartyBattleDungeonInfo.Listener.ViewRewardInfo(EnterDungeonData enterDungeon)
	{
		int dungeonRewardIndex = enterDungeon.DungeonRewardIndex;
		RewardData data = GameRoot.Instance.RewardDataContainer.GetData(dungeonRewardIndex);
		if (data != null)
		{
			CampaignDungeonData data2 = GameRoot.Instance.CampaignDungeonDataContainer.GetData(EnterDungeon.Chapter, EnterDungeon.OriginalDungeon);
			if (data2 != null)
			{
				DungeonRewardInfo.OpenGlobal(data, data2.BattleType);
			}
		}
	}

	void PartyBattleDungeonInfo.Listener.ViewBossInfo(HeroInfo heroInfo)
	{
		if ((bool)BossMonsterInfo)
		{
			BossMonsterInfo.Open(heroInfo);
		}
	}

	void PartyBattleDungeonInfo.Listener.OnClickPartyPlayPopup(EnterDungeonData enterDungeon)
	{
		Vespa_System_Action_Extension_Method.fire(OnClickPartyPlayPopup, enterDungeon);
	}

	void PartyBattleDungeonInfo.Listener.OnMoveEnter(EnterDungeonData enterDungeon)
	{
		Vespa_System_Action_Extension_Method.fire(OnMoveEnter, enterDungeon);
	}

	void PartyBattleDungeonInfo.Listener.OnClickDispatch(EnterDungeonData enterDungeon)
	{
		ContentUIDispatchBattle.OpenGlobalChapter(enterDungeon);
	}
}

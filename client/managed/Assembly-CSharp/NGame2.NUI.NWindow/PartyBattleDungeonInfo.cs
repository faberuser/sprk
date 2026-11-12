using System.Collections;
using System.Collections.Generic;
using NGame2.NBattleContext;
using NGame2.NUI.NComponent2.NPortrait;
using NGame2.NUI.NWindow.NBattle;
using NShared;
using NVespa;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NUtil;
using UnityEngine;

namespace NGame2.NUI.NWindow;

[UIEvent(AllowDuringPatchDownloading = true)]
public class PartyBattleDungeonInfo : BaseDungeonInfo, MonsterComponent.IListener
{
	public interface Listener
	{
		void OnReadyBattle(EnterDungeonData enterDungeon);

		void OnMoveEnter(EnterDungeonData enterDungeon);

		void OnResetEventDungeon();

		void OnClickPartyPlayPopup(EnterDungeonData enterDungeon);

		void OnClickDispatch(EnterDungeonData enterDungeon);

		void ReOpen();

		void ViewRewardInfo(EnterDungeonData enterDungeon);

		void ViewBossInfo(HeroInfo heroInfo);
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Dungeon/PartyBattleDungeonInfo";

	public UISimpleGrid BossMonsterGrid;

	private MonsterComponent _bossMonsterComp;

	public UIButton Button_ViewReward;

	private Listener _listener;

	public override void OnOpen<T>(EnterDungeonData enterDungeon, T listener, bool notCurrentNode = false)
	{
		_listener = listener as Listener;
	}

	public override void Refresh()
	{
		RefreshContents();
		AutoNextBattle();
	}

	public override void AutoNextBattle()
	{
		if (GameOptionData.EditorCanAutoPlay)
		{
			SingletonUpdater<Executer>.instance.DelayRun(3f, this, OnClickEnterBattleParty);
		}
		if (NGame2.NUI.NWindow.NBattle.AutoNextBattle.IsEnabled())
		{
			if (GetDungeonDiffcultyComp() != null)
			{
				DungeonDifficultyType dungeonDifficulty = NGame2.NUI.NWindow.NBattle.AutoNextBattle.instance.DungeonDifficulty;
				GetDungeonDiffcultyComp().SelectDifficuly(dungeonDifficulty);
			}
			VespaCoroutine.Start(CoRunAutoNextBattle());
		}
	}

	private IEnumerator CoRunAutoNextBattle()
	{
		yield return new WaitForSeconds(NGame2.NUI.NWindow.NBattle.AutoNextBattle.AutoCampaignDelayTime);
		if (NGame2.NUI.NWindow.NBattle.AutoNextBattle.IsEnabled() && IsOpen() && _listener != null)
		{
			_listener.OnReadyBattle(base.EnterDungeon);
		}
	}

	public override void Reset()
	{
		base.Reset();
		InfoComponents.Reset();
		ClearBossMonsterComponent();
	}

	public override void OnSystemBackButton()
	{
		NGame2.NUI.NWindow.NBattle.AutoNextBattle.StopWithClickButton();
		Close();
	}

	public override void OnClosing()
	{
		base.OnClosing();
		InfoComponents.Reset();
		ClearBossMonsterComponent();
	}

	public void RefreshContents()
	{
		base.Refresh();
		SetupRewardButton();
	}

	public override List<MonsterHeroInfo> GetDungeonMonsterInfos()
	{
		return GetNormalMonsterInfos();
	}

	public override void SetupMonsterInfos()
	{
		base.SetupMonsterInfos();
		ClearBossMonsterComponent();
		_bossMonsterComp = AddGridBossMonster(GetBossMonsterInfo(), BossMonsterGrid);
		if (_bossMonsterComp != null)
		{
			UIHelper.RepositionGrid(BossMonsterGrid);
		}
	}

	private void ClearBossMonsterComponent()
	{
		FactoryBase.SafeReleaseComponent(ref _bossMonsterComp);
	}

	private void SetupRewardButton()
	{
		bool flag = !base.EnterDungeon.IsScenarioReplay;
		UIHelper.EnableButton(Button_ViewReward, flag);
		if (flag)
		{
			RewardData data = GameRoot.Instance.RewardDataContainer.GetData(base.EnterDungeon.DungeonRewardIndex);
			bool enable = data != null && !data.Item1Code.IsNullOrEmpty();
			UIHelper.EnableGameObject(Button_ViewReward, enable);
		}
	}

	public void OnButtonResetEventDungeon()
	{
		NGame2.NUI.NWindow.NBattle.AutoNextBattle.StopWithClickButton();
		if (_listener != null)
		{
			_listener.OnResetEventDungeon();
		}
	}

	public void OnClickViewRewardInfo()
	{
		NGame2.NUI.NWindow.NBattle.AutoNextBattle.StopWithClickButton();
		if (_listener != null)
		{
			_listener.ViewRewardInfo(base.EnterDungeon);
		}
	}

	public void OnClickEnterBattleParty()
	{
		NGame2.NUI.NWindow.NBattle.AutoNextBattle.StopWithClickButton();
		if (_listener != null)
		{
			_listener.OnReadyBattle(base.EnterDungeon);
		}
	}

	public void OnClickMoveEnter()
	{
		NGame2.NUI.NWindow.NBattle.AutoNextBattle.StopWithClickButton();
		if (_listener != null)
		{
			_listener.OnMoveEnter(base.EnterDungeon);
		}
	}

	public void OnClickDispatch()
	{
		if (_listener != null)
		{
			_listener.OnClickDispatch(base.EnterDungeon);
		}
	}

	public void OnClickPartyPlayPopup()
	{
		NGame2.NUI.NWindow.NBattle.AutoNextBattle.StopWithClickButton();
		if (CanEnterPartyPlay() && _listener != null)
		{
			_listener.OnClickPartyPlayPopup(base.EnterDungeon);
		}
	}

	void MonsterComponent.IListener.OnClickViewInfo(HeroInfo heroInfo)
	{
		if (_listener != null)
		{
			_listener.ViewBossInfo(heroInfo);
		}
	}
}

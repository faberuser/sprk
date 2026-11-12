using System.Collections;
using System.Collections.Generic;
using NGame2.NBattleContext;
using NGame2.NScene.NPreloader;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NWindow.NBattle;
using NShared;
using NVespa;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using NVespa.NUtil;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class BattleDungeonInfo : BaseDungeonInfo, LobbyTopmostMenu.ICustomStaminaObserver
{
	public interface Listener
	{
		void OnReadyBattle(EnterDungeonData enterDungeon);

		void OnMoveEnter(EnterDungeonData enterDungeon);

		void OnResetEventDungeon();

		void OnClickPartyPlayPopup(EnterDungeonData enterDungeon);

		void OnClickDispatch(EnterDungeonData enterDungeon);

		bool OnCloseRequest(BattleDungeonInfo closeThis);

		int GetDungeonRemainTime(EnterDungeonData enterDungeon);
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Dungeon/BattleDungeonInfo";

	public UIScrollGrid RewardItemGrid;

	protected List<Component> _itemIconList = new List<Component>();

	public GameObject MiniGuideButton;

	private Listener _listener;

	private StaminaType _staminaType;

	StaminaType LobbyTopmostMenu.ICustomStaminaObserver.StaminaType => _staminaType;

	protected override int GetDungeonRemainTime()
	{
		if (_listener == null)
		{
			return -1;
		}
		return _listener.GetDungeonRemainTime(base.EnterDungeon);
	}

	public override void OnOpen<T>(EnterDungeonData enterDungeon, T listener, bool notCurrentNode = false)
	{
		_listener = listener as Listener;
		SetupEnterDungeonData(enterDungeon);
	}

	public override void Refresh()
	{
		RefreshContents();
		AutoNextBattle();
	}

	public override void SetupMonsterInfos()
	{
		InfoComponents.ReleaseMonsterComponents();
		UIHelper.EnableGameObject(InfoComponents.MonsterRoot, enable: true);
		int questMonsterIndex = base.EnterDungeon.QuestDungeon?.MonsterIndex ?? 0;
		List<Component> list = UIHelper.CreateMonsterPortraitList(InfoComponents.Grid_Monsters, base.EnterDungeon, questMonsterIndex);
		if (__Extension_Method_ICollection__.COUNT(list) > 0)
		{
			InfoComponents.AddMonsterComponents(list);
		}
	}

	public override void AutoNextBattle()
	{
		if (GameOptionData.EditorCanAutoPlay)
		{
			SingletonUpdater<Executer>.instance.DelayRun(3f, this, OnButtonReadyBattle);
		}
		if (NGame2.NUI.NWindow.NBattle.AutoNextBattle.IsEnabled())
		{
			if (GetDungeonDiffcultyComp() != null)
			{
				DungeonDifficultyType dungeonDifficulty = NGame2.NUI.NWindow.NBattle.AutoNextBattle.instance.DungeonDifficulty;
				GetDungeonDiffcultyComp().SelectDifficuly(dungeonDifficulty);
				RefreshContents();
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

	public void RefreshContents()
	{
		base.Refresh();
		SetupStamina();
		SetupDropItems();
		SetMiniGuideButton(WindowGuideIndex);
	}

	public override void ReserveCache(UiPreloader preloader)
	{
		base.ReserveCache(preloader);
	}

	private void Clear()
	{
		InfoComponents.Reset();
		ClearDropItem();
	}

	protected void ClearDropItem()
	{
		FactoryBase.ReleaseAndClearList(_itemIconList);
	}

	public override void Reset()
	{
		base.Reset();
		Clear();
	}

	public override void OnClosing()
	{
		base.OnClosing();
		Clear();
	}

	private void SetupStamina()
	{
		_staminaType = base.EnterDungeon.LinkedDungeonData.ReqStaminaType;
	}

	private void SetMiniGuideButton(int miniGuideIndex)
	{
		UIHelper.EnableGameObject(MiniGuideButton, miniGuideIndex != 0);
	}

	public virtual void SetupDropItems()
	{
		ClearDropItem();
		if (!base.EnterDungeon.IsScenarioReplay)
		{
			CampaignDungeonData data = GameRoot.Instance.CampaignDungeonDataContainer.GetData(base.EnterDungeon.Chapter, base.EnterDungeon.LinkedDungeon);
			bool includeCurrency = data.BattleType == BattleType.EventDungeon;
			List<Component> collection = BaseDungeonInfo.AddRewardItemToGrid(GetRewardData(), RewardItemGrid, includeCurrency, null, data.BattleType, _showHeroReward);
			_itemIconList.AddRange(collection);
			if (data.BattleType == BattleType.EventDungeon)
			{
				List<Component> collection2 = AddEventDungeonPointReward(RewardItemGrid, base.EnterDungeon.Chapter, base.EnterDungeon.LinkedDungeon, base.EnterDungeon.Difficulty);
				_itemIconList.AddRange(collection2);
			}
		}
	}

	protected virtual bool PreprocessReadyBattle()
	{
		return true;
	}

	[UIEvent(AllowDuringPatchDownloading = true)]
	public void OnButtonReadyBattle()
	{
		if (!NGame2.NUI.NWindow.NBattle.AutoNextBattle.IsEnabled() && PreprocessReadyBattle() && _listener != null)
		{
			_listener.OnReadyBattle(base.EnterDungeon);
		}
	}

	public void OnButtonResetEventDungeon()
	{
		if (_listener != null)
		{
			_listener.OnResetEventDungeon();
		}
	}

	public void OnButtonClose()
	{
		NGame2.NUI.NWindow.NBattle.AutoNextBattle.StopWithClickButton();
		if (_listener == null || _listener.OnCloseRequest(this))
		{
			Close();
		}
	}

	public override void OnSystemBackButton()
	{
		OnButtonClose();
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
	}

	[UIEvent(AllowDuringPatchDownloading = true)]
	public void OnClickMoveAndEnter()
	{
		NGame2.NUI.NWindow.NBattle.AutoNextBattle.StopWithClickButton();
		if (_listener != null)
		{
			_listener.OnMoveEnter(base.EnterDungeon);
		}
	}

	public void OnClickButton_MiniGuideMenu()
	{
		NGame2.NUI.NWindow.NBattle.AutoNextBattle.StopWithClickButton();
		if (SceneAutoBehaviour<LobbyManagement>.instance.GetInstance<LobbyTopmostMenu>().CurrentMiniGuideData != null)
		{
			int[] gameGuideIndex = SceneAutoBehaviour<LobbyManagement>.instance.GetInstance<LobbyTopmostMenu>().CurrentMiniGuideData.GameGuideIndex;
			SceneAutoBehaviour<LobbyManagement>.instance.GetInstance<MiniGuidePopup>().Open(gameGuideIndex);
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

	public void OnClickDispatch()
	{
		if (_listener != null)
		{
			_listener.OnClickDispatch(base.EnterDungeon);
		}
	}
}

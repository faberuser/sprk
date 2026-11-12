using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NBattleContext;
using NGame2.NUI.NComponent2.NTower;
using NGame2.NUI.NHelper;
using NGame2.NUI.NHelper.NTower;
using NGame2.NUI.NManager.NCommonImpl;
using NShared;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using NVespa.NUtil;
using UnityEngine;

namespace NGame2.NUI.NWindow.NTower;

public class ChallengeTowerWindow : BattleDungeonInfo, OrdealTowerFloorComponent.Listener, IUserObserver<UserManager.UserStatInfo>, IUserObserver, ISweepDungeonWindow
{
	public new const string PrefabPath = "Resources/Prefab2/Window/Tower/ChallengeTowerWindow";

	private BattleDungeonInfoListener _listener;

	public UILabel PageLabel;

	public UILabel RemainResetTimeLabel;

	private List<OrdealTowerFloorComponent> _floorComponentList = new List<OrdealTowerFloorComponent>();

	public UIScrollGrid TowerGrid;

	public UIScrollGrid ItemGrid;

	public UILabel SweepFloorLabel;

	public UIButton SweepButton;

	private Action<TowerFloorData> _onChangeFloor;

	private float _lastUpdateRemainResetTime;

	public TowerData TowerData { get; private set; }

	public TowerInfo TowerInfo { get; private set; }

	public TowerFloorData FloorData { get; private set; }

	public int ChapterIndex { get; private set; }

	public int DungeonIndex { get; private set; }

	public int FloorIndex { get; private set; }

	public int MaxFloor { get; private set; }

	bool IUserObserver.IsGlobal()
	{
		return false;
	}

	void IUserObserver<UserManager.UserStatInfo>.OnChanged(UserManager.UserStatInfo changedInfo)
	{
		if (changedInfo.UserStatType == UserManager.UserStatType.Stamina && changedInfo.StaminaType == StaminaType.ChallengeTowerKey)
		{
			SetSweepButton();
		}
	}

	public override string GetDungeonName()
	{
		return FloorIndex.ToString();
	}

	public void Init(TowerData towerData, TowerInfo towerInfo, int showFloor, BattleDungeonInfoListener listener)
	{
		_listener = listener;
		TowerData = towerData;
		TowerInfo = towerInfo;
		MaxFloor = BaseTower.GetMaxFloor(towerData.TowerIndex);
		if (showFloor == 0)
		{
			showFloor = GetCurrentFloor();
		}
		RefreshFloorData(showFloor);
		CreateFloorComponent();
		SetTopClearedFloor();
		SetDungeonInfo();
		RefreshFloorComponent();
		SetFloorTitle();
		UpdateRemainResetTimeLabel();
	}

	public void RefreshFloorData(int floor)
	{
		TowerFloorData data = GameRoot.Instance.TowerFloorDataContainer.GetData(TowerData.TowerIndex, floor);
		if (data == null)
		{
			VespaDebug.LogError("Floor data not found : towerIndex=" + TowerData.TowerIndex + " floor=" + floor);
			return;
		}
		FloorIndex = floor;
		RefreshFloorData(data);
	}

	public void RefreshFloorData(TowerFloorData floorData)
	{
		if (floorData != null)
		{
			ChapterIndex = floorData.ChapterIndex;
			DungeonIndex = floorData.DungeonIndex;
			FloorData = floorData;
			Vespa_System_Action_Extension_Method.fire(_onChangeFloor, floorData);
		}
	}

	public void SetOnChangeFloorCallback(Action<TowerFloorData> onChangeFloor)
	{
		_onChangeFloor = onChangeFloor;
	}

	private void SetDungeonInfo()
	{
		EnterDungeonData enterDungeonData = new EnterDungeonData(ChapterIndex, DungeonIndex, DungeonIndex, NShared.DungeonDifficultyType.Easy);
		enterDungeonData.ShowDuplicated = true;
		_listener.EnterDungeon = enterDungeonData;
		OnOpen(_listener.EnterDungeon, _listener);
		Refresh();
		bool enable = FloorIndex <= GetCurrentFloor();
		UIHelper.EnableButton(InfoComponents.Button_BattleReady, enable);
		SetSweepButton();
	}

	private void SetSweepButton()
	{
		bool flag = FloorIndex <= GetCurrentFloor();
		CampaignDungeonData data = GameRoot.Instance.CampaignDungeonDataContainer.GetData(ChapterIndex, DungeonIndex);
		bool flag2 = SweepHelper.IsRemainStamina(data);
		bool flag3 = SweepHelper.IsPossibleSweepStep(data);
		UIHelper.EnableButton(SweepButton, flag2 && flag3 && flag, grayscaleIfDisable: true);
	}

	public override void Reset()
	{
		base.Reset();
		FactoryBase.ReleaseAndClearList(_floorComponentList);
		_onChangeFloor = null;
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		if (Time.time - _lastUpdateRemainResetTime > 0.1f)
		{
			_lastUpdateRemainResetTime = Time.time;
			UpdateRemainResetTimeLabel();
		}
	}

	private void SetFloorTitle()
	{
		UIHelper.SetLabelText(PageLabel, FloorIndex);
	}

	public void SetTopClearedFloor()
	{
		string text = "-";
		CampaignDungeonData data = GameRoot.Instance.CampaignDungeonDataContainer.GetData(ChapterIndex, DungeonIndex);
		if (data != null)
		{
			int topClearDungeonStep = SweepHelper.GetTopClearDungeonStep(data);
			long floor = topClearDungeonStep;
			text = SweepHelper.GetStepLocalText(data.SweepDungeonType, floor);
		}
		UIHelper.SetLabelText(SweepFloorLabel, text);
	}

	public override void SetupDropItems()
	{
		ClearDropItem();
		TowerHelper.SetupReward(FloorData, GetCompletedFloor(), ItemGrid, _itemIconList, isShowEventDungeonPoint3: true);
	}

	private void UpdateRemainResetTimeLabel()
	{
		TowerHelper.SetupRemainResetTimeLabel(RemainResetTimeLabel, TowerData.TowerIndex);
	}

	public override void AutoNextBattle()
	{
		if (GameOptionData.EditorCanAutoPlay)
		{
			SingletonUpdater<Executer>.instance.DelayRun(3f, this, base.OnButtonReadyBattle);
		}
	}

	public void RefreshBySweep()
	{
		TowerInfo = GlobalAutoBehaviour<AccountManager>.instance.TowerManager.Get(TowerData.TowerIndex);
		int floor = TowerInfo.CompletedFloor;
		_floorComponentList.ForEach(delegate(OrdealTowerFloorComponent e)
		{
			e.SetCompletedFloor(floor);
		});
		int currentFloor = GetCurrentFloor();
		RefreshFloorData(currentFloor);
		SetDungeonInfo();
		RefreshFloorComponent();
		SetFloorTitle();
		ScrollToFloorComponent(currentFloor);
	}

	protected override bool PreprocessReadyBattle()
	{
		if (FloorData.ReqClearedChapterIndex != null && FloorData.ReqClearedChapterIndex.Length != 0)
		{
			_ = FloorData.ReqClearedChapterDifficulty;
			int[] reqClearedChapterIndex = FloorData.ReqClearedChapterIndex;
			foreach (int num in reqClearedChapterIndex)
			{
				if (!GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.ChapterCleared(num))
				{
					MessagePopup.OpenError(LocalizationManager.GetString(Tower.TOWER_REQ_CHAPTER_NOT_CLEARED, num));
					return false;
				}
			}
		}
		return true;
	}

	public void OnClickSweep()
	{
		TowerFloorData data = GameRoot.Instance.TowerFloorDataContainer.GetData(TowerData.TowerIndex, GetCurrentFloor());
		CampaignDungeonData data2 = GameRoot.Instance.CampaignDungeonDataContainer.GetData(ChapterIndex, data.DungeonIndex);
		if (data2 != null)
		{
			SweepHelper.OpenSweepPopup(data2, this);
		}
	}

	private int GetCompletedFloor()
	{
		if (TowerInfo == null)
		{
			return 0;
		}
		return TowerInfo.CompletedFloor;
	}

	private int GetCurrentFloor()
	{
		return Mathf.Min(GetCompletedFloor() + 1, MaxFloor);
	}

	private void CreateFloorComponent()
	{
		for (int num = MaxFloor; num >= 1; num--)
		{
			OrdealTowerFloorComponent ordealTowerFloorComponent = TowerGrid.AddGridItem(OrdealTowerFloorComponent.ChallengePrefabAuto);
			ordealTowerFloorComponent.Open(this, num, GetCompletedFloor(), TowerData.FloorCountPerPage, num == MaxFloor);
			_floorComponentList.Add(ordealTowerFloorComponent);
		}
		TowerGrid.RepositionGrid();
		ScrollToFloorComponent(FloorIndex);
	}

	private void RefreshFloorComponent()
	{
		_floorComponentList.ForEach(delegate(OrdealTowerFloorComponent e)
		{
			e.RefreshSelectionChallenge(FloorIndex);
		});
	}

	private void ScrollToFloorComponent(int floorIndex)
	{
		OrdealTowerFloorComponent ordealTowerFloorComponent = _floorComponentList.Find((OrdealTowerFloorComponent e) => e.Floor == floorIndex);
		if (ordealTowerFloorComponent != null)
		{
			TowerGrid.ScrollTo(ordealTowerFloorComponent, 0f, 0f);
		}
	}

	void OrdealTowerFloorComponent.Listener.OnClickFloor(OrdealTowerFloorComponent component)
	{
		RefreshFloorData(component.Floor);
		SetDungeonInfo();
		RefreshFloorComponent();
		SetFloorTitle();
	}

	void OrdealTowerFloorComponent.Listener.OnDragComponent()
	{
	}

	void OrdealTowerFloorComponent.Listener.OnDragComponentEnd()
	{
	}
}

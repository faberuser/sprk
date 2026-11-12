using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NBattleContext;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NComponent2.NTower;
using NGame2.NUI.NHelper;
using NGame2.NUI.NHelper.NTower;
using NGame2.NUI.NManager;
using NGame2.NUI.NManager.NCommonImpl;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using NVespa.NUtil;
using UnityEngine;

namespace NGame2.NUI.NWindow.NTower;

public class MazeTowerWindow : BattleDungeonInfo, OrdealTowerFloorComponent.Listener, IUserObserver<UserManager.UserStatInfo>, IUserObserver, ISweepDungeonWindow
{
	public new const string PrefabPath = "Resources/Prefab2/Window/Tower/MazeTowerWindow";

	public UIBaseGrid ItemGrid;

	public UIBaseGrid FloorSkillIconGrid;

	public UIBaseGrid TowerGrid;

	public UILabel TowerName;

	public UILabel RemainResetTimeLabel;

	public UILabel SweepFloorLabel;

	public UIButton SweepButton;

	private BattleDungeonInfoListener _listener2;

	private CachedAccountInfo[] _npcInfos;

	private List<OrdealTowerFloorComponent> _floorComponentList = new List<OrdealTowerFloorComponent>();

	private List<ItemIconComponent> _itemComponentList = new List<ItemIconComponent>();

	private List<SkillIconComponent> _skillIconComponentList = new List<SkillIconComponent>();

	private Action<TowerFloorData> _onChangeFloor;

	private float _lastUpdateRemainResetTime;

	public TowerData TowerData { get; private set; }

	public TowerInfo TowerInfo { get; private set; }

	public TowerFloorData FloorData { get; private set; }

	public MazeInfoData _mazeInfoData { get; set; }

	public int ChapterIndex => GetChapterIndex();

	public int DungeonIndex => GetDungeonIndex();

	public int FloorIndex { get; private set; }

	public int MaxFloor { get; private set; }

	public int StartFloor { get; private set; }

	public int NpcIndex => FloorIndex - 1;

	bool IUserObserver.IsGlobal()
	{
		return false;
	}

	void IUserObserver<UserManager.UserStatInfo>.OnChanged(UserManager.UserStatInfo changedInfo)
	{
		if (changedInfo.UserStatType == UserManager.UserStatType.Stamina && changedInfo.StaminaType == StaminaType.MazeKey)
		{
			SetSweepButton();
		}
	}

	public void Init(TowerData towerData, TowerInfo towerInfo, CachedAccountInfo[] npcInfos, int showFloor, BattleDungeonInfoListener listener)
	{
		_listener2 = listener;
		TowerData = towerData;
		TowerInfo = towerInfo;
		MaxFloor = BaseTower.GetMaxFloor(towerData.TowerIndex);
		_npcInfos = npcInfos;
		_mazeInfoData = GameRoot.Instance.MazeInfoDataContainer.GetData(towerData.TowerIndex);
		if (showFloor == 0)
		{
			showFloor = GetCurrentFloor();
		}
		InitStartFloor(showFloor);
		RefreshFloorData(showFloor);
		CreateFloorComponent();
		SetTopClearedFloor();
		SetDungeonInfo();
		SetupFloorSkill();
		RefreshFloorComponent();
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
			FloorData = floorData;
			Vespa_System_Action_Extension_Method.fire(_onChangeFloor, floorData);
		}
	}

	public int GetChapterIndex()
	{
		if (FloorData == null)
		{
			return 0;
		}
		return FloorData.ChapterIndex;
	}

	public int GetDungeonIndex()
	{
		return TowerHelper.GetFloorDungeonIndex(FloorData);
	}

	public void SetOnChangeFloorCallback(Action<TowerFloorData> onChangeFloor)
	{
		_onChangeFloor = onChangeFloor;
	}

	private void SetDungeonInfo()
	{
		if (_mazeInfoData != null)
		{
			string stringDirect = LocalizationManager.GetStringDirect(_mazeInfoData.TowerNameKey);
			string stringDirect2 = LocalizationManager.GetStringDirect("MAZE_FLOOR", FloorIndex);
			string text = $"{stringDirect} - {stringDirect2}";
			UIHelper.SetLabelText(TowerName, text);
		}
		bool enable = FloorIndex <= GetCurrentFloor();
		UIHelper.EnableButton(InfoComponents.Button_BattleReady, enable);
		SetSweepButton();
		EnterDungeonData enterDungeonData = new EnterDungeonData(ChapterIndex, DungeonIndex, DungeonIndex, DungeonDifficultyType.Easy);
		if (enterDungeonData != null)
		{
			enterDungeonData.ShowDuplicated = true;
			_listener2.EnterDungeon = enterDungeonData;
			OnOpen(_listener2.EnterDungeon, _listener2);
			Refresh();
		}
	}

	private void SetSweepButton()
	{
		CampaignDungeonData data = GameRoot.Instance.CampaignDungeonDataContainer.GetData(ChapterIndex, DungeonIndex);
		bool flag = FloorIndex <= GetCurrentFloor();
		bool flag2 = SweepHelper.IsRemainStamina(data);
		bool flag3 = SweepHelper.IsPossibleSweepStep(data);
		UIHelper.EnableButton(SweepButton, flag2 && flag3 && flag, grayscaleIfDisable: true);
	}

	public override void SetupMonsterInfos()
	{
		CachedAccountInfo cachedAccountInfo = null;
		if (_npcInfos != null && _npcInfos.Length > NpcIndex)
		{
			cachedAccountInfo = _npcInfos[NpcIndex];
		}
		if (cachedAccountInfo == null)
		{
			base.SetupMonsterInfos();
			return;
		}
		foreach (KeyValuePair<int, CachedHeroInfo> heroInfo in cachedAccountInfo.HeroInfos)
		{
			_ = heroInfo.Key;
			CachedHeroInfo value = heroInfo.Value;
			HeroPortraitComponent heroPortraitComponent = UIHelper.AddGridItem(InfoComponents.Grid_Monsters, HeroPortraitComponent.ShortPrefabAuto);
			if (!(heroPortraitComponent == null) && heroPortraitComponent.Open(value, null))
			{
				InfoComponents.AddMonsterComponent(heroPortraitComponent);
			}
		}
		UIHelper.RepositionGrid(InfoComponents.Grid_Monsters);
	}

	public override void Reset()
	{
		base.Reset();
		FactoryBase.ReleaseAndClearList(_floorComponentList);
		FactoryBase.ReleaseAndClearList(_skillIconComponentList);
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

	private void UpdateRemainResetTimeLabel()
	{
		TowerHelper.SetupRemainResetTimeLabel(RemainResetTimeLabel, TowerData.TowerIndex);
	}

	public override void SetupDropItems()
	{
		ClearDropItem();
		TowerHelper.SetupReward(FloorData, GetCompletedFloor(), ItemGrid, _itemIconList);
	}

	private void SetupFloorSkill()
	{
		FactoryBase.ReleaseAndClearList(_skillIconComponentList);
		TowerHelper.SetupFloorSkillIcons(FloorData, FloorSkillIconGrid, _skillIconComponentList);
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

	private void InitStartFloor(int showFloor)
	{
		StartFloor = TowerWindow.GetStartFloor(Mathf.Min((showFloor > 0) ? showFloor : GetCurrentFloor(), MaxFloor), TowerData.FloorCountPerPage);
	}

	private void CreateFloorComponent()
	{
		for (int i = 1; i < MaxFloor + 1; i++)
		{
			OrdealTowerFloorComponent ordealTowerFloorComponent = TowerGrid.AddGridItem(OrdealTowerFloorComponent.ChallengePrefabAuto);
			ordealTowerFloorComponent.Open(this, i, GetCompletedFloor(), TowerData.FloorCountPerPage, isMaxFloor: false, isUnderGround: true);
			_floorComponentList.Add(ordealTowerFloorComponent);
		}
		TowerGrid.RepositionGrid();
		OrdealTowerFloorComponent ordealTowerFloorComponent2 = _floorComponentList.Find((OrdealTowerFloorComponent e) => e.Floor == FloorIndex);
		if (ordealTowerFloorComponent2 != null)
		{
			UIScrollView componentInParent = TowerGrid.GetComponentInParent<UIScrollView>();
			if (componentInParent != null && componentInParent.canMoveVertically)
			{
				TowerGrid.ScrollTo(ordealTowerFloorComponent2, 0f, 0f);
			}
		}
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

	private void RefreshFloorComponent()
	{
		_floorComponentList.ForEach(delegate(OrdealTowerFloorComponent e)
		{
			e.RefreshSelectionMaze(FloorIndex);
		});
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
		SetupFloorSkill();
		RefreshFloorComponent();
		SceneAutoBehaviour<TowerManagement>.instance.RefreshTowerClearGauage();
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

	void OrdealTowerFloorComponent.Listener.OnClickFloor(OrdealTowerFloorComponent component)
	{
		RefreshFloorData(component.Floor);
		SetDungeonInfo();
		SetupFloorSkill();
		RefreshFloorComponent();
	}

	void OrdealTowerFloorComponent.Listener.OnDragComponent()
	{
	}

	void OrdealTowerFloorComponent.Listener.OnDragComponentEnd()
	{
	}
}

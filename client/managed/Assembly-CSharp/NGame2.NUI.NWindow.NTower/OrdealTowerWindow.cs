using System;
using System.Collections.Generic;
using NGame2.NBattleContext;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NComponent2.NTower;
using NGame2.NUI.NHelper.NTower;
using NGame2.NUI.NManager.NCommonImpl;
using NShared;
using NVespa;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NUtil;
using UnityEngine;

namespace NGame2.NUI.NWindow.NTower;

public class OrdealTowerWindow : BattleDungeonInfo, OrdealTowerFloorComponent.Listener
{
	public new const string PrefabPath = "Resources/Prefab2/Window/Tower/OrdealTowerWindow";

	public UIBaseGrid ItemGrid;

	public UIBaseGrid TowerGrid;

	public UILabel FloorLabel;

	public UILabel RemainResetTimeLabel;

	private BattleDungeonInfoListener _listener2;

	private CachedAccountInfo[] _npcInfos;

	private List<OrdealTowerFloorComponent> _floorComponentList = new List<OrdealTowerFloorComponent>();

	private Action<TowerFloorData> _onChangeFloor;

	private float _lastUpdateRemainResetTime;

	public TowerData TowerData { get; private set; }

	public TowerInfo TowerInfo { get; private set; }

	public TowerFloorData FloorData { get; private set; }

	public int ChapterIndex => GetChapterIndex();

	public int DungeonIndex => GetDungeonIndex();

	public int FloorIndex { get; private set; }

	public int MaxFloor { get; private set; }

	public int StartFloor { get; private set; }

	public int NpcIndex => FloorIndex - 1;

	public void Init(TowerData towerData, TowerInfo towerInfo, CachedAccountInfo[] npcInfos, int showFloor, BattleDungeonInfoListener listener)
	{
		_listener2 = listener;
		TowerData = towerData;
		TowerInfo = towerInfo;
		MaxFloor = BaseTower.GetMaxFloor(towerData.TowerIndex);
		_npcInfos = npcInfos;
		if (showFloor == 0)
		{
			showFloor = GetCurrentFloor();
		}
		InitStartFloor(showFloor);
		RefreshFloorData(showFloor);
		CreateFloorComponent();
		SetDungeonInfo();
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
		EnterDungeonData enterDungeonData = new EnterDungeonData(ChapterIndex, DungeonIndex, DungeonIndex, DungeonDifficultyType.Easy);
		enterDungeonData.ShowDuplicated = true;
		_listener2.EnterDungeon = enterDungeonData;
		OnOpen(_listener2.EnterDungeon, _listener2);
		Refresh();
		UIHelper.EnableButton(InfoComponents.Button_BattleReady, FloorIndex == GetCompletedFloor() + 1);
		UIHelper.SetLabelText(FloorLabel, FloorIndex);
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
		InfoComponents.ReleaseMonsterComponents();
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

	public override void AutoNextBattle()
	{
		if (GameOptionData.EditorCanAutoPlay)
		{
			SingletonUpdater<Executer>.instance.DelayRun(3f, this, base.OnButtonReadyBattle);
		}
	}

	public override void SetupDropItems()
	{
		ClearDropItem();
		TowerHelper.SetupReward(FloorData, GetCompletedFloor(), ItemGrid, _itemIconList);
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
		for (int num = MaxFloor; num >= 1; num--)
		{
			OrdealTowerFloorComponent ordealTowerFloorComponent = TowerGrid.AddGridItem(OrdealTowerFloorComponent.PrefabAuto);
			ordealTowerFloorComponent.Open(this, num, GetCompletedFloor(), TowerData.FloorCountPerPage);
			_floorComponentList.Add(ordealTowerFloorComponent);
		}
		TowerGrid.RepositionGrid();
		OrdealTowerFloorComponent ordealTowerFloorComponent2 = _floorComponentList.Find((OrdealTowerFloorComponent e) => e.Floor == FloorIndex);
		if (ordealTowerFloorComponent2 != null)
		{
			TowerGrid.ScrollTo(ordealTowerFloorComponent2, 0f, 0f);
		}
	}

	private void RefreshFloorComponent()
	{
		_floorComponentList.ForEach(delegate(OrdealTowerFloorComponent e)
		{
			e.RefreshSelection(FloorIndex);
		});
	}

	void OrdealTowerFloorComponent.Listener.OnClickFloor(OrdealTowerFloorComponent component)
	{
		RefreshFloorData(component.Floor);
		SetDungeonInfo();
		RefreshFloorComponent();
	}

	void OrdealTowerFloorComponent.Listener.OnDragComponent()
	{
	}

	void OrdealTowerFloorComponent.Listener.OnDragComponentEnd()
	{
	}
}

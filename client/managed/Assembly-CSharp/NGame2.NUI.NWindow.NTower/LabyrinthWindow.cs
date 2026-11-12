using System.Collections.Generic;
using NGame2.NBattleContext;
using NGame2.NUI.NComponent2.NTower;
using NGame2.NUI.NHelper.NTower;
using NGame2.NUI.NManager;
using NGameAsset;
using NShared;
using NVespa;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow.NTower;

public class LabyrinthWindow : BaseTower, LabyrinthFloorComponent.Listener, LobbyTopmostMenu.ICustomStaminaObserver, IInspectorGuiDrawer
{
	public interface Listener
	{
		void OnEnterFloor(LabyrinthWindow caller, TowerFloorData floorData);
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Tower/LabyrinthWindow";

	public UIScrollGrid FloorGrid;

	public int FloorFocusOffset;

	public UILabel FloorNameLabel;

	public UIScrollGrid MonsterGrid;

	public UILabel MinGoldLabel;

	public UILabel MaxGoldLabel;

	[HideInInspector]
	public BaseBoss3dView Boss3dView;

	private Listener _listener;

	private List<LabyrinthFloorComponent> _floorComponentList = new List<LabyrinthFloorComponent>();

	private List<Component> _monsterComponentList = new List<Component>();

	private TowerFloorData _currentFloorData;

	private StaminaType _staminaType;

	StaminaType LobbyTopmostMenu.ICustomStaminaObserver.StaminaType => _staminaType;

	public void Init(TowerData towerData, TowerInfo towerInfo, int showFloor, Listener listener)
	{
		Init(towerData, towerInfo);
		_listener = listener;
		_currentFloorData = null;
		SetupFloorList();
		SetupCurrentFloorInfo(showFloor);
	}

	public override bool CanReset()
	{
		if (base.TowerInfo == null || string.IsNullOrEmpty(base.TowerInfo.CompletedTime))
		{
			return false;
		}
		return true;
	}

	public override void Close()
	{
		base.Close();
		ClearBoss3dView();
		ClearFloorList();
		ClearMonsterList();
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		Boss3dView.Update();
	}

	private void SetupCurrentFloorInfo(int showFloor = 0)
	{
		if (base.TowerData == null)
		{
			return;
		}
		int num = 0;
		if (showFloor == 0)
		{
			int num2 = ((base.TowerInfo != null) ? base.TowerInfo.CompletedFloor : 0);
			int maxFloor = BaseTower.GetMaxFloor(base.TowerData.TowerIndex);
			num = Mathf.Min(num2 + 1, maxFloor);
		}
		else
		{
			num = showFloor;
		}
		LabyrinthFloorComponent floorComponent = GetFloorComponent(num);
		if (!(floorComponent == null))
		{
			if ((bool)FloorGrid)
			{
				FloorGrid.ScrollTo(floorComponent, FloorFocusOffset, 0f);
			}
			TowerFloorData floorData = floorComponent.FloorData;
			ShowFloorInfo(floorData);
		}
	}

	private void ShowFloorInfo(TowerFloorData floorData)
	{
		if (floorData == null || floorData == _currentFloorData)
		{
			return;
		}
		_currentFloorData = floorData;
		_staminaType = floorData.GetReqStaminaType(GlobalAutoBehaviour<BoosterManager>.instance.CheckSaveStamina(), StaminaType.UndergroundLabyrinthKey);
		foreach (LabyrinthFloorComponent floorComponent in _floorComponentList)
		{
			if ((bool)floorComponent)
			{
				bool selected = floorComponent.FloorData == floorData;
				floorComponent.SetSelected(selected);
			}
		}
		string text = base.TowerData.Name;
		string localizedFloorText = TowerHelper.GetLocalizedFloorText(floorData.Floor);
		UIHelper.SetLabelText(FloorNameLabel, text + " " + localizedFloorText);
		InitBoss3dView();
		InitMonsterList();
		InitRewardGold();
	}

	public void EnterCurrentFloor()
	{
		if (_listener != null)
		{
			_listener.OnEnterFloor(this, _currentFloorData);
		}
	}

	private void InitBoss3dView()
	{
		if (_currentFloorData != null)
		{
			int chapterIndex = _currentFloorData.ChapterIndex;
			int dungeonIndex = _currentFloorData.DungeonIndex;
			Boss3dView.Open(chapterIndex, dungeonIndex, "LabyrinthBoss", CreatureStock.ELoadLevel.IncludeFacial);
			Boss3dView.Init();
		}
	}

	private void ClearBoss3dView()
	{
		Boss3dView.Clear();
	}

	private void ClearMonsterList()
	{
		FactoryBase.ReleaseAndClearList(_monsterComponentList);
	}

	private void InitMonsterList()
	{
		ClearMonsterList();
		if (_currentFloorData != null)
		{
			EnterDungeonData enterDungeon = new EnterDungeonData(_currentFloorData.ChapterIndex, _currentFloorData.DungeonIndex, _currentFloorData.DungeonIndex, DungeonDifficultyType.Easy);
			List<Component> list = UIHelper.CreateSmallMonsterPortraitList(MonsterGrid, enterDungeon);
			if (__Extension_Method_ICollection__.COUNT(list) > 0)
			{
				_monsterComponentList.AddRange(list);
			}
		}
	}

	private void InitRewardGold()
	{
		int num = 0;
		long i_value = num;
		int num2 = 0;
		long i_value2 = num2;
		int chapterIndex = _currentFloorData.ChapterIndex;
		int dungeonIndex = _currentFloorData.DungeonIndex;
		CampaignDungeonData data = GameRoot.Instance.CampaignDungeonDataContainer.GetData(chapterIndex, dungeonIndex);
		if (data != null)
		{
			int rewardIndexOfDifficulty = data.GetRewardIndexOfDifficulty(DungeonDifficultyType.Easy);
			RewardData data2 = GameRoot.Instance.RewardDataContainer.GetData(rewardIndexOfDifficulty);
			if (data2 != null)
			{
				int goldMin = data2.GoldMin;
				i_value = goldMin;
				int goldMax = data2.GoldMax;
				i_value2 = goldMax;
			}
		}
		UIHelper.SetLabelText(MinGoldLabel, Vespa_Int_Extension_Method.ToCommaString(i_value));
		UIHelper.SetLabelText(MaxGoldLabel, Vespa_Int_Extension_Method.ToCommaString(i_value2));
	}

	private void SetupFloorList()
	{
		ClearFloorList();
		int maxFloor = BaseTower.GetMaxFloor(base.TowerData.TowerIndex);
		for (int i = 1; i <= maxFloor; i++)
		{
			TowerFloorData data = GameRoot.Instance.TowerFloorDataContainer.GetData(base.TowerData.TowerIndex, i);
			if (data == null)
			{
				break;
			}
			LabyrinthFloorComponent labyrinthFloorComponent = UIHelper.AddGridItem(FloorGrid, LabyrinthFloorComponent.PrefabAuto);
			labyrinthFloorComponent.Open(this, base.TowerData, data, base.CompletedFloor);
			_floorComponentList.Add(labyrinthFloorComponent);
		}
		UIHelper.RepositionGrid(FloorGrid);
	}

	private void ClearFloorList()
	{
		FactoryBase.ReleaseAndClearList(_floorComponentList);
	}

	private LabyrinthFloorComponent GetFloorComponent(int floor)
	{
		return _floorComponentList.Find((LabyrinthFloorComponent e) => e.FloorData != null && e.FloorData.Floor == floor);
	}

	public void OnClickClose()
	{
		Close();
	}

	void LabyrinthFloorComponent.Listener.OnClickFloor(LabyrinthFloorComponent component)
	{
		if (component.FloorData.Floor <= component.CompletedFloor + 1)
		{
			ShowFloorInfo(component.FloorData);
		}
	}
}

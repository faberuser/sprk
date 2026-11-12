using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NShared;
using NVespa;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow.NTower;

public class MazeTowerList : LobbyFullWindow, MazeTowerComponent.Listener, LobbyTopmostMenu.ICustomStaminaObserver
{
	public interface Listener
	{
		void OnClickEnterDungeon(int DungeonIndex);

		void OnClickSweepAdd(int DungeonIndex);

		void OnClickInvestigate(int DungeonIndex);

		bool OnCloseRequest(MazeTowerList closeThis);

		void ReOpen();
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Tower/MazeTowerList";

	public UIScrollGrid DungeonGrid;

	public UISprite FloorClearedGauge;

	public UILabel Label_ClearCount;

	public UIScrollGrid RewardItemGrid;

	public UIButton Button_Reward;

	public GameObject RewardRoot;

	private List<MazeTowerComponent> _dungeonList = new List<MazeTowerComponent>();

	private List<ItemIconComponent> _rewardItemList = new List<ItemIconComponent>();

	private Listener _listener;

	private LocalAccountInt _lastEnterDungeon;

	private int _MazeTowerRewardStep;

	private int _TotalFloorCount;

	private int _TotalClearedFloorCount;

	public StaminaType StaminaType => Stamina;

	public void Open(Listener listener, int MazeTowerRewardStep)
	{
		Open(delegate
		{
			_listener = listener;
			_MazeTowerRewardStep = MazeTowerRewardStep;
			_lastEnterDungeon = new LocalAccountInt(0, "LastMazeTowerOffset");
			Init();
		}, delegate
		{
			MoveDungeonListPanel(_lastEnterDungeon.Value);
		});
	}

	private void Clear()
	{
		_TotalFloorCount = 0;
		_TotalClearedFloorCount = 0;
		FactoryBase.ReleaseAndClearList(_dungeonList);
		FactoryBase.ReleaseAndClearList(_rewardItemList);
		UIHelper.EnableGameObject(RewardRoot, enable: true);
	}

	public override void Reset()
	{
		base.Reset();
		Clear();
	}

	public void Init()
	{
		SetupTowerList();
		SetupRewardStatus();
		SetupRewardItem();
		SetRewardButton();
	}

	public override void OnClosing()
	{
		base.OnClosing();
		Clear();
	}

	public void SetupRewardStatus()
	{
		int[] towerIndices = GameRoot.Instance.TowerDataContainer.GetTowerIndices(TowerType.Maze);
		for (int i = 0; i < towerIndices.Length; i++)
		{
			int floorCount = GameRoot.Instance.TowerFloorDataContainer.GetFloorCount(towerIndices[i]);
			_TotalFloorCount += floorCount;
			TowerInfo towerInfo = GlobalAutoBehaviour<AccountManager>.instance.TowerManager.Get(towerIndices[i]);
			if (towerInfo != null && towerInfo.CompletedFloor != 0)
			{
				_TotalClearedFloorCount += towerInfo.CompletedFloor;
			}
		}
		MazeClearRewardData data = GameRoot.Instance.MazeClearRewardDataContainer.GetData(_MazeTowerRewardStep + 1);
		if (data == null)
		{
			UIHelper.EnableGameObject(RewardRoot, enable: false);
			return;
		}
		int clearFloor = data.ClearFloor;
		FloorClearedGauge.fillAmount = (float)_TotalClearedFloorCount / (float)clearFloor;
		UIHelper.SetLabelText(Label_ClearCount, $"{_TotalClearedFloorCount}/{clearFloor}");
	}

	public void SetupRewardItem()
	{
		MazeClearRewardData data = GameRoot.Instance.MazeClearRewardDataContainer.GetData(_MazeTowerRewardStep + 1);
		if (data == null)
		{
			UIHelper.EnableGameObject(RewardRoot, enable: false);
			return;
		}
		RewardData data2 = GameRoot.Instance.RewardDataContainer.GetData(data.RewardIndex);
		if (data2 == null)
		{
			VespaDebug.LogError("reward not found");
			return;
		}
		List<RewardElementData> rewardItemList = data2.GetRewardItemList();
		if (rewardItemList == null)
		{
			return;
		}
		for (int i = 0; i < rewardItemList.Count; i++)
		{
			RewardElementData rewardElementData = rewardItemList[i];
			HashString itemCode = rewardElementData.ItemCode;
			ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(itemCode);
			if (dataByCode != null && (rewardElementData.ItemMin != 0 || rewardElementData.ItemMax != 0))
			{
				ItemIconComponent itemIconComponent = UIHelper.AddGridItem(RewardItemGrid, ItemIconComponent.PrefabAuto);
				itemIconComponent.Open(dataByCode);
				if (rewardElementData.ItemMin != rewardElementData.ItemMax)
				{
					int itemMin = rewardElementData.ItemMin;
					int itemMax = rewardElementData.ItemMax;
					Color? color = null;
					int num = itemMax;
					int num2 = itemMin;
					itemIconComponent.SetItemMinMax(num2, num, color);
				}
				else
				{
					int itemMin2 = rewardElementData.ItemMin;
					Color? color2 = null;
					int num3 = itemMin2;
					itemIconComponent.SetItemCount(num3, color2);
				}
				_rewardItemList.Add(itemIconComponent);
			}
		}
		UIHelper.RepositionGrid(RewardItemGrid);
	}

	public void SetRewardButton()
	{
		MazeClearRewardData data = GameRoot.Instance.MazeClearRewardDataContainer.GetData(_MazeTowerRewardStep + 1);
		if (data != null)
		{
			int clearFloor = data.ClearFloor;
			if (_TotalClearedFloorCount >= clearFloor)
			{
				UIHelper.EnableButton(Button_Reward, enable: true, grayscaleIfDisable: true);
			}
			else
			{
				UIHelper.EnableButton(Button_Reward, enable: false, grayscaleIfDisable: true);
			}
		}
	}

	public void SetupTowerList()
	{
		Clear();
		using (TemporaryList<TowerData> temporaryList = GameRoot.Instance.TowerDataContainer.GetTowers(TowerType.Maze))
		{
			for (int i = 0; i < temporaryList.Count; i++)
			{
				TowerData towerData = temporaryList[i];
				CreateTower(towerData);
			}
		}
		UIHelper.RepositionGrid(DungeonGrid);
	}

	private void MoveDungeonListPanel(int selectedIndex)
	{
		if (_dungeonList.Count <= 0 || selectedIndex < 0 || selectedIndex > _dungeonList.Count - 1)
		{
			return;
		}
		MazeTowerComponent mazeTowerComponent = _dungeonList[selectedIndex];
		if (mazeTowerComponent == null)
		{
			return;
		}
		UIPanel panel = DungeonGrid.ScrollView.panel;
		if (panel == null)
		{
			return;
		}
		Transform cachedTransform = panel.cachedTransform;
		Vector3[] worldCorners = panel.worldCorners;
		if (selectedIndex == 0)
		{
			DungeonGrid.RepositionGrid();
			return;
		}
		Vector3 vector2;
		if (selectedIndex == _dungeonList.Count - 1)
		{
			Vector3 vector = cachedTransform.InverseTransformPoint(worldCorners[3]);
			vector2 = cachedTransform.InverseTransformPoint(_dungeonList[selectedIndex].transform.position) - vector;
			vector2.x += DungeonGrid.Grid.cellWidth / 2f;
		}
		else
		{
			Vector3 position = (worldCorners[1] + worldCorners[3]) * 0.5f;
			Vector3 vector3 = cachedTransform.InverseTransformPoint(mazeTowerComponent.transform.position);
			Vector3 vector4 = cachedTransform.InverseTransformPoint(position);
			vector2 = vector3 - vector4;
		}
		if (!DungeonGrid.ScrollView.canMoveHorizontally)
		{
			vector2.x = 0f;
		}
		if (!DungeonGrid.ScrollView.canMoveVertically)
		{
			vector2.y = 0f;
		}
		vector2.z = 0f;
		Vector3 relative = cachedTransform.localPosition - vector2;
		DungeonGrid.ScrollView.MoveRelative(relative);
	}

	private void CreateTower(TowerData towerData)
	{
		MazeTowerComponent mazeTowerComponent = UIHelper.AddGridItem(DungeonGrid, MazeTowerComponent.PrefabAuto);
		mazeTowerComponent.Open(this, towerData);
		_dungeonList.Add(mazeTowerComponent);
	}

	public void OnClickExitButton()
	{
		if (_listener == null || _listener.OnCloseRequest(this))
		{
			Close();
		}
	}

	public void OnClickRewardButton()
	{
		SceneAutoBehaviour<TowerManagement>.instance.RequestMazeTowerReward(_MazeTowerRewardStep + 1, delegate
		{
			int mazeTowerRewardStep = _MazeTowerRewardStep + 1;
			Reset();
			_MazeTowerRewardStep = mazeTowerRewardStep;
			Init();
		});
	}

	public override void OnSystemBackButton()
	{
		OnClickExitButton();
	}

	public override void ReOpen()
	{
		base.ReOpen();
		if (_listener != null)
		{
			_listener.ReOpen();
		}
	}

	public void Refresh()
	{
		_TotalFloorCount = 0;
		_TotalClearedFloorCount = 0;
		SetupRewardStatus();
		SetRewardButton();
	}

	void MazeTowerComponent.Listener.OnClickInfo(TowerData towerData)
	{
		MazeTowerInfoPopup.Open(towerData);
	}

	void MazeTowerComponent.Listener.OnClickEnter(TowerData towerData)
	{
		foreach (MazeTowerComponent dungeon in _dungeonList)
		{
			if (towerData.TowerIndex == dungeon._towerData.TowerIndex)
			{
				LocalAccountInt lastEnterDungeon = _lastEnterDungeon;
				int num = _dungeonList.IndexOf(dungeon);
				lastEnterDungeon.Value = num;
				break;
			}
		}
		SceneAutoBehaviour<TowerManagement>.instance.OpenTower(towerData.TowerIndex);
	}
}

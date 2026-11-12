using System.Collections.Generic;
using NGame2.NUI.NHelper.NTower;
using NShared;
using NVespa;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NTower;

public class TowerFloorComponent : LinkFactoryComponent, InitOnCreate, HeroPortraitComponent.Listener, ItemIconComponent.Listener
{
	public enum TowerBossType
	{
		Normal,
		SemiBoss,
		Boss
	}

	public interface Listener
	{
		void OnClickFloor(TowerFloorComponent component);
	}

	public const string PrefabPath = "Prefab2/Component/Tower/TowerFloorComponent";

	public static PrefabAuto<TowerFloorComponent> PrefabAuto = new PrefabAuto<TowerFloorComponent>("Prefab2/Component/Tower/TowerFloorComponent");

	public UILabel FloorLabel;

	public Transform LockedRoot;

	public Transform LockedNormal;

	public Transform LockedSemiBoss;

	public Transform LockedBoss;

	public Transform PassedRoot;

	public UIGame3DWidget PassedEffectWidget;

	public UIWidget VisibleRoot;

	public Transform VisibleNormal;

	public Transform VisibleSemiBoss;

	public Transform VisibleBoss;

	public Transform PortraitRoot;

	public UIGame3DWidget OpenNormalEffectWidget;

	public UIGame3DWidget OpenSemiBossEffectWidget;

	public UIGame3DWidget OpenBossEffectWidget;

	public Transform[] FloorType;

	public Transform RewardRoot;

	public Transform EnabledReward;

	public Transform DisabledReward;

	public UIWidget RewardIconRoot;

	private Listener _listener;

	private HeroPortraitComponent _portraitComponent;

	private Component _rewardItemComponent;

	public TowerData TowerData { get; private set; }

	public TowerFloorData FloorData { get; private set; }

	public int CompletedFloor { get; private set; }

	public TowerBossType BossType { get; private set; }

	public bool IsCompleted => CompletedFloor >= FloorData.Floor;

	public int FloorTypeIndex { get; private set; }

	public CachedAccountInfo NpcInfo { get; private set; }

	public void Open(Listener listener, TowerData towerData, TowerFloorData floorData, int completedFloor, TowerBossType bossType, CachedAccountInfo npcInfo)
	{
		_listener = listener;
		TowerData = towerData;
		FloorData = floorData;
		CompletedFloor = completedFloor;
		BossType = bossType;
		NpcInfo = npcInfo;
		InitCompletedState();
		InitFloorNumber();
		InitFloorType();
		InitPortrait();
		InitReward();
	}

	public void RefreshFloorEffect()
	{
		InitCompletedState();
	}

	private void InitCompletedState()
	{
		if (!(VisibleRoot == null))
		{
			if (IsCompleted)
			{
				VisibleRoot.isGrayscale = true;
				PlayPassedWidget(justCleared: false);
			}
			else
			{
				VisibleRoot.isGrayscale = false;
			}
		}
	}

	public void PlayPassedWidget(bool justCleared)
	{
		if (!(PassedEffectWidget == null))
		{
			if (justCleared)
			{
				PassedEffectWidget.Play(null);
				return;
			}
			PassedEffectWidget.Play(null);
			PassedEffectWidget.PlayPhase("End");
		}
	}

	public void StopPassedWidget()
	{
		if (!(PassedEffectWidget == null))
		{
			PassedEffectWidget.Stop(immediatelyClear: true);
		}
	}

	public void PlayOpenEffectWidget()
	{
		if (BossType == TowerBossType.Normal && (bool)OpenNormalEffectWidget)
		{
			OpenNormalEffectWidget.Play(null);
		}
		else if (BossType == TowerBossType.SemiBoss && (bool)OpenSemiBossEffectWidget)
		{
			OpenSemiBossEffectWidget.Play(null);
		}
		else if (BossType == TowerBossType.Boss && (bool)OpenBossEffectWidget)
		{
			OpenBossEffectWidget.Play(null);
		}
	}

	public void SetLocked(bool locked)
	{
		UIHelper.EnableGameObject(LockedRoot, locked);
		if (locked)
		{
			InitLockedPortrait();
		}
	}

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
		FactoryBase.SafeReleaseComponent(ref _portraitComponent);
		FactoryBase.SafeReleaseComponent(ref _rewardItemComponent);
		if ((bool)PassedEffectWidget)
		{
			PassedEffectWidget.Stop(immediatelyClear: true);
		}
		if ((bool)OpenNormalEffectWidget)
		{
			OpenNormalEffectWidget.Stop(immediatelyClear: true);
		}
		if ((bool)OpenSemiBossEffectWidget)
		{
			OpenSemiBossEffectWidget.Stop(immediatelyClear: true);
		}
		if ((bool)OpenBossEffectWidget)
		{
			OpenBossEffectWidget.Stop(immediatelyClear: true);
		}
	}

	private void InitFloorNumber()
	{
		string localizedFloorText = TowerHelper.GetLocalizedFloorText(FloorData.Floor);
		UIHelper.SetLabelText(FloorLabel, localizedFloorText);
	}

	private void InitFloorType()
	{
		int floor = FloorData.Floor;
		int num = ((TowerData.FloorCountPerPage <= 0) ? 1 : TowerData.FloorCountPerPage);
		FloorTypeIndex = (floor - 1) % num;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(FloorType); i++)
		{
			UIHelper.EnableGameObject(FloorType[i], i == FloorTypeIndex);
		}
	}

	private void InitPortrait()
	{
		bool flag = FloorData.Floor > CompletedFloor + 1;
		bool flag2 = !flag;
		bool isCompleted = IsCompleted;
		UIHelper.EnableGameObject(LockedRoot, flag);
		UIHelper.EnableGameObject(VisibleRoot, flag2);
		UIHelper.EnableGameObject(PassedRoot, isCompleted);
		if (flag)
		{
			InitLockedPortrait();
		}
		else if (flag2)
		{
			InitVisiblePortrait(isCompleted);
		}
	}

	private void InitLockedPortrait()
	{
		UIHelper.EnableGameObject(LockedNormal, BossType == TowerBossType.Normal);
		UIHelper.EnableGameObject(LockedSemiBoss, BossType == TowerBossType.SemiBoss);
		UIHelper.EnableGameObject(LockedBoss, BossType == TowerBossType.Boss);
	}

	private void InitVisiblePortrait(bool passed)
	{
		int chapterIndex = FloorData.ChapterIndex;
		int dungeonIndex = FloorData.DungeonIndex;
		CreatureData bossCreatureData = GameRoot.Instance.CampaignDungeonDataContainer.GetBossCreatureData(chapterIndex, dungeonIndex, DungeonDifficultyType.Easy, scenario: false);
		if (bossCreatureData != null)
		{
			_portraitComponent = HeroPortraitComponent.SmallPrefabAuto.Factory.Create(PortraitRoot);
			_portraitComponent.Open(bossCreatureData, this, HeroPortraitComponent.EPortraitMode.Icon);
			Vespa_Unity_Transform_Extension_Method.ResetLocalTM(_portraitComponent.transform);
		}
		UIHelper.EnableGameObject(VisibleNormal, BossType == TowerBossType.Normal);
		UIHelper.EnableGameObject(VisibleSemiBoss, BossType == TowerBossType.SemiBoss);
		UIHelper.EnableGameObject(VisibleBoss, BossType == TowerBossType.Boss);
	}

	public static Component CreateStateComponent<L>(TowerFloorData floorData, bool isRepeating, Transform root, L listener) where L : ItemIconComponent.Listener
	{
		if (floorData == null)
		{
			return null;
		}
		int[] array = floorData.RewardIndex;
		if (isRepeating && __Extension_Method_ICollection__.COUNT(floorData.RepeatRewardIndex) > 0)
		{
			array = floorData.RepeatRewardIndex;
		}
		if (__Extension_Method_ICollection__.COUNT(array) == 0)
		{
			return null;
		}
		int floorDungeonSlot = TowerHelper.GetFloorDungeonSlot(floorData);
		int key = ((floorDungeonSlot < __Extension_Method_ICollection__.COUNT(array)) ? array[floorDungeonSlot] : array[__Extension_Method_ICollection__.COUNT(array) - 1]);
		RewardData data = GameRoot.Instance.RewardDataContainer.GetData(key);
		if (data == null)
		{
			VespaDebug.LogError("reward not found : rewardNo=" + floorDungeonSlot + " rewardIndex=" + key);
			return null;
		}
		List<RewardElementData> rewardItemList = data.GetRewardItemList();
		if (rewardItemList == null || rewardItemList.Count == 0)
		{
			return null;
		}
		for (int i = 0; i < rewardItemList.Count; i++)
		{
			RewardElementData rewardElementData = rewardItemList[i];
			ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(rewardElementData.ItemCode);
			if (dataByCode != null)
			{
				int index = dataByCode.Index;
				ItemIconComponent itemIconComponent = ItemIconComponent.PrefabAuto.Factory.Create(root);
				if (dataByCode.Type == ItemType.Equip)
				{
					int itemStarMin = rewardElementData.ItemStarMin;
					EquipItemInfo equipItemInfo = new EquipItemInfo
					{
						ItemIndex = index,
						Star = itemStarMin
					};
					itemIconComponent.Open(dataByCode, equipItemInfo, listener);
				}
				else
				{
					int itemMin = rewardElementData.ItemMin;
					long count = itemMin;
					itemIconComponent.Open(dataByCode, listener);
					itemIconComponent.SetItemCount(count);
				}
				Vespa_Unity_Transform_Extension_Method.ResetLocalTM(itemIconComponent.transform);
				return itemIconComponent;
			}
		}
		return null;
	}

	public static Component CreateRewardComponent<L>(TowerFloorData floorData, bool isRepeating, Transform root, L listener) where L : ItemIconComponent.Listener
	{
		if (floorData == null)
		{
			return null;
		}
		int[] array = floorData.RewardIndex;
		if (isRepeating && __Extension_Method_ICollection__.COUNT(floorData.RepeatRewardIndex) > 0)
		{
			array = floorData.RepeatRewardIndex;
		}
		if (__Extension_Method_ICollection__.COUNT(array) == 0)
		{
			return null;
		}
		int floorDungeonSlot = TowerHelper.GetFloorDungeonSlot(floorData);
		int key = ((floorDungeonSlot < __Extension_Method_ICollection__.COUNT(array)) ? array[floorDungeonSlot] : array[__Extension_Method_ICollection__.COUNT(array) - 1]);
		RewardData data = GameRoot.Instance.RewardDataContainer.GetData(key);
		if (data == null)
		{
			VespaDebug.LogError("reward not found : rewardNo=" + floorDungeonSlot + " rewardIndex=" + key);
			return null;
		}
		List<RewardElementData> rewardItemList = data.GetRewardItemList();
		if (rewardItemList == null || rewardItemList.Count == 0)
		{
			return null;
		}
		for (int i = 0; i < rewardItemList.Count; i++)
		{
			RewardElementData rewardElementData = rewardItemList[i];
			ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(rewardElementData.ItemCode);
			if (dataByCode != null)
			{
				int index = dataByCode.Index;
				ItemIconComponent itemIconComponent = ItemIconComponent.PrefabAuto.Factory.Create(root);
				if (dataByCode.Type == ItemType.Equip)
				{
					int itemStarMin = rewardElementData.ItemStarMin;
					EquipItemInfo equipItemInfo = new EquipItemInfo
					{
						ItemIndex = index,
						Star = itemStarMin
					};
					itemIconComponent.Open(dataByCode, equipItemInfo, listener);
				}
				else
				{
					int itemMin = rewardElementData.ItemMin;
					long count = itemMin;
					itemIconComponent.Open(dataByCode, listener);
					itemIconComponent.SetItemCount(count);
				}
				Vespa_Unity_Transform_Extension_Method.ResetLocalTM(itemIconComponent.transform);
				return itemIconComponent;
			}
		}
		return null;
	}

	private void InitReward()
	{
		UIHelper.EnableGameObject(RewardRoot, enable: true);
		bool isCompleted = IsCompleted;
		UIHelper.EnableGameObject(EnabledReward, !isCompleted);
		UIHelper.EnableGameObject(DisabledReward, isCompleted);
		_rewardItemComponent = CreateRewardComponent(FloorData, isCompleted, RewardIconRoot ? RewardIconRoot.transform : null, this);
		if ((bool)RewardIconRoot)
		{
			RewardIconRoot.isGrayscale = isCompleted;
		}
	}

	public void OnClickFloor()
	{
		if (_listener != null)
		{
			_listener.OnClickFloor(this);
		}
	}

	void HeroPortraitComponent.Listener.OnClick(HeroPortraitComponent heroPortrait)
	{
		OnClickFloor();
	}

	void HeroPortraitComponent.Listener.OnViewInfo(HeroPortraitComponent heroPortrait, bool isMine)
	{
	}

	void HeroPortraitComponent.Listener.OnClickHeroNotify(HeroNotifyComponent.ENotifyInfo notifyInfo, CachedHeroInfo cachedHeroInfo)
	{
	}

	void HeroPortraitComponent.Listener.OnLongClick(HeroPortraitComponent heroPortrait, CachedHeroInfo heroInfo)
	{
	}

	void ItemIconComponent.Listener.OnClick(ItemIconComponent itemIconComponent)
	{
		OnClickFloor();
	}
}

using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NAccount.NExtension;
using NGame2.NDropBonusEvent;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NHelper.NItem;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class EquipEnhanceBase : BaseWindow, ItemIconComponent.Listener, HeroEquipListComponent.Listener, IUserObserver<NGame2.NAccount.UserEquipItemManager.EquipStatInfo>, IUserObserver
{
	public enum ESortType
	{
		Up,
		Down,
		Max
	}

	[Serializable]
	public class UpDownSpriteInfo
	{
		public UISprite SpriteUp;

		public UISprite SpriteDown;

		public bool Initial;

		public bool IsUp { get; set; }
	}

	public class EquipItemTableData
	{
		public EquipItemInfo EquipItemInfo;

		public ItemData ItemData;

		public EquipItemData EquipData;
	}

	[Space(10f)]
	[Header("EquipEnhanceBase")]
	public bool isTechnomagicDisable;

	public UISimpleGrid TargetEquipItemGrid;

	public UIScrollGrid HeroEquipItemGrid;

	public UIScrollGrid InventoryEquipItemGrid;

	public UpDownSpriteInfo SortByHeroLevel;

	public UpDownSpriteInfo SortByHeroStar;

	public UpDownSpriteInfo SortByEquipLevel;

	public UpDownSpriteInfo SortByEquipGrade;

	public UpDownSpriteInfo SortByEquipStar;

	public UIToggle HeroEquipToggle;

	public UIToggle InventoryEquipToggle;

	public Transform HeroEquipRoot;

	public Transform InventoryEquipRoot;

	public Transform EmptyHeroEquipRoot;

	public Transform EmptyInventoryEquipRoot;

	public Transform NoEquipSelectedRoot;

	public Transform EquipSelectedRoot;

	public GameObject EventRoot;

	public UISprite EventButtonImage;

	protected List<HeroEquipListComponent> _heroEquipListComponentList = new List<HeroEquipListComponent>();

	protected List<ItemIconComponent> _inventoryItemComponentList = new List<ItemIconComponent>();

	protected ItemIconComponent _targetItemIconComponent;

	protected ItemIconComponent _selectedItemIconComponent;

	protected virtual EventBonusType BonusType => EventBonusType.None;

	public EquipItemInfo SelectedEquipItemInfo { get; private set; }

	protected void InitBase()
	{
		InitSortButton();
		InitEnhanceableEquipItem();
		UpdateEquipSelectedRoot(selected: false);
		SetEvent(BonusType);
	}

	public override void Reset()
	{
		base.Reset();
		FactoryBase.ReleaseAndClearList(_heroEquipListComponentList);
		FactoryBase.ReleaseAndClearList(_inventoryItemComponentList);
		ClearTargetItemIconComponent();
		_selectedItemIconComponent = null;
		SelectedEquipItemInfo = null;
	}

	private void ClearTargetItemIconComponent()
	{
		FactoryBase.SafeReleaseComponent(ref _targetItemIconComponent);
	}

	public virtual bool CanEnhance(EquipItemInfo equipItemInfo)
	{
		return false;
	}

	public virtual void OnTargetItemSelected(EquipItemInfo equipItemInfo)
	{
		SelectedEquipItemInfo = equipItemInfo;
		UpdateEquipSelectedRoot(equipItemInfo != null);
	}

	private void UpdateEquipSelectedRoot(bool selected)
	{
		UIHelper.EnableGameObject(EquipSelectedRoot, selected);
		UIHelper.EnableGameObject(NoEquipSelectedRoot, !selected);
	}

	protected void InitEnhanceableEquipItem()
	{
		CreateHeroEquipList();
		CreateInventoryEquipList();
	}

	protected virtual void CreateHeroEquipList()
	{
		if (!(HeroEquipItemGrid == null))
		{
			FactoryBase.ReleaseAndClearList(_heroEquipListComponentList);
			CachedHeroInfo[] array = GlobalAutoBehaviour<AccountManager>.instance.HeroManager.ToArray();
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(array); i++)
			{
				CachedHeroInfo heroInfo = array[i];
				CreateHeroEquipListComponent(heroInfo);
			}
			HeroEquipItemGrid.Grid.sorting = UIGrid.Sorting.Custom;
			HeroEquipItemGrid.Grid.onCustomSort = SortHeroEquipItemList;
			UIHelper.RepositionGrid(HeroEquipItemGrid);
			UIHelper.EnableGameObject(EmptyHeroEquipRoot, __Extension_Method_ICollection__.COUNT(_heroEquipListComponentList) == 0);
		}
	}

	protected HeroEquipListComponent CreateHeroEquipListComponent(CachedHeroInfo heroInfo)
	{
		HeroEquipListComponent heroEquipListComponent = UIHelper.AddGridItem(HeroEquipItemGrid, HeroEquipListComponent.PrefabAuto);
		Vespa_Unity_Transform_Extension_Method.ResetLocalTM(heroEquipListComponent.transform);
		heroEquipListComponent.Open(heroInfo, this, selectable: false, useDispatch: true);
		heroEquipListComponent.ForEachItemComponent(PostProcessItemComponent);
		_heroEquipListComponentList.Add(heroEquipListComponent);
		return heroEquipListComponent;
	}

	protected void CreateInventoryEquipList()
	{
		if (InventoryEquipItemGrid == null)
		{
			return;
		}
		FactoryBase.ReleaseAndClearList(_inventoryItemComponentList);
		EquipItemInfo[] array = GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.ToArray();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(array); i++)
		{
			EquipItemInfo equipItemInfo = array[i];
			if (equipItemInfo != null && GlobalAutoBehaviour<AccountManager>.instance.GetHeroOfEquipItem(equipItemInfo.SlotIndex) == null && equipItemInfo.GetInventoryType() != InventoryType.Chest)
			{
				CreateInventoryItemComponent(equipItemInfo, noTechnomagic: true);
			}
		}
		InventoryEquipItemGrid.Grid.sorting = UIGrid.Sorting.Custom;
		InventoryEquipItemGrid.Grid.onCustomSort = SortInventoryEquipItemList;
		UIHelper.RepositionGrid(InventoryEquipItemGrid);
		UIHelper.EnableGameObject(EmptyInventoryEquipRoot, __Extension_Method_ICollection__.COUNT(_inventoryItemComponentList) == 0);
	}

	protected virtual ItemIconComponent CreateInventoryItemComponent(EquipItemInfo equipItemInfo, bool noTechnomagic = false)
	{
		if (noTechnomagic && equipItemInfo.IsTechnoMagic)
		{
			return null;
		}
		ItemIconComponent itemIconComponent = UIHelper.AddGridItem(InventoryEquipItemGrid, ItemIconComponent.PrefabAuto);
		itemIconComponent.Open(equipItemInfo, this);
		PostProcessItemComponent(itemIconComponent);
		_inventoryItemComponentList.Add(itemIconComponent);
		return itemIconComponent;
	}

	protected bool RemoveInventoryItemComponent(EquipItemInfo equipItemInfo)
	{
		ItemIconComponent comp = _inventoryItemComponentList.Find((ItemIconComponent e) => e.EquipItemInfo == equipItemInfo);
		if ((bool)comp)
		{
			_inventoryItemComponentList.Remove(comp);
			FactoryBase.SafeReleaseComponent(ref comp);
			return true;
		}
		return false;
	}

	private void PostProcessItemComponent(ItemIconComponent component)
	{
		EquipItemInfo equipItemInfo = component.EquipItemInfo;
		bool flag = CanEnhance(equipItemInfo);
		component.SetDisabled(!flag || (isTechnomagicDisable && equipItemInfo.IsTechnoMagic));
	}

	protected void OnClickItemIconComponent(ItemIconComponent component)
	{
		if (!(component == null))
		{
			if (component.EquipItemInfo == null && component.ItemData != null)
			{
				OnClickItem(component);
			}
			else if (!component.EquipItemInfo.IsTechnoMagic)
			{
				OnClickEquipItem(component);
			}
		}
	}

	protected virtual void OnClickItem(ItemIconComponent component)
	{
	}

	public void SelectItemIconComponent(EquipItemInfo itemInfo)
	{
		ItemIconComponent itemIconComponent = _inventoryItemComponentList.Find((ItemIconComponent comp) => comp.EquipItemInfo.SlotIndex == itemInfo.SlotIndex);
		if (itemIconComponent != null)
		{
			OnClickItemIconComponent(itemIconComponent);
			return;
		}
		if (itemIconComponent == null)
		{
			foreach (HeroEquipListComponent heroEquipListComponent in _heroEquipListComponentList)
			{
				ItemIconComponent itemIconComponent2 = heroEquipListComponent.FindEquipItem(itemInfo.SlotIndex);
				if (itemIconComponent2 != null)
				{
					itemIconComponent = itemIconComponent2;
					break;
				}
			}
		}
		if (itemIconComponent != null)
		{
			OnClickItemIconComponent(itemIconComponent);
		}
		else if (itemInfo.IsEquipStorageSlot())
		{
			CreateTargetItemIcon(itemInfo);
			OnTargetItemSelected(itemInfo);
		}
	}

	protected void OnClickEquipItem(ItemIconComponent component)
	{
		if (!(component == null))
		{
			if (component.EquipItemInfo == SelectedEquipItemInfo)
			{
				DeselectTargetEquipItem();
			}
			else if (!component.IsDisabled() && !component.EquipItemInfo.IsTechnoMagic)
			{
				DeselectTargetEquipItem();
				component.SetSelected(selected: true);
				_selectedItemIconComponent = component;
				CreateTargetItemIcon(component.EquipItemInfo);
				OnTargetItemSelected(component.EquipItemInfo);
			}
		}
	}

	private void CreateTargetItemIcon(EquipItemInfo itemInfo)
	{
		if (!(TargetEquipItemGrid == null))
		{
			ClearTargetItemIconComponent();
			_targetItemIconComponent = UIHelper.AddGridItem(TargetEquipItemGrid, ItemIconComponent.PrefabAuto);
			Vespa_Unity_Transform_Extension_Method.ResetLocalTM(_targetItemIconComponent.transform);
			_targetItemIconComponent.Open(itemInfo, this);
		}
	}

	public void DeselectTargetEquipItem()
	{
		if ((bool)_selectedItemIconComponent)
		{
			_selectedItemIconComponent.SetSelected(selected: false);
			_selectedItemIconComponent = null;
		}
		ClearTargetItemIconComponent();
		OnTargetItemSelected(null);
	}

	private void InitSortButton()
	{
		InitSortButton(SortByHeroLevel);
		InitSortButton(SortByHeroStar);
		InitSortButton(SortByEquipLevel);
		InitSortButton(SortByEquipGrade);
		InitSortButton(SortByEquipStar);
	}

	private void InitSortButton(UpDownSpriteInfo info)
	{
		if (info != null)
		{
			info.IsUp = info.Initial;
			UpdateSortButton(info, toggle: false);
		}
	}

	private void UpdateSortButton(UpDownSpriteInfo info, bool toggle)
	{
		if (info != null)
		{
			if (toggle)
			{
				info.IsUp = !info.IsUp;
			}
			UIHelper.EnableGameObject(info.SpriteUp, info.IsUp);
			UIHelper.EnableGameObject(info.SpriteDown, !info.IsUp);
		}
	}

	public void OnClickSortByHeroLevel()
	{
		UpdateSortButton(SortByHeroLevel, toggle: true);
		UIHelper.RepositionGrid(HeroEquipItemGrid);
	}

	public void OnClickSortByHeroStar()
	{
		UpdateSortButton(SortByHeroStar, toggle: true);
		UIHelper.RepositionGrid(HeroEquipItemGrid);
	}

	protected int SortHeroEquipItemList(Transform l, Transform r)
	{
		HeroEquipListComponent component = l.GetComponent<HeroEquipListComponent>();
		HeroEquipListComponent component2 = r.GetComponent<HeroEquipListComponent>();
		if (component == null || component2 == null)
		{
			return 0;
		}
		CachedHeroInfo heroInfo = component.HeroInfo;
		CachedHeroInfo heroInfo2 = component2.HeroInfo;
		if (heroInfo == null || heroInfo2 == null)
		{
			return 0;
		}
		int num = CompareValue(SortByHeroLevel.IsUp, heroInfo.Level, heroInfo2.Level);
		if (num == 0)
		{
			num = CompareValue(SortByHeroStar.IsUp, heroInfo.Star, heroInfo2.Star);
		}
		if (num == 0)
		{
			num = CompareValue(SortByHeroStar.IsUp, heroInfo.Transcended, heroInfo2.Transcended);
		}
		if (num == 0)
		{
			num = CompareValue(isAscending: false, heroInfo.HeroIndex, heroInfo2.HeroIndex);
		}
		return num;
	}

	public void OnClickSortByEquipLevel()
	{
		UpdateSortButton(SortByEquipLevel, toggle: true);
		UIHelper.RepositionGrid(InventoryEquipItemGrid);
	}

	public void OnClickSortByEquipGrade()
	{
		UpdateSortButton(SortByEquipGrade, toggle: true);
		UIHelper.RepositionGrid(InventoryEquipItemGrid);
	}

	public void OnClickSortByEquipStar()
	{
		UpdateSortButton(SortByEquipStar, toggle: true);
		UIHelper.RepositionGrid(InventoryEquipItemGrid);
	}

	private int SortInventoryEquipItemList(Transform l, Transform r)
	{
		ItemIconComponent component = l.GetComponent<ItemIconComponent>();
		ItemIconComponent component2 = r.GetComponent<ItemIconComponent>();
		int num = ItemComparer.NullCheck(component, component2);
		if (num != 0)
		{
			return num;
		}
		ItemData itemData = component.ItemData;
		ItemData itemData2 = component2.ItemData;
		EquipItemInfo equipItemInfo = component.EquipItemInfo;
		EquipItemInfo equipItemInfo2 = component2.EquipItemInfo;
		EquipItemData equipItemData = component.EquipItemData;
		EquipItemData equipItemData2 = component2.EquipItemData;
		ItemComparer.SortClass sortClass = new ItemComparer.SortClass();
		sortClass.Add(ItemComparer.Compare, itemData.Tutorial, itemData2.Tutorial, isAscending: false).Add(ItemComparer.Compare, equipItemData.ReqLevel, equipItemData2.ReqLevel, !SortByEquipLevel.IsUp).Add(ItemComparer.Compare_ItemGrade, itemData.Grade, itemData2.Grade, !SortByEquipGrade.IsUp)
			.Add(ItemComparer.Compare_WeaponType, equipItemData.PartType, equipItemData2.PartType, isAscending: true)
			.Add(ItemComparer.Compare, equipItemData.SubType, equipItemData2.SubType, isAscending: true)
			.Add(ItemComparer.Compare, itemData.Index, itemData2.Index, isAscending: true)
			.Add(ItemComparer.Compare, equipItemInfo.Star, equipItemInfo2.Star, SortByEquipStar.IsUp)
			.Add(ItemComparer.Compare, equipItemInfo.Level, equipItemInfo2.Level, isAscending: false)
			.Add(ItemComparer.Compare_CreateTime, equipItemInfo.CreateDateTime, equipItemInfo2.CreateDateTime, isAscending: true);
		return sortClass.Sort();
	}

	public void OnEquipMenuToggleChanged()
	{
		UIHelper.EnableGameObject(HeroEquipRoot, (bool)HeroEquipToggle && HeroEquipToggle.value);
		UIHelper.EnableGameObject(InventoryEquipRoot, (bool)InventoryEquipToggle && InventoryEquipToggle.value);
	}

	void ItemIconComponent.Listener.OnClick(ItemIconComponent itemIconComponent)
	{
		OnClickItemIconComponent(itemIconComponent);
	}

	void HeroEquipListComponent.Listener.OnClick(HeroPortraitComponent component)
	{
	}

	void HeroEquipListComponent.Listener.OnClick(ItemIconComponent component)
	{
		OnClickItemIconComponent(component);
	}

	void HeroEquipListComponent.Listener.OnClickThis(int heroIndex)
	{
	}

	bool IUserObserver.IsGlobal()
	{
		return false;
	}

	public virtual void OnChanged(NGame2.NAccount.UserEquipItemManager.EquipStatInfo changedInfo)
	{
		if (changedInfo == null)
		{
			return;
		}
		NGame2.NAccount.UserEquipItemManager.EquipStatType statType = changedInfo.StatType;
		EquipItemInfo equipItemInfo = changedInfo.EquipItemInfo;
		if (statType == NGame2.NAccount.UserEquipItemManager.EquipStatType.EquippedHeroIndex)
		{
			long beforeHeroIndex = changedInfo.BeforeValue;
			long afterHeroIndex = changedInfo.AfterValue;
			List<HeroEquipListComponent> list = _heroEquipListComponentList.FindAll((HeroEquipListComponent e) => e.HeroIndex == beforeHeroIndex || e.HeroIndex == afterHeroIndex);
			if (list != null && __Extension_Method_ICollection__.COUNT(list) > 0)
			{
				list.ForEach(delegate(HeroEquipListComponent e)
				{
					e.Refresh();
				});
			}
			if (afterHeroIndex > 0)
			{
				RemoveInventoryItemComponent(equipItemInfo);
				InventoryEquipItemGrid.Grid.Reposition();
			}
			else if (!_inventoryItemComponentList.Exists((ItemIconComponent e) => e.EquipItemInfo == equipItemInfo))
			{
				CreateInventoryItemComponent(equipItemInfo, noTechnomagic: true);
				InventoryEquipItemGrid.Grid.Reposition();
			}
			return;
		}
		for (int num = 0; num < __Extension_Method_ICollection__.COUNT(_heroEquipListComponentList); num++)
		{
			_heroEquipListComponentList[num].ForEachItemComponent(delegate(ItemIconComponent itemComponent)
			{
				if (itemComponent.EquipItemInfo == equipItemInfo)
				{
					itemComponent.Refresh();
					PostProcessItemComponent(itemComponent);
				}
			});
		}
		for (int num2 = 0; num2 < __Extension_Method_ICollection__.COUNT(_inventoryItemComponentList); num2++)
		{
			ItemIconComponent itemIconComponent = _inventoryItemComponentList[num2];
			if (itemIconComponent.EquipItemInfo == equipItemInfo)
			{
				itemIconComponent.Refresh();
				PostProcessItemComponent(itemIconComponent);
			}
		}
	}

	private static int CompareValue<T>(bool isAscending, T a, T b) where T : IComparable
	{
		return HeroEquipAwaken.CompareValue(isAscending ? HeroEquipAwaken.ESortType.Down : HeroEquipAwaken.ESortType.Up, a, b);
	}

	protected ItemIconComponent GetEquipItemIconComponent(int equipItemSlotIndex)
	{
		ItemIconComponent itemIconComponent = null;
		foreach (HeroEquipListComponent heroEquipListComponent in _heroEquipListComponentList)
		{
			ItemIconComponent itemIconComponent2 = heroEquipListComponent.FindEquipItem(equipItemSlotIndex);
			if (itemIconComponent2 != null)
			{
				itemIconComponent = itemIconComponent2;
				break;
			}
		}
		if (itemIconComponent == null)
		{
			foreach (ItemIconComponent inventoryItemComponent in _inventoryItemComponentList)
			{
				if (inventoryItemComponent.EquipItemInfo != null && inventoryItemComponent.EquipItemInfo.SlotIndex == equipItemSlotIndex)
				{
					itemIconComponent = inventoryItemComponent;
					break;
				}
			}
		}
		return itemIconComponent;
	}

	public virtual void SetEvent(EventBonusType type)
	{
		DropBonusEvent effectiveEvent = GlobalAutoBehaviour<DropBonusEventManager>.instance.GetEffectiveEvent(type, null);
		ShowEventRoot(effectiveEvent != null);
	}

	public virtual void ShowEventRoot(bool isShow)
	{
		UIHelper.EnableGameObject(EventRoot, isShow);
		UIHelper.EnableGameObject(EventButtonImage, isShow);
	}
}

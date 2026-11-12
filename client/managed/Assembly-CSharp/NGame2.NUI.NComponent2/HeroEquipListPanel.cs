using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NDispatchBattle;
using NGame2.NEventTrigger;
using NGame2.NScene.NPreloader;
using NGame2.NUI.NHelper.NItem;
using NGame2.NUI.NManager;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NWindow;
using NShared;
using NShared.NEvent;
using NVespa;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using NVespa.NUtil;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class HeroEquipListPanel : MonoBehaviour, IHeroPanel, EquipListManager.Listener, EquipmentItemFilter.Listener, HeroPresetStorageWebManager.ILoadPresetListener, IUserObserver<NGame2.NAccount.UserHeroManager.ChangeHeroInfo>, IUserObserver, IUserObserver<NGame2.NAccount.UserEquipItemManager.EquipStatInfo>, IUserObserver<ItemInfo>, IUserObserver<NGame2.NAccount.UserSoulWeaponManager.SoulWeaponChangedInfo>, IInspectorGuiDrawer
{
	public interface Listener
	{
		void OnClickItemIcon(ItemIconComponent This);

		void OnEquipPartChanged(EquipPartType equipPartType);

		void OnAutoEquip(HeroEquipListPanel caller);

		void OnAutoUnequip(HeroEquipListPanel caller);

		bool NewItemExist(ItemType itemType, EquipPartType partType);

		void OnClickOpenRecommendEquipPopup();
	}

	[Serializable]
	public class Category
	{
		public ItemType itemType;

		public EquipPartType partType;

		public UIWidget WidgetNew;
	}

	[Serializable]
	public class EquipPartOrder
	{
		public EquipPartType Type;

		public string TitleKey;
	}

	public const string PrefabPath = "Resources/Prefab2/Component/Hero/HeroEquipListPanel";

	public static PrefabAuto<HeroEquipListPanel> PrefabAuto = new PrefabAuto<HeroEquipListPanel>("Resources/Prefab2/Component/Hero/HeroEquipListPanel");

	public UIButton AutoEquipButton;

	public UIButton AutoUnequipButton;

	public GameObject SortedButttonRoot;

	public UILabel EmptyInventoryGuideLabel;

	public CachedHeroInfo CachedHeroInfo;

	protected EquipPartType SelectEquipPart = EquipPartType.Weapon;

	protected Listener _listener;

	public EquipListManager equipListManager;

	private EquipmentItemFilterManager _equipmentItemFilterManager = new EquipmentItemFilterManager();

	public EquipPartOrder[] EquipPartTypeOrderList;

	private bool _isDirty;

	private bool _isSkipRefreshCheck;

	private void Awake()
	{
	}

	public void Open(CachedHeroInfo cachedHeroInfo, EquipPartType equipPart, Listener listener)
	{
		Init(cachedHeroInfo, equipPart, listener);
	}

	public void Init(CachedHeroInfo cachedHeroInfo, EquipPartType equipPart, Listener listener)
	{
		Reset();
		CachedHeroInfo = cachedHeroInfo;
		_listener = listener;
		base.gameObject.SetActive(value: true);
		SelectCategory(equipPart);
		equipListManager.Init(this);
		equipListManager.InitEquipmentItemFilterManager(ref _equipmentItemFilterManager);
		_equipmentItemFilterManager.Init(EquipmentItemFilter.Mode.HeroList, CachedHeroInfo.CreatureData.TagType, SelectEquipPart);
		Refresh();
		UpdateSortedButtonState();
		GlobalAutoBehaviour<AccountManager>.instance.AddUserObserver(this);
		SingletonAuto<HeroPresetStorageWebManager>.instance.AddLoadPresetListener(this);
		_isSkipRefreshCheck = false;
		EnterHeroEquipListPage triggerEvent = new EnterHeroEquipListPage();
		SingletonAuto<EventTriggerManager>.instance.RaiseEvent(triggerEvent);
	}

	public ItemIconComponent GetItemIconComponent(Transform tr)
	{
		if (equipListManager == null)
		{
			return null;
		}
		return equipListManager.GetItemIconComponent(tr);
	}

	public ItemIconComponent GetItemIconComponent(string uid)
	{
		if (equipListManager == null)
		{
			return null;
		}
		return equipListManager.GetItemIconComponent(uid);
	}

	public void ReserveCache(UiPreloader preloader)
	{
	}

	public void Reset()
	{
		_listener = null;
		equipListManager.ClearItemList();
	}

	public void ResetScroll()
	{
		if (equipListManager != null && equipListManager.EquipIconGrid != null)
		{
			SingletonUpdater<Executer>.instance.NextRun(delegate
			{
				equipListManager.EquipIconGrid.ResetScroll();
			});
		}
	}

	public void Close()
	{
		base.gameObject.SetActive(value: false);
		GlobalAutoBehaviour<AccountManager>.instance.RemoveUserObserver(this);
		SingletonAuto<HeroPresetStorageWebManager>.instance.RemoveLoadPresetListener(this);
		Reset();
	}

	public void Refresh(bool immediate = true, bool refreshFilter = true)
	{
		if (!base.gameObject.activeSelf)
		{
			return;
		}
		if (!immediate)
		{
			_isDirty = true;
			return;
		}
		if (refreshFilter)
		{
			equipListManager.InitEquipList(Selector, null, bInitMinSlot: true);
		}
		if (IsInventoryEmpty())
		{
			ShowEmptyInventoryGuide();
		}
		else
		{
			UIHelper.EnableGameObject(EmptyInventoryGuideLabel, enable: false);
		}
		UpdateButtonState();
		Reposition();
		_isDirty = false;
	}

	public void ShowEmptyInventoryGuide()
	{
		string key = (_equipmentItemFilterManager.IsFilterApplied(GetExcepFilterGroupTable()) ? "NOTICE_SORTING_EMPTY" : "NOTICE_INVENTORY_EMPTY");
		EmptyInventoryGuideLabel.text = LocalizationManager.GetStringDirect(key);
		UIHelper.EnableGameObject(EmptyInventoryGuideLabel, enable: true);
	}

	public bool IsInventoryEmpty()
	{
		return equipListManager.GetItemCount() <= 0;
	}

	public void Reposition(CachedHeroInfo cachedHeroInfo = null)
	{
		if (base.gameObject.activeSelf)
		{
			if (cachedHeroInfo != null)
			{
				CachedHeroInfo = cachedHeroInfo;
			}
			equipListManager.UpdateAllItem(UpdateItemState);
			ItemGridSort gridSort = new ItemGridSort(equipListManager, _equipmentItemFilterManager);
			equipListManager.SetGridSort(gridSort);
			UIHelper.RepositionGrid(equipListManager.EquipIconGrid);
		}
	}

	public void Update()
	{
		if (_isDirty)
		{
			_isDirty = false;
			Refresh();
		}
	}

	private void SelectCategory(EquipPartType equipPartType)
	{
		SelectEquipPart = equipPartType;
		OnCategoryChanged();
	}

	private bool Selector(IItemInfo iItemInfo)
	{
		ItemData data = GameRoot.Instance.ItemDataContainer.GetData(iItemInfo.ItemIndex);
		if (data != null && data.Type != ItemType.PunishmentRune)
		{
			if (data.Type == ItemType.Equip)
			{
				EquipItemData subData = data.GetSubData<EquipItemData>();
				if (subData == null || subData.PartType != SelectEquipPart)
				{
					return false;
				}
				if (CachedHeroInfo == null)
				{
					return false;
				}
				CachedHeroInfo.EquipResult equipResult = CachedHeroInfo.CheckEquipResult(subData);
				if (equipResult == CachedHeroInfo.EquipResult.Fail || equipResult == CachedHeroInfo.EquipResult.TagMismatch || equipResult == CachedHeroInfo.EquipResult.CreatureMismatch)
				{
					return false;
				}
				return true;
			}
			return true;
		}
		if (iItemInfo is EquipItemInfo equipItemInfo)
		{
			equipItemInfo.GetInventoryType();
			return false;
		}
		return false;
	}

	public int SortItem(ItemIconComponent comp1, ItemIconComponent comp2)
	{
		if (comp1 == comp2)
		{
			return 0;
		}
		int num = ItemComparer.NullCheck(comp1, comp2);
		EquipItemData equipItemData = comp1.EquipItemData;
		EquipItemData equipItemData2 = comp2.EquipItemData;
		if (num == 0)
		{
			int level = CachedHeroInfo.Level;
			bool a = equipItemData.ReqLevel <= level;
			bool b = equipItemData2.ReqLevel <= level;
			num = ItemComparer.Compare(a, b, isAscending: false);
		}
		if (num == 0 && equipItemData.PartType == EquipPartType.Accessory && equipItemData2.PartType == EquipPartType.Accessory)
		{
			EquipItemInfo equipItemInfoByPartType = CachedHeroInfo.GetEquipItemInfoByPartType(EquipPartType.Accessory);
			if (equipItemInfoByPartType != null)
			{
				int itemIndex = equipItemInfoByPartType.ItemIndex;
				ItemData<EquipItemData> data = GameRoot.Instance.ItemDataContainer.GetData<EquipItemData>(itemIndex);
				if (data != null)
				{
					EquipItemData subData = data.GetSubData<EquipItemData>();
					if (subData != null)
					{
						bool a2 = subData.SubType == equipItemData.SubType;
						bool b2 = subData.SubType == equipItemData2.SubType;
						num = ItemComparer.Compare(a2, b2, isAscending: false);
					}
				}
			}
		}
		if (num == 0)
		{
			num = ItemComparer.Sort(comp1, comp2);
		}
		return num;
	}

	protected virtual void OnCategoryChanged()
	{
		if (_listener != null)
		{
			_listener.OnEquipPartChanged(SelectEquipPart);
		}
		if (equipListManager != null && equipListManager.EquipIconGrid != null)
		{
			equipListManager.EquipIconGrid.ResetScroll();
		}
	}

	void EquipListManager.Listener.OnClickItemIcon(ItemIconComponent This)
	{
		_listener.OnClickItemIcon(This);
	}

	bool IUserObserver.IsGlobal()
	{
		return false;
	}

	void IUserObserver<NGame2.NAccount.UserSoulWeaponManager.SoulWeaponChangedInfo>.OnChanged(NGame2.NAccount.UserSoulWeaponManager.SoulWeaponChangedInfo changeInfo)
	{
		switch (changeInfo.ChangedType)
		{
		case NGame2.NAccount.UserSoulWeaponManager.ESoulWeaponChangedType.Grade:
		case NGame2.NAccount.UserSoulWeaponManager.ESoulWeaponChangedType.ReinforceLevel:
			if (equipListManager.UpdateUserChanged(changeInfo))
			{
				Refresh(immediate: false);
			}
			break;
		case NGame2.NAccount.UserSoulWeaponManager.ESoulWeaponChangedType.EquipItemSlotIndex:
			Refresh(immediate: false);
			break;
		}
	}

	void IUserObserver<NGame2.NAccount.UserEquipItemManager.EquipStatInfo>.OnChanged(NGame2.NAccount.UserEquipItemManager.EquipStatInfo equipStatInfo)
	{
		if (!_isSkipRefreshCheck && equipListManager.UpdateUserChanged(equipStatInfo, Selector))
		{
			Refresh(immediate: false);
		}
	}

	void IUserObserver<ItemInfo>.OnChanged(ItemInfo itemInfo)
	{
		if (equipListManager.UpdateUserChanged(itemInfo, Selector))
		{
			Refresh(immediate: false);
		}
	}

	void IUserObserver<NGame2.NAccount.UserHeroManager.ChangeHeroInfo>.OnChanged(NGame2.NAccount.UserHeroManager.ChangeHeroInfo info)
	{
		if (info.ChangeType == NGame2.NAccount.UserHeroManager.HeroChangeType.HeroLevelup)
		{
			Refresh(immediate: false);
		}
	}

	private void UpdateItemState(ItemIconComponent itemIconComponent)
	{
		if (!(itemIconComponent == null) && itemIconComponent.ItemData != null)
		{
			itemIconComponent.HideCustomLabel();
			itemIconComponent.SetSellSelected(selected: false);
			itemIconComponent.SetBreakSelected(selected: false);
			if (itemIconComponent.ItemData.Type == ItemType.Equip)
			{
				itemIconComponent.UpdateStateByHero(CachedHeroInfo);
			}
		}
	}

	public void OnAutoEquip()
	{
		if (_listener != null)
		{
			_listener.OnAutoEquip(this);
		}
	}

	public void OnAutoUnequip()
	{
		if (_listener != null)
		{
			_listener.OnAutoUnequip(this);
		}
	}

	private void UpdateButtonState()
	{
		bool flag = DispatchHelper.IsDispatchedHero(CachedHeroInfo);
		UIHelper.EnableButton(AutoEquipButton, !flag);
		UIHelper.EnableButton(AutoUnequipButton, !flag);
	}

	private void UpdateSortedButtonState()
	{
		bool enable = _equipmentItemFilterManager.IsFilterApplied(GetExcepFilterGroupTable());
		UIHelper.EnableGameObject(SortedButttonRoot, enable);
	}

	void HeroPresetStorageWebManager.ILoadPresetListener.OnBeforeLoadPreset()
	{
		_isSkipRefreshCheck = true;
	}

	void HeroPresetStorageWebManager.ILoadPresetListener.OnAfterLoadPreset()
	{
		_isSkipRefreshCheck = false;
		Refresh(immediate: false);
	}

	void EquipmentItemFilter.Listener.OnClickOK()
	{
		SelectCategory(_equipmentItemFilterManager.SelectedPartType);
		Refresh();
		UpdateSortedButtonState();
	}

	public void OnClickFilter()
	{
		List<EquipPartType> list = new List<EquipPartType>();
		list.Clear();
		EquipFilterOptionGroupData data = GameRoot.Instance.EquipFilterOptionGroupDataContainer.GetData("EquipPart");
		if (data == null)
		{
			return;
		}
		EquipmentItemFilterManager.OptionFilterTable filterTableByGroup = _equipmentItemFilterManager.GetFilterTableByGroup(data.Index);
		if (filterTableByGroup == null)
		{
			return;
		}
		foreach (int key in filterTableByGroup.GetKeys())
		{
			if (key > 0 && !CachedHeroInfo.IsEquipSlotLocked(((EquipPartType)key).GetEquipSlotIndex()))
			{
				list.Add((EquipPartType)key);
			}
		}
		_equipmentItemFilterManager.SetEquipablePartList(list);
		SceneAutoBehaviour<LobbyManagement>.instance.GetInstance<EquipmentItemFilter>().OpenInHeroList(this, ref _equipmentItemFilterManager, GetExcepFilterGroupTable());
	}

	private HashSet<int> GetExcepFilterGroupTable()
	{
		HashSet<int> hashSet = new HashSet<int>();
		EquipFilterOptionGroupData data = GameRoot.Instance.EquipFilterOptionGroupDataContainer.GetData("CreatureTag");
		if (data != null)
		{
			hashSet.Add(data.Index);
		}
		data = GameRoot.Instance.EquipFilterOptionGroupDataContainer.GetData("EquipPart");
		if (data != null)
		{
			hashSet.Add(data.Index);
		}
		return hashSet;
	}

	public void OnClickRecommendEquipmentButton()
	{
		if (_listener != null)
		{
			_listener.OnClickOpenRecommendEquipPopup();
		}
	}
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NGame2.NAccount;
using NGame2.NDropBonusEvent;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NHelper.NItem;
using NGame2.NUI.NManager;
using NGame2.NUI.NManager.NLobby;
using NShared;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow.NInventory;

[UIEvent(AllowDuringPatchDownloading = true)]
public class InvenList : LobbyFullWindow, EquipListManager.Listener, CraftItemComponent.Listener, EquipmentItemFilter.Listener, IUserObserver<NGame2.NAccount.UserEquipItemManager.EquipStatInfo>, IUserObserver, IUserObserver<NGame2.NAccount.UserItemManager.ItemCountChangedInfo>, IUserObserver<EquipStoreagChangeInfo>, IUserObserver<ItemInfo>, IUserObserver<CraftSlotInfo>, LobbyTopmostMenu.IMiniGuideButtonObserver
{
	public enum EToggle
	{
		None,
		Equip,
		Growth,
		Material,
		ETC,
		Recipe,
		Rune,
		Craft,
		Chest
	}

	public enum SubMenuType
	{
		None,
		EquipTypeSelect,
		EquipBreakSelect,
		ItemBasic,
		ItemSellSelect,
		EquipSellSelect,
		Craft,
		RuneSelect,
		RuneBreakSelect,
		ChestTypeSelect,
		EquipKeepSelect,
		EquipBringSelect,
		EquipFilteredBreakSelect,
		EquipFilteredSellSelect,
		EquipFilteredKeepSelect,
		EquipFilteredBringSelect
	}

	public interface Listener
	{
		void Close();

		void ReOpen();

		void OnClickItemIcon(ItemIconComponent This);

		void OnClickExtend(InvenList caller);

		bool IsNewItem(ItemType[] ItemTypes, EquipPartType[] partTypes, bool isInChestBox);

		void OnClickSellEquip(IEnumerable<EquipItemInfo> equipItemInfos);

		void OnClickSellItem(IEnumerable<int> itemIndices, IEnumerable<int> itemCounts);

		void OnClickBreakEquip(IEnumerable<EquipItemInfo> equipItemInfos);

		void OnClickBreakRune(IEnumerable<ItemInfo> RuneItemInfos, IEnumerable<EquipItemInfo> eqiupItemInfo);

		void OnClickBatchSell();

		void OnClickBatchBreak();

		void OnClickBatchBreakRune();

		void OnAddSlot(int index);

		void OnCraftItem(int index);

		void OnCancelCraft(int index);

		void OnFinishCraft(CraftItemComponent comp);

		void OnGetItem(int index);

		void OnClickExtendChest(InvenList caller);

		void OnClickBatchKeep();

		void OnClickBatchBring();

		void OnClickKeep(IEnumerable<EquipItemInfo> equipItemInfos);

		void OnClickBring(IEnumerable<EquipItemInfo> equipItemInfos);

		void OnClickExtendPunishmentRune(InvenList caller);
	}

	public interface IEquipListListener
	{
		void OnClick(ItemIconComponent component, Action OnComplete = null);

		void UpdateItemIcon(ItemIconComponent component);

		KeyValuePair<EToggle, int> GetAvailableTab();
	}

	[Serializable]
	public class Category
	{
		public EToggle Type;

		public UIToggle Toggle;

		public UIWidget WidgetNew;

		public SubCategory[] SubCategorys;
	}

	[Serializable]
	public class SubCategory
	{
		public UIToggle Toggle;

		public ItemType[] ItemTypes;

		public EquipPartType[] PartTypes;

		public UIWidget WidgetNew;
	}

	[Serializable]
	public class EquipTypeToggleInfo
	{
		public EquipPartType Type;

		public UIToggle Toggle;
	}

	[Serializable]
	public class SubMenuInfo
	{
		public SubMenuType Type;

		public GameObject Root;
	}

	[Serializable]
	public class InventoryCountInfo
	{
		public EToggle Type;

		public int SubCategoryIndex;

		public GameObject SpaceRoot;

		public UILabel CountLabel;

		public UIButton ExtendButton;
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Inventory/InvenList";

	public Category[] Categories;

	public InventoryCountInfo[] InventoryCounts;

	public EquipTypeToggleInfo[] EquipTypeToggles;

	public SubMenuInfo[] SubMenus;

	public UILabel SellItemCountLabel;

	public UILabel EquipSellItemCountLabel;

	public UILabel EquipSellFilteredCountLabel;

	public UILabel BreakItemCountLabel;

	public UILabel BreakItemFilteredCountLabel;

	public UILabel RuneBreakItemCountLabel;

	public UILabel EquipKeepItemCountLabel;

	public UILabel EquipKeepItemFilteredCountLabel;

	public UILabel ChestBringItemCountLabel;

	public UILabel ChestBringItemFilteredCountLabel;

	public UILabel EmptyInventoryGuideLabel;

	public GameObject GridBgRoot;

	public UIGrid InvenCategoryToggleGrid;

	public UIScrollGrid CraftGrid;

	private EToggle _toggle;

	private int _subCategoryIndex;

	private SubMenuType _subMenuType = SubMenuType.EquipTypeSelect;

	private Listener _listener;

	private ItemListListener _itemListListener;

	private bool _isDirty;

	public EquipListManager equipListManager;

	private List<CraftItemComponent> _craftItemList = new List<CraftItemComponent>();

	private bool _pendingInitNewItemCountTag;

	[SerializeField]
	private UIButton BtnInitEquipmentItem;

	[SerializeField]
	private UIButton BtnAllUnEquipmentItem;

	[SerializeField]
	private GameObject GoBgFilter;

	[SerializeField]
	private UIButton BtnBreak;

	[SerializeField]
	private UIButton BtnSell;

	[SerializeField]
	private UIButton BtnKeep;

	[SerializeField]
	private UIButton BtnBring;

	[SerializeField]
	private UIButton BtnInitChestItem;

	[SerializeField]
	private UIButton BtnAllUnChestItem;

	[SerializeField]
	private UIButton BtnRuneAllBreak;

	private EquipmentItemFilterManager _equipmentItemFilterManager = new EquipmentItemFilterManager();

	private EquipmentItemFilterManager _chestItemFilterManager = new EquipmentItemFilterManager();

	[SerializeField]
	private UISprite[] SprEventBonusSell;

	[SerializeField]
	private UISprite[] SprEventBonusBreak;

	public UIButton ButtonRefreshEquip;

	public UIButton ButtonRefreshChest;

	public UIButton ButtonRefreshBreak;

	public UIButton ButtonRefreshSell;

	public UIButton ButtonRefreshKeep;

	public UIButton ButtonRefreshBring;

	public UISprite ButtonSortEquipAccepted;

	public UISprite ButtonSortChestAccepted;

	public UISprite ButtonSortBreakAccepted;

	public UISprite ButtonSortSellAccepted;

	public UISprite ButtonSortKeepAccepted;

	public UISprite ButtonSortBringAccepted;

	public UIButton[] BtnUnSelectAlls;

	private Action _onClosing;

	private Coroutine _coProcessingSelectAll;

	private Coroutine _coProcessingUnselectAll;

	private EquipPartType selectedEquipType { get; set; }

	int LobbyTopmostMenu.IMiniGuideButtonObserver.WindowGuideIndex => 0;

	public override void Awake()
	{
		base.Awake();
		selectedEquipType = EquipPartType.Weapon;
		_equipmentItemFilterManager.Init(EquipmentItemFilter.Mode.Inventory);
		_chestItemFilterManager.Init(EquipmentItemFilter.Mode.Inventory);
		equipListManager.InitEquipmentItemFilterManager(ref _equipmentItemFilterManager);
		equipListManager.InitChestItemFilterManager(ref _chestItemFilterManager);
	}

	public void Init(Listener listener, EToggle toggle = EToggle.Equip, string craftItemCode = null)
	{
		if (craftItemCode != null)
		{
			SceneAutoBehaviour<InventoryManagement>.instance.OpenCraftWindow(craftItemCode);
			return;
		}
		_listener = listener;
		_itemListListener = null;
		_equipmentItemFilterManager.Init(EquipmentItemFilter.Mode.Inventory);
		_chestItemFilterManager.Init(EquipmentItemFilter.Mode.Inventory);
		SelectCategory(toggle, 0);
		ShowRoot(show: true);
		Refresh();
		equipListManager.Init(this);
		equipListManager.InitEquipmentItemFilterManager(ref _equipmentItemFilterManager);
		equipListManager.InitChestItemFilterManager(ref _chestItemFilterManager);
		equipListManager.EquipIconGrid.ScrollView.ResetPosition();
		UIHelper.EnableGameObject(GridBgRoot, enable: false);
		SetEventBonusStatus();
		GlobalAutoBehaviour<AccountManager>.instance.EquipPresetSotrageManger.ObserverManager.AddObserver(this);
	}

	private void Clear()
	{
		_listener = null;
		_itemListListener = null;
		equipListManager.ClearItemList();
	}

	public override void Reset()
	{
		base.Reset();
		Clear();
	}

	public void SetOnClosing(Action onClosing)
	{
		_onClosing = onClosing;
	}

	public override void OnClosing()
	{
		GlobalAutoBehaviour<AccountManager>.instance.EquipPresetSotrageManger.ObserverManager.RemoveObserver(this);
		UpdateNewInTabOnChanged(_toggle);
		Vespa_System_Action_Extension_Method.fire(_onClosing);
		_onClosing = null;
		base.OnClosing();
		Clear();
	}

	public void Refresh(bool immediate = true)
	{
		if (!IsOpen())
		{
			return;
		}
		if (!immediate)
		{
			_isDirty = true;
			return;
		}
		SetInventoryCountLayout(_toggle, _subCategoryIndex);
		UIHelper.EnableGameObject(equipListManager.EquipIconGrid, enable: true);
		UIHelper.EnableGameObject(CraftGrid, enable: false);
		ClearCraftList();
		if (_toggle == EToggle.Equip || (_toggle == EToggle.Rune && _subCategoryIndex == 1))
		{
			equipListManager.InitEquipList(Selector, null, bInitMinSlot: false, (_toggle != EToggle.Equip) ? InventoryType.PunishmentRune : InventoryType.Inventory);
			ItemGridSort gridSort = new ItemGridSort(equipListManager, equipListManager._equipmentItemFilterManager);
			equipListManager.SetGridSort(gridSort);
			if (ButtonSortEquipAccepted != null)
			{
				UpdateSortFilterButtonState(equipListManager._equipmentItemFilterManager, ButtonSortEquipAccepted);
			}
			if (ButtonSortKeepAccepted != null)
			{
				UpdateSortFilterButtonState(equipListManager._equipmentItemFilterManager, ButtonSortKeepAccepted);
			}
			if (ButtonSortBreakAccepted != null)
			{
				UpdateSortFilterButtonState(equipListManager._equipmentItemFilterManager, ButtonSortBreakAccepted);
			}
			if (ButtonSortSellAccepted != null)
			{
				UpdateSortFilterButtonState(equipListManager._equipmentItemFilterManager, ButtonSortSellAccepted);
			}
		}
		else if (_toggle == EToggle.Chest)
		{
			equipListManager.InitEquipList(Selector, null, bInitMinSlot: false, InventoryType.Chest);
			ItemGridSort gridSort2 = new ItemGridSort(equipListManager, equipListManager._chestItemFilterManager);
			equipListManager.SetGridSort(gridSort2);
			if (ButtonSortBringAccepted != null)
			{
				UpdateSortFilterButtonState(equipListManager._chestItemFilterManager, ButtonSortBringAccepted);
			}
			if (ButtonSortChestAccepted != null)
			{
				UpdateSortFilterButtonState(equipListManager._chestItemFilterManager, ButtonSortChestAccepted);
			}
		}
		else
		{
			equipListManager.InitItemList(Selector, null);
			equipListManager.SetSortFunc(ItemComparer.Sort);
		}
		Reposition();
		UpdateFilterAppliedState();
		SingletonUpdater<GCManager>.instance.IPhoneLowMemCollectLite();
		_isDirty = false;
	}

	private void UpdateSortFilterButtonState(EquipmentItemFilterManager filterManager, UISprite buttonSprite)
	{
		if (filterManager != null && !(buttonSprite == null))
		{
			buttonSprite.gameObject.SetActive(filterManager.IsFilterApplied());
		}
	}

	public void SetInventoryCountLayout(EToggle toggle, int subCategoryIndex)
	{
		InventoryCountInfo[] inventoryCounts = InventoryCounts;
		foreach (InventoryCountInfo inventoryCountInfo in inventoryCounts)
		{
			if (inventoryCountInfo.Type == toggle && inventoryCountInfo.SubCategoryIndex == subCategoryIndex)
			{
				UIHelper.EnableGameObject(inventoryCountInfo.SpaceRoot, enable: true);
			}
			else
			{
				UIHelper.EnableGameObject(inventoryCountInfo.SpaceRoot, enable: false);
			}
		}
	}

	public void Reposition()
	{
		if (IsOpen())
		{
			UpdateItemList();
			UpdateTabNewState();
			UpdateToggleState();
			UpdateInventoryItemCount();
		}
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		if (_isDirty)
		{
			_isDirty = false;
			Refresh();
		}
		if (_pendingInitNewItemCountTag)
		{
			_pendingInitNewItemCountTag = false;
			UpdateTabNewState();
		}
		SetEventBonusStatus();
	}

	public void OnClickClose()
	{
		if (_listener != null)
		{
			_listener.Close();
		}
		NotifyClose();
	}

	public override void OnSystemBackButton()
	{
		OnClickClose();
	}

	public override void ReOpen()
	{
		base.ReOpen();
		if (_listener != null)
		{
			_listener.ReOpen();
		}
	}

	public void AddEquipIconListener()
	{
		IEquipListListener itemListListener = _itemListListener;
		if (itemListListener != null)
		{
			KeyValuePair<EToggle, int> availableTab = itemListListener.GetAvailableTab();
			EToggle key = availableTab.Key;
			int value = availableTab.Value;
			if (key != _toggle && value != _subCategoryIndex)
			{
				SelectCategory(key, value);
			}
			UpdateToggleState();
			UpdateItemList();
		}
	}

	public void RemoveEquipIconListener()
	{
		_itemListListener = null;
		UpdateToggleState();
		UpdateItemList();
	}

	public void UpdateTabNewState()
	{
		Category[] categories = Categories;
		foreach (Category category in categories)
		{
			bool flag = false;
			SubCategory[] subCategorys = category.SubCategorys;
			foreach (SubCategory subCategory in subCategorys)
			{
				bool flag2 = _listener.IsNewItem(subCategory.ItemTypes, subCategory.PartTypes, category.Type == EToggle.Chest);
				UIHelper.EnableGameObject(subCategory.WidgetNew, flag2);
				flag = flag || flag2;
			}
			if (category.WidgetNew != null)
			{
				UIHelper.EnableGameObject(category.WidgetNew, flag);
			}
		}
	}

	private void SelectCategory(EToggle toggle, int index)
	{
		Category[] categories = Categories;
		foreach (Category category in categories)
		{
			if (category.Toggle != null)
			{
				category.Toggle.value = toggle == category.Type;
				for (int j = 0; j < category.SubCategorys.Length; j++)
				{
					UIHelper.EnableGameObject(category.SubCategorys[j].Toggle, category.Toggle.value);
					if (_toggle != toggle)
					{
						index = 0;
					}
				}
			}
			for (int k = 0; k < category.SubCategorys.Length; k++)
			{
				SubCategory subCategory = category.SubCategorys[k];
				if ((bool)subCategory.Toggle)
				{
					subCategory.Toggle.value = category.Type == toggle && k == index;
				}
				if (category.Type == toggle && k == index)
				{
					if (subCategory.PartTypes.Length == 0)
					{
						selectedEquipType = EquipPartType.None;
					}
					else
					{
						selectedEquipType = subCategory.PartTypes[0];
					}
				}
			}
		}
		_toggle = toggle;
		_subCategoryIndex = index;
		if (InvenCategoryToggleGrid != null)
		{
			InvenCategoryToggleGrid.repositionNow = true;
			InvenCategoryToggleGrid.Reposition();
		}
		UpdateToggleState(toggle);
	}

	private void UpdateToggleState(EToggle toggle)
	{
		switch (toggle)
		{
		case EToggle.Equip:
			ChangeSubMenuType(SubMenuType.EquipTypeSelect);
			break;
		case EToggle.Chest:
			ChangeSubMenuType(SubMenuType.ChestTypeSelect);
			break;
		case EToggle.Growth:
		case EToggle.Material:
		case EToggle.ETC:
		case EToggle.Recipe:
			ChangeSubMenuType(SubMenuType.ItemBasic);
			break;
		case EToggle.Rune:
			ChangeSubMenuType(SubMenuType.RuneSelect);
			break;
		default:
			ResetSubMenu();
			break;
		}
	}

	private Category GetCategory(EToggle toggle)
	{
		Category[] categories = Categories;
		foreach (Category category in categories)
		{
			if (category.Type == toggle)
			{
				return category;
			}
		}
		return null;
	}

	private void UpdateToggleState()
	{
		IEquipListListener itemListListener = _itemListListener;
		EToggle eToggle = EToggle.None;
		int num = 0;
		if (itemListListener != null)
		{
			KeyValuePair<EToggle, int> availableTab = itemListListener.GetAvailableTab();
			eToggle = availableTab.Key;
			num = availableTab.Value;
		}
		Category[] categories = Categories;
		foreach (Category category in categories)
		{
			if (category == null)
			{
				continue;
			}
			if (category.Toggle != null)
			{
				bool flag = itemListListener == null || eToggle == category.Type;
				UISprite[] componentsInChildren = category.Toggle.GetComponentsInChildren<UISprite>();
				if (__Extension_Method_ICollection__.COUNT(componentsInChildren) > 0)
				{
					UISprite[] array = componentsInChildren;
					for (int j = 0; j < array.Length; j++)
					{
						array[j].isGrayscale = !flag;
					}
				}
			}
			for (int k = 0; k < category.SubCategorys.Length; k++)
			{
				SubCategory subCategory = category.SubCategorys[k];
				if (subCategory.Toggle == null)
				{
					continue;
				}
				bool flag2 = itemListListener == null || (eToggle == category.Type && num == k);
				UISprite[] componentsInChildren2 = subCategory.Toggle.GetComponentsInChildren<UISprite>();
				if (__Extension_Method_ICollection__.COUNT(componentsInChildren2) > 0)
				{
					UISprite[] array = componentsInChildren2;
					for (int j = 0; j < array.Length; j++)
					{
						array[j].isGrayscale = !flag2;
					}
				}
			}
		}
	}

	private void UpdateNewInTabOnChanged(EToggle type)
	{
		if (type == EToggle.Equip || type == EToggle.Chest)
		{
			return;
		}
		List<ItemIconComponent> list = equipListManager.GetitemIconContainer();
		int i = 0;
		for (int num = __Extension_Method_ICollection__.COUNT(list); i < num; i++)
		{
			ItemIconComponent itemIconComponent = list[i];
			if (!(itemIconComponent == null))
			{
				GlobalAutoBehaviour<AccountManager>.instance.ItemManager.RemoveAddedItemIndex(itemIconComponent.ItemData.Index);
			}
		}
	}

	public void OnToggleChanged(UIToggle caller)
	{
		EToggle eToggle = EToggle.None;
		int num = -1;
		bool flag = true;
		Category[] categories = Categories;
		foreach (Category category in categories)
		{
			bool flag2 = true;
			if (category.Toggle != null)
			{
				flag2 = category.Toggle.value;
				if (flag2)
				{
					eToggle = category.Type;
				}
			}
			for (int j = 0; j < category.SubCategorys.Length; j++)
			{
				SubCategory subCategory = category.SubCategorys[j];
				if (caller == subCategory.Toggle && subCategory.Toggle.value && flag2)
				{
					eToggle = category.Type;
					num = j;
				}
			}
		}
		if (_toggle == eToggle && num == -1)
		{
			flag = false;
			num = _subCategoryIndex;
		}
		else if (_toggle == eToggle && _subCategoryIndex == num)
		{
			return;
		}
		if (eToggle == EToggle.None)
		{
			return;
		}
		IEquipListListener itemListListener = _itemListListener;
		if (itemListListener != null)
		{
			KeyValuePair<EToggle, int> availableTab = itemListListener.GetAvailableTab();
			if (availableTab.Key == eToggle && availableTab.Value == num)
			{
				return;
			}
			categories = Categories;
			foreach (Category category2 in categories)
			{
				if (category2.Toggle != null)
				{
					category2.Toggle.value = category2.Type == _toggle;
				}
				for (int k = 0; k < category2.SubCategorys.Length; k++)
				{
					if (category2.SubCategorys[k].Toggle != null)
					{
						category2.SubCategorys[k].Toggle.value = category2.Type == _toggle && k == _subCategoryIndex;
					}
				}
			}
		}
		else
		{
			Category category3 = GetCategory(_toggle);
			UpdateNewInTabOnChanged(category3.Type);
			if (equipListManager != null && equipListManager.EquipIconGrid != null)
			{
				equipListManager.EquipIconGrid.ResetScroll();
			}
			SelectCategory(eToggle, num);
			if (flag)
			{
				Refresh(immediate: false);
			}
		}
	}

	void EquipListManager.Listener.OnClickItemIcon(ItemIconComponent This)
	{
		IEquipListListener itemListListener = _itemListListener;
		if (itemListListener != null)
		{
			itemListListener.OnClick(This);
		}
		else if (_listener != null)
		{
			_listener.OnClickItemIcon(This);
		}
		UpdateUnselectButtonsState();
		UpdateTabNewState();
	}

	bool IUserObserver.IsGlobal()
	{
		return false;
	}

	void IUserObserver<NGame2.NAccount.UserEquipItemManager.EquipStatInfo>.OnChanged(NGame2.NAccount.UserEquipItemManager.EquipStatInfo equipStatInfo)
	{
		if (equipListManager.UpdateUserChanged(equipStatInfo, Selector))
		{
			Refresh(immediate: false);
		}
		else
		{
			_pendingInitNewItemCountTag = true;
		}
	}

	void IUserObserver<EquipStoreagChangeInfo>.OnChanged(EquipStoreagChangeInfo changedInfo)
	{
		if (changedInfo != null)
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
		else
		{
			_pendingInitNewItemCountTag = true;
		}
	}

	void IUserObserver<CraftSlotInfo>.OnChanged(CraftSlotInfo changedInfo)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(_craftItemList))
		{
			return;
		}
		foreach (CraftItemComponent craftItem in _craftItemList)
		{
			craftItem.OnChanged(changedInfo);
		}
	}

	private bool isExtendable(InventoryType inventory)
	{
		int extendMaxCount = GameRoot.Instance.InventoryExtendDataContainer.GetExtendMaxCount(inventory);
		int num = 0;
		if (inventory == InventoryType.Inventory)
		{
			num = GlobalAutoBehaviour<AccountManager>.instance.UserManager.MiscInfo.InventoryExtend;
		}
		if (inventory == InventoryType.Chest)
		{
			num = GlobalAutoBehaviour<AccountManager>.instance.UserManager.MiscInfo.ChestExtend;
		}
		if (inventory == InventoryType.PunishmentRune)
		{
			num = GlobalAutoBehaviour<AccountManager>.instance.UserManager.MiscInfo.PunishmentRuneStorageSlotExtend;
		}
		return num < extendMaxCount;
	}

	public void OnClickExtend()
	{
		if ((_itemListListener == null || _itemListListener.GetTotalSelectedCount() <= 0) && _listener != null)
		{
			if (isExtendable(InventoryType.Inventory))
			{
				_listener.OnClickExtend(this);
				return;
			}
			string text = LocalizationManager.GetString(NShared.NLocalization.Common.WARNING);
			string stringDirect = LocalizationManager.GetStringDirect("COMMON_LACK_SLOT");
			MessagePopup.Open(text, stringDirect);
		}
	}

	public void OnClickExtendChest()
	{
		if ((_itemListListener == null || _itemListListener.GetTotalSelectedCount() <= 0) && _listener != null)
		{
			if (isExtendable(InventoryType.Chest))
			{
				_listener.OnClickExtendChest(this);
				return;
			}
			string text = LocalizationManager.GetString(NShared.NLocalization.Common.WARNING);
			string stringDirect = LocalizationManager.GetStringDirect("COMMON_LACK_CHEST_SLOT");
			MessagePopup.Open(text, stringDirect);
		}
	}

	public void OnClickExtendPunishmentRune()
	{
		if ((_itemListListener == null || _itemListListener.GetTotalSelectedCount() <= 0) && _listener != null)
		{
			if (isExtendable(InventoryType.PunishmentRune))
			{
				_listener.OnClickExtendPunishmentRune(this);
				return;
			}
			string text = LocalizationManager.GetString(NShared.NLocalization.Common.WARNING);
			string stringDirect = LocalizationManager.GetStringDirect("RESULT_PUNISHMENTRUNE_SLOT_CHEST_FULL");
			MessagePopup.Open(text, stringDirect);
		}
	}

	public void UpdateItemList()
	{
		IEquipListListener itemListListener = _itemListListener;
		if (itemListListener != null)
		{
			equipListManager.UpdateAllItem(itemListListener.UpdateItemIcon);
		}
		else
		{
			equipListManager.UpdateAllItem(UpdateItemState);
		}
	}

	public bool IsInventoryEmpty()
	{
		int num;
		if (_toggle == EToggle.Chest || _toggle == EToggle.Equip || (_toggle == EToggle.Rune && _subCategoryIndex == 1))
		{
			num = equipListManager.GetAllSlotItemCount();
		}
		else
		{
			num = GlobalAutoBehaviour<AccountManager>.instance.ItemManager.SelectItemList(Selector).Count;
			if (num <= 0)
			{
				return equipListManager.CheckHaveEventItem(Selector);
			}
		}
		return num <= 0;
	}

	public void ShowEmptyInventoryGuide()
	{
		string key = ((_toggle == EToggle.Chest) ? "NOTICE_CHEST_EMPTY" : "NOTICE_INVENTORY_EMPTY");
		if ((_equipmentItemFilterManager.IsFilterApplied() && _toggle == EToggle.Equip) || (_chestItemFilterManager.IsFilterApplied() && _toggle == EToggle.Chest))
		{
			key = "NOTICE_SORTING_EMPTY";
		}
		else if (_toggle == EToggle.Rune && _subCategoryIndex == 1)
		{
			key = "INVENTORY_NO_PUNISHMENTRUNE";
		}
		EmptyInventoryGuideLabel.text = LocalizationManager.GetStringDirect(key);
		UIHelper.EnableGameObject(EmptyInventoryGuideLabel, enable: true);
	}

	public void UpdateInventoryItemCount()
	{
		if (IsInventoryEmpty())
		{
			ShowEmptyInventoryGuide();
		}
		else
		{
			UIHelper.EnableGameObject(EmptyInventoryGuideLabel, enable: false);
		}
		InventoryCountInfo[] inventoryCounts = InventoryCounts;
		foreach (InventoryCountInfo inventoryCountInfo in inventoryCounts)
		{
			if (inventoryCountInfo.Type == EToggle.Equip && inventoryCountInfo.SubCategoryIndex == 0)
			{
				UIHelper.SetLabelText(inventoryCountInfo.CountLabel, equipListManager.GetAllSlotItemCount() + " / " + GlobalAutoBehaviour<AccountManager>.instance.GetMaxEquipItemCount());
			}
			if (inventoryCountInfo.Type == EToggle.Chest && inventoryCountInfo.SubCategoryIndex == 0)
			{
				UIHelper.SetLabelText(inventoryCountInfo.CountLabel, equipListManager.GetAllSlotItemCount() + " / " + GlobalAutoBehaviour<AccountManager>.instance.GetMaxChestItemCount());
			}
			if (inventoryCountInfo.Type == EToggle.Rune && inventoryCountInfo.SubCategoryIndex == 1)
			{
				UIHelper.SetLabelText(inventoryCountInfo.CountLabel, equipListManager.GetAllSlotItemCount() + " / " + GlobalAutoBehaviour<AccountManager>.instance.GetMaxPunishmentRuneItemCount());
			}
		}
	}

	private bool Selector(IItemInfo iItemInfo)
	{
		ItemData data = GameRoot.Instance.ItemDataContainer.GetData(iItemInfo.ItemIndex);
		Category category = GetCategory(_toggle);
		EquipItemInfo equipItemInfo = iItemInfo as EquipItemInfo;
		if (category.Type == EToggle.Equip && equipItemInfo != null && equipItemInfo.GetInventoryType() != InventoryType.Inventory)
		{
			return false;
		}
		if (category.Type == EToggle.Chest && equipItemInfo != null && equipItemInfo.GetInventoryType() != InventoryType.Chest)
		{
			return false;
		}
		if (data != null && category.SubCategorys.Length != 0)
		{
			SubCategory subCategory = category.SubCategorys[_subCategoryIndex];
			if (subCategory.ItemTypes != null && subCategory.ItemTypes.Contains(data.Type))
			{
				if (data.Type == ItemType.Equip)
				{
					EquipItemData subData = data.GetSubData<EquipItemData>();
					if (subData != null && subCategory.PartTypes != null)
					{
						return subCategory.PartTypes.Contains(subData.PartType);
					}
					return false;
				}
				if (data.Type == ItemType.PunishmentRune)
				{
					if (equipItemInfo != null)
					{
						return !equipItemInfo.IsEquipped;
					}
					return false;
				}
				return true;
			}
		}
		return false;
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
				itemIconComponent.UpdateStateByHero(null);
			}
			else
			{
				itemIconComponent.SetSelected(selected: false);
			}
		}
	}

	private void SetEquipTypeToggle(EquipPartType equipType)
	{
		EquipTypeToggleInfo[] equipTypeToggles = EquipTypeToggles;
		foreach (EquipTypeToggleInfo equipTypeToggleInfo in equipTypeToggles)
		{
			equipTypeToggleInfo.Toggle.Set(equipType == equipTypeToggleInfo.Type);
		}
	}

	public void OnEquipTypeToggleChanged()
	{
		EquipPartType equipPartType = EquipPartType.None;
		EquipTypeToggleInfo[] equipTypeToggles = EquipTypeToggles;
		foreach (EquipTypeToggleInfo equipTypeToggleInfo in equipTypeToggles)
		{
			if (equipTypeToggleInfo.Toggle.value)
			{
				equipPartType = equipTypeToggleInfo.Type;
				break;
			}
		}
		if (equipPartType != EquipPartType.None && selectedEquipType != equipPartType)
		{
			selectedEquipType = equipPartType;
			ItemGridSort gridSort = new ItemGridSort(equipListManager, equipListManager._equipmentItemFilterManager);
			equipListManager.SetGridSort(gridSort);
			Reposition();
		}
	}

	private void EnableSubMenu(SubMenuType type)
	{
		SubMenuInfo[] subMenus = SubMenus;
		foreach (SubMenuInfo subMenuInfo in subMenus)
		{
			UIHelper.EnableGameObject(subMenuInfo.Root, subMenuInfo.Type == type);
		}
	}

	private void ChangeSubMenuType(SubMenuType type)
	{
		_subMenuType = type;
		ResetSubMenu();
	}

	private void ResetSubMenu()
	{
		EnableSubMenu(_subMenuType);
		switch (_subMenuType)
		{
		case SubMenuType.EquipTypeSelect:
		case SubMenuType.EquipBreakSelect:
		case SubMenuType.EquipSellSelect:
		case SubMenuType.EquipKeepSelect:
		case SubMenuType.EquipFilteredBreakSelect:
		case SubMenuType.EquipFilteredSellSelect:
		case SubMenuType.EquipFilteredKeepSelect:
			SetEquipTypeToggle(selectedEquipType);
			UpdateUnselectButtonsState();
			break;
		case SubMenuType.ChestTypeSelect:
		case SubMenuType.EquipBringSelect:
		case SubMenuType.EquipFilteredBringSelect:
			UpdateUnselectButtonsState();
			break;
		case SubMenuType.ItemBasic:
		case SubMenuType.ItemSellSelect:
			UpdateUnselectButtonsState();
			break;
		case SubMenuType.RuneSelect:
		case SubMenuType.RuneBreakSelect:
			UpdateUnselectButtonsState();
			break;
		}
		if (BtnRuneAllBreak != null)
		{
			UIHelper.EnableButton(BtnRuneAllBreak, _toggle != EToggle.Rune || _subCategoryIndex != 1);
		}
		UpdateFilterAppliedState();
	}

	public void OnClickSell()
	{
		if (_toggle == EToggle.Equip)
		{
			if (!_equipmentItemFilterManager.IsFilterApplied())
			{
				ChangeSubMenuType(SubMenuType.EquipSellSelect);
			}
			else
			{
				ChangeSubMenuType(SubMenuType.EquipFilteredSellSelect);
			}
		}
		else
		{
			ChangeSubMenuType(SubMenuType.ItemSellSelect);
		}
		_itemListListener = new ItemListListener(_toggle, _subCategoryIndex);
		_itemListListener.SelectableFunction = delegate(ItemIconComponent component)
		{
			if (component.EquipItemInfo != null)
			{
				if (component.EquipItemInfo.Locked == 0)
				{
					return !component.ItemData.NotForSale;
				}
				return false;
			}
			ItemInfo itemInfo = GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Get(component.ItemIndex);
			return (itemInfo == null || itemInfo.Locked == 0) && !component.ItemData.NotForSale;
		};
		_itemListListener.functionType = ItemListListener.EFunctionType.Sell;
		_itemListListener.OnClickItemIcon = delegate
		{
			UpdateSellCountLabel();
		};
		_itemListListener.MaxTotalSelectCount = int.MaxValue;
		_itemListListener.UpdateFunction = delegate(ItemIconComponent component, bool selected)
		{
			component.ApplyNotForSale(apply: true);
			component.SetSellSelected(selected);
		};
		AddEquipIconListener();
		UpdateSellCountLabel();
		UIHelper.EnableGameObject(GridBgRoot, enable: true);
	}

	public void CloseSubMenu(SubMenuType nextmenuType = SubMenuType.None)
	{
		RemoveEquipIconListener();
		_itemListListener = null;
		if (nextmenuType == SubMenuType.None)
		{
			UpdateToggleState(_toggle);
		}
		else
		{
			ChangeSubMenuType(nextmenuType);
		}
		UIHelper.EnableGameObject(GridBgRoot, enable: false);
	}

	public void OnClickCancelSell()
	{
		CloseSubMenu();
	}

	public void OnClickConfirmSell()
	{
		Listener listener = _listener;
		if (listener == null)
		{
			return;
		}
		List<ItemIconComponent> selectedItemIconList = _itemListListener.SelectedItemIconList;
		if (selectedItemIconList.Count <= 0)
		{
			return;
		}
		selectedItemIconList.RemoveAll((ItemIconComponent e) => e == null);
		if (selectedItemIconList.Count == 0)
		{
			return;
		}
		List<EquipItemInfo> list = selectedItemIconList.Select((ItemIconComponent e) => e.EquipItemInfo).ToList();
		list.RemoveAll((EquipItemInfo e) => e == null);
		if (list.Count > 0)
		{
			listener.OnClickSellEquip(list);
			return;
		}
		List<int> list2 = selectedItemIconList.Select((ItemIconComponent e) => e.ItemData.Index).ToList();
		List<int> list3 = new List<int>();
		int[] array = list2.ToArray();
		foreach (int num2 in array)
		{
			int itemCount = GlobalAutoBehaviour<AccountManager>.instance.ItemManager.GetItemCount(num2);
			if (itemCount <= 0)
			{
				list2.Remove(num2);
			}
			else
			{
				list3.Add(itemCount);
			}
		}
		if (list2.Count != 0)
		{
			listener.OnClickSellItem(list2, list3);
		}
	}

	private void UpdateSellCountLabel()
	{
		int totalSelectedCount = _itemListListener.GetTotalSelectedCount();
		UIHelper.SetLabelText(SellItemCountLabel, $"{totalSelectedCount}");
		UIHelper.SetLabelText(EquipSellItemCountLabel, $"{totalSelectedCount}");
		UIHelper.SetLabelText(EquipSellFilteredCountLabel, $"{totalSelectedCount}");
	}

	private bool IsEnoughEmptySlot(int checkNum)
	{
		if (GlobalAutoBehaviour<AccountManager>.instance.GetMaxEquipItemCount() - equipListManager.GetInventoryItemCount() < checkNum)
		{
			return false;
		}
		return true;
	}

	private void OpenPopupNoticeInvenNotEnough()
	{
		string text = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
		string stringDirect = LocalizationManager.GetStringDirect("RESULT_TYPE_EQUIP_ITEM_FULL");
		MessagePopup.Open(text, stringDirect);
	}

	private bool CheckChestEmptySlot()
	{
		if (GlobalAutoBehaviour<AccountManager>.instance.GetMaxChestItemCount() - equipListManager.GetChestItemCount() > 0)
		{
			return true;
		}
		string text = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
		string stringDirect = LocalizationManager.GetStringDirect("RESULT_TYPE_EQUIP_CHEST_FULL");
		MessagePopup.Open(text, stringDirect);
		return false;
	}

	public void OnClickKeep()
	{
		if (!CheckChestEmptySlot())
		{
			return;
		}
		if (_itemListListener != null && _itemListListener.GetTotalSelectedCount() > 0)
		{
			OnClickConfirmKeep();
			return;
		}
		if (!_equipmentItemFilterManager.IsFilterApplied())
		{
			ChangeSubMenuType(SubMenuType.EquipKeepSelect);
		}
		else
		{
			ChangeSubMenuType(SubMenuType.EquipFilteredKeepSelect);
		}
		_itemListListener = new ItemListListener(_toggle, _subCategoryIndex);
		_itemListListener.SelectableFunction = (ItemIconComponent component) => true;
		int num = GlobalAutoBehaviour<AccountManager>.instance.GetMaxChestItemCount() - equipListManager.GetChestItemCount();
		if (equipListManager.GetInventoryItemCount() < num)
		{
			num = equipListManager.GetInventoryItemCount();
		}
		_itemListListener.functionType = ItemListListener.EFunctionType.Keep;
		_itemListListener.isFiltering = true;
		_itemListListener.OnClickItemIcon = delegate
		{
			UpdateKeepCountLabel();
		};
		_itemListListener.MaxTotalSelectCount = num;
		_itemListListener.UpdateFunction = delegate(ItemIconComponent component, bool selected)
		{
			component.ApplyKeepable();
			component.SetSelected(selected);
		};
		AddEquipIconListener();
		UpdateKeepCountLabel();
		UIHelper.EnableGameObject(GridBgRoot, enable: true);
	}

	public void OnClickBring()
	{
		if (!IsEnoughEmptySlot(1))
		{
			OpenPopupNoticeInvenNotEnough();
			return;
		}
		if (_itemListListener != null && _itemListListener.GetTotalSelectedCount() > 0)
		{
			OnClickConfirmBring();
			return;
		}
		if (ButtonSortBringAccepted != null)
		{
			ButtonSortBringAccepted.gameObject.SetActive(_chestItemFilterManager.IsFilterApplied());
		}
		if (_chestItemFilterManager.IsFilterApplied())
		{
			ChangeSubMenuType(SubMenuType.EquipFilteredBringSelect);
		}
		else
		{
			ChangeSubMenuType(SubMenuType.EquipBringSelect);
		}
		_itemListListener = new ItemListListener(_toggle, _subCategoryIndex);
		_itemListListener.SelectableFunction = (ItemIconComponent component) => true;
		int num = GlobalAutoBehaviour<AccountManager>.instance.GetMaxEquipItemCount() - equipListManager.GetInventoryItemCount();
		if (equipListManager.GetChestItemCount() < num)
		{
			num = equipListManager.GetChestItemCount();
		}
		_itemListListener.functionType = ItemListListener.EFunctionType.Bring;
		_itemListListener.isFiltering = true;
		_itemListListener.OnClickItemIcon = delegate
		{
			UpdateBringCountLabel();
		};
		_itemListListener.MaxTotalSelectCount = num;
		_itemListListener.UpdateFunction = delegate(ItemIconComponent component, bool selected)
		{
			component.SetSelected(selected);
		};
		AddEquipIconListener();
		UpdateBringCountLabel();
		UIHelper.EnableGameObject(GridBgRoot, enable: true);
	}

	private void UpdateKeepCountLabel()
	{
		int totalSelectedCount = _itemListListener.GetTotalSelectedCount();
		int num = GlobalAutoBehaviour<AccountManager>.instance.GetMaxChestItemCount() - equipListManager.GetChestItemCount();
		if (equipListManager.GetInventoryItemCount() < num)
		{
			num = equipListManager.GetInventoryItemCount();
		}
		UIHelper.SetLabelText(EquipKeepItemCountLabel, $"{totalSelectedCount} / {num}");
		UIHelper.SetLabelText(EquipKeepItemFilteredCountLabel, $"{totalSelectedCount} / {num}");
	}

	private void UpdateBringCountLabel()
	{
		int totalSelectedCount = _itemListListener.GetTotalSelectedCount();
		int num = GlobalAutoBehaviour<AccountManager>.instance.GetMaxEquipItemCount() - equipListManager.GetInventoryItemCount();
		if (equipListManager.GetChestItemCount() < num)
		{
			num = equipListManager.GetChestItemCount();
		}
		UIHelper.SetLabelText(ChestBringItemCountLabel, $"{totalSelectedCount} / {num}");
		UIHelper.SetLabelText(ChestBringItemFilteredCountLabel, $"{totalSelectedCount} / {num}");
	}

	public void OnClickCancelKeep()
	{
		CloseSubMenu(SubMenuType.EquipTypeSelect);
	}

	public void OnClickCancelBring()
	{
		CloseSubMenu(SubMenuType.ChestTypeSelect);
	}

	public void OnClickBatchKeep()
	{
		_listener?.OnClickBatchKeep();
	}

	public void OnClickBatchBring()
	{
		_listener?.OnClickBatchBring();
	}

	public void OnClickConfirmKeep()
	{
		Listener listener = _listener;
		if (listener == null)
		{
			return;
		}
		int num = GlobalAutoBehaviour<AccountManager>.instance.GetMaxChestItemCount() - equipListManager.GetChestItemCount();
		List<ItemIconComponent> selectedItemIconList = _itemListListener.SelectedItemIconList;
		if (selectedItemIconList.Count <= 0)
		{
			return;
		}
		selectedItemIconList.RemoveAll((ItemIconComponent e) => e == null);
		if (selectedItemIconList.Count == 0)
		{
			return;
		}
		if (selectedItemIconList.Count > num)
		{
			string text = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
			string stringDirect = LocalizationManager.GetStringDirect("RESULT_TYPE_EQUIP_CHEST_FULL");
			MessagePopup.Open(text, stringDirect);
			return;
		}
		List<EquipItemInfo> list = selectedItemIconList.Select((ItemIconComponent e) => e.EquipItemInfo).ToList();
		list.RemoveAll((EquipItemInfo e) => e == null);
		if (list.Count > 0)
		{
			listener.OnClickKeep(list);
		}
	}

	public void OnClickConfirmBring()
	{
		Listener listener = _listener;
		if (listener == null || _itemListListener == null)
		{
			return;
		}
		List<ItemIconComponent> selectedItemIconList = _itemListListener.SelectedItemIconList;
		int num = GlobalAutoBehaviour<AccountManager>.instance.GetMaxEquipItemCount() - equipListManager.GetInventoryItemCount();
		if (selectedItemIconList.Count <= 0)
		{
			return;
		}
		selectedItemIconList.RemoveAll((ItemIconComponent e) => e == null);
		if (selectedItemIconList.Count == 0)
		{
			return;
		}
		if (selectedItemIconList.Count > num)
		{
			string text = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
			string stringDirect = LocalizationManager.GetStringDirect("RESULT_TYPE_EQUIP_FULL");
			MessagePopup.Open(text, stringDirect);
			return;
		}
		List<EquipItemInfo> list = selectedItemIconList.Select((ItemIconComponent e) => e.EquipItemInfo).ToList();
		list.RemoveAll((EquipItemInfo e) => e == null);
		if (list.Count > 0)
		{
			listener.OnClickBring(list);
		}
	}

	private int GetMaxBreakCount()
	{
		int valueInt = GameRoot.Instance.ConstantDataContainer.GetValueInt("MaxBreakItemCount");
		if (valueInt <= 0)
		{
			return 10;
		}
		return valueInt;
	}

	public void OnClickBreak()
	{
		if (!_equipmentItemFilterManager.IsFilterApplied())
		{
			ChangeSubMenuType(SubMenuType.EquipBreakSelect);
		}
		else
		{
			ChangeSubMenuType(SubMenuType.EquipFilteredBreakSelect);
		}
		_itemListListener = new ItemListListener(_toggle, _subCategoryIndex);
		_itemListListener.SelectableFunction = delegate(ItemIconComponent component)
		{
			if (component.EquipItemInfo != null)
			{
				if (component.EquipItemInfo.IsSoulWeapon())
				{
					return false;
				}
				if (component.EquipItemInfo.Locked == 0)
				{
					return component.ItemData.Breakable;
				}
				return false;
			}
			return component.ItemData.Breakable;
		};
		_itemListListener.functionType = ItemListListener.EFunctionType.Break;
		_itemListListener.OnClickItemIcon = delegate
		{
			UpdateBreakCountLabel();
		};
		_itemListListener.MaxTotalSelectCount = int.MaxValue;
		_itemListListener.UpdateFunction = delegate(ItemIconComponent component, bool selected)
		{
			component.ApplyBreakable(apply: true);
			component.SetBreakSelected(selected);
		};
		AddEquipIconListener();
		UpdateBreakCountLabel();
		UIHelper.EnableGameObject(GridBgRoot, enable: true);
	}

	public void OnClickCancelBreak()
	{
		CloseSubMenu();
	}

	public void OnClickConfirmBreak()
	{
		Listener listener = _listener;
		if (listener == null)
		{
			return;
		}
		List<ItemIconComponent> selectedItemIconList = _itemListListener.SelectedItemIconList;
		if (selectedItemIconList.Count > 0)
		{
			IEnumerable<EquipItemInfo> equipItemInfos = selectedItemIconList.Select((ItemIconComponent e) => e.EquipItemInfo);
			listener.OnClickBreakEquip(equipItemInfos);
		}
	}

	public void OnClickBatchBreak()
	{
		_listener?.OnClickBatchBreak();
	}

	public void OnClickBatchSell()
	{
		_listener?.OnClickBatchSell();
	}

	private void UpdateBreakCountLabel()
	{
		int totalSelectedCount = _itemListListener.GetTotalSelectedCount();
		UIHelper.SetLabelText(BreakItemCountLabel, $"{totalSelectedCount}");
		UIHelper.SetLabelText(BreakItemFilteredCountLabel, $"{totalSelectedCount}");
	}

	private void UpdateBreakStatus()
	{
		_itemListListener = new ItemListListener(_toggle, _subCategoryIndex);
		_itemListListener.SelectableFunction = delegate(ItemIconComponent component)
		{
			if (component.ItemData == null)
			{
				return false;
			}
			if (component.ItemData.Type == ItemType.PunishmentRune)
			{
				if (component.EquipItemInfo != null)
				{
					return component.EquipItemInfo.Locked == 0;
				}
			}
			else
			{
				ItemInfo itemInfo = GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Get(component.ItemIndex);
				if (itemInfo != null)
				{
					return itemInfo.Locked == 0;
				}
			}
			return false;
		};
		_itemListListener.OnClickItemIcon = delegate
		{
			UpdateRuneBreakCountLabel();
		};
		_itemListListener.MaxTotalSelectCount = int.MaxValue;
		_itemListListener.UpdateFunction = delegate(ItemIconComponent component, bool selected)
		{
			component.ApplyBreakable(apply: true);
			component.SetBreakSelected(selected);
		};
		AddEquipIconListener();
		UIHelper.EnableGameObject(GridBgRoot, enable: true);
	}

	private void UpdateRuneBreakCountLabel()
	{
		int totalSelectedCount = _itemListListener.GetTotalSelectedCount();
		UIHelper.SetLabelText(RuneBreakItemCountLabel, $"{totalSelectedCount}");
	}

	public void OnClickRuneBreak()
	{
		ChangeSubMenuType(SubMenuType.RuneBreakSelect);
		UpdateBreakStatus();
		UpdateRuneBreakCountLabel();
	}

	public void OnClickConfirmRuneBreak()
	{
		Listener listener = _listener;
		if (listener == null)
		{
			return;
		}
		List<ItemIconComponent> selectedItemIconList = _itemListListener.SelectedItemIconList;
		if (selectedItemIconList.Count <= 0)
		{
			return;
		}
		if (_toggle == EToggle.Rune && _subCategoryIndex == 1)
		{
			IEnumerable<EquipItemInfo> eqiupItemInfo = selectedItemIconList.Select((ItemIconComponent e) => e.EquipItemInfo);
			listener.OnClickBreakRune(null, eqiupItemInfo);
		}
		else
		{
			IEnumerable<ItemInfo> runeItemInfos = selectedItemIconList.Select((ItemIconComponent e) => GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Get(e.ItemIndex));
			listener.OnClickBreakRune(runeItemInfos, null);
		}
	}

	public void OnClickBatchBreakRune()
	{
		_listener?.OnClickBatchBreakRune();
	}

	public void OnClickBreakHousingFurniture()
	{
		UpdateBreakStatus();
		UpdateBreakCountLabel();
	}

	public void OnClickConfirmHouseObjectBreak()
	{
	}

	private void InitCraftList()
	{
		ClearCraftList();
		int valueInt = GameRoot.Instance.ConstantDataContainer.GetValueInt("CraftSlotCountMax");
		for (int i = 1; i <= valueInt; i++)
		{
			CraftItemComponent craftItemComponent = CraftGrid.AddGridItem(CraftItemComponent.PrefabAuto);
			craftItemComponent.Open(this, i);
			_craftItemList.Add(craftItemComponent);
		}
	}

	private void ClearCraftList()
	{
		FactoryBase.ReleaseAndClearList(_craftItemList);
	}

	[Obsolete]
	private UIButton GetBtnAllEquipmentItem()
	{
		if (BtnAllUnEquipmentItem != null)
		{
			return BtnAllUnEquipmentItem.transform.parent.GetComponent<UIButton>();
		}
		return null;
	}

	[Obsolete]
	private UIButton GetBtnAllChestItem()
	{
		if (BtnAllUnChestItem != null)
		{
			return BtnAllUnChestItem.transform.parent.GetComponent<UIButton>();
		}
		return null;
	}

	public void UpdateFilterAppliedState()
	{
		switch (_subMenuType)
		{
		case SubMenuType.EquipTypeSelect:
			UIHelper.EnableButton(ButtonRefreshEquip, _equipmentItemFilterManager.IsFilterApplied());
			break;
		case SubMenuType.EquipFilteredSellSelect:
			UIHelper.EnableButton(ButtonRefreshSell, _equipmentItemFilterManager.IsFilterApplied());
			break;
		case SubMenuType.EquipFilteredBreakSelect:
			UIHelper.EnableButton(ButtonRefreshBreak, _equipmentItemFilterManager.IsFilterApplied());
			break;
		case SubMenuType.EquipFilteredKeepSelect:
			UIHelper.EnableButton(ButtonRefreshKeep, _equipmentItemFilterManager.IsFilterApplied());
			break;
		case SubMenuType.ChestTypeSelect:
			UIHelper.EnableButton(ButtonRefreshChest, _chestItemFilterManager.IsFilterApplied());
			break;
		case SubMenuType.EquipFilteredBringSelect:
			UIHelper.EnableButton(ButtonRefreshBring, _chestItemFilterManager.IsFilterApplied());
			break;
		}
		switch (_toggle)
		{
		case EToggle.Equip:
			UIHelper.EnableGameObject(GoBgFilter, _equipmentItemFilterManager.IsFilterApplied());
			break;
		case EToggle.Chest:
			UIHelper.EnableGameObject(GoBgFilter, _chestItemFilterManager.IsFilterApplied());
			break;
		default:
			UIHelper.EnableGameObject(GoBgFilter, enable: false);
			break;
		}
	}

	public void InitEquipmentFilter()
	{
		SubMenuType subMenuType = _subMenuType;
		if ((uint)(subMenuType - 12) <= 2u)
		{
			_subMenuType = SubMenuType.EquipTypeSelect;
		}
		CloseSubMenu(_subMenuType);
		_equipmentItemFilterManager.ResetFilterStatus();
		Refresh();
	}

	public void InitChestFilter()
	{
		if (_subMenuType == SubMenuType.EquipFilteredBringSelect)
		{
			_subMenuType = SubMenuType.ChestTypeSelect;
		}
		CloseSubMenu(_subMenuType);
		_chestItemFilterManager.ResetFilterStatus();
		Refresh();
	}

	public int GetEmptyCraftSlotIndex()
	{
		int num = _craftItemList.FindIndex((CraftItemComponent slot) => slot.CanCraftState());
		if (num < 0)
		{
			return num;
		}
		return num + 1;
	}

	public void OnClickInitEquipmentFilter()
	{
		InitEquipmentFilter();
	}

	public void OnClickInitChestFilter()
	{
		InitChestFilter();
	}

	public void OnClickAllEquipmentItem()
	{
		_itemListListener = new ItemListListener(_toggle, _subCategoryIndex);
		_itemListListener.SelectableFunction = (ItemIconComponent component) => component.EquipItemInfo == null || component.EquipItemInfo.Locked == 0;
		_itemListListener.isFiltering = true;
		_itemListListener.functionType = ItemListListener.EFunctionType.Sell;
		_itemListListener.UpdateFunction = delegate(ItemIconComponent component, bool selected)
		{
			if (component.IsSelected() && !selected)
			{
				SetUnSelectedEquipItemIconComponentStatus(component);
			}
			else
			{
				SetSelectedEquipItemIconComponentStatus(component);
			}
		};
		AddEquipIconListener();
		UIHelper.EnableGameObject(GridBgRoot, enable: true);
	}

	public void OnClickAllChestItem()
	{
		_itemListListener = new ItemListListener(_toggle, _subCategoryIndex);
		_itemListListener.SelectableFunction = (ItemIconComponent component) => component.EquipItemInfo != null;
		_itemListListener.isFiltering = true;
		_itemListListener.functionType = ItemListListener.EFunctionType.Bring;
		_itemListListener.UpdateFunction = delegate(ItemIconComponent component, bool selected)
		{
			if (component.IsSelected() && !selected)
			{
				SetUnSelectedChestItemIconComponentStatus(component);
			}
			else
			{
				SetSelectedChestItemIconComponentStatus(component);
			}
		};
		AddEquipIconListener();
		UIHelper.EnableGameObject(GridBgRoot, enable: true);
	}

	public void OnClickAllUnEquipmentItem()
	{
		CloseSubMenu();
		UIHelper.EnableButton(BtnKeep, enable: false, grayscaleIfDisable: true);
	}

	public void OnClickAllUnChestItem()
	{
		CloseSubMenu();
		SetBtnStatusChestItem(status: false);
		UIHelper.EnableButton(BtnBring, enable: false, grayscaleIfDisable: true);
	}

	public void OnClickEquipmentFilter()
	{
		SceneAutoBehaviour<LobbyManagement>.instance.GetInstance<EquipmentItemFilter>().Open(this, ref equipListManager._equipmentItemFilterManager);
	}

	public void OnClickChestFilter()
	{
		SceneAutoBehaviour<LobbyManagement>.instance.GetInstance<EquipmentItemFilter>().Open(this, ref equipListManager._chestItemFilterManager);
	}

	public void OnClickFilterSell()
	{
		_itemListListener.RemoveNotSellItem();
		OnClickConfirmSell();
	}

	public void OnClickFilterBreak()
	{
		_itemListListener.RemoveNotBreakItem();
		OnClickConfirmBreak();
	}

	public void OnClickSelectAll()
	{
		SubMenuType subMenuType = _subMenuType;
		if ((subMenuType == SubMenuType.EquipBringSelect || subMenuType == SubMenuType.EquipFilteredBringSelect) && !IsEnoughEmptySlot(equipListManager.GetAllSlotItemCount()))
		{
			OpenPopupNoticeInvenNotEnough();
			return;
		}
		if (_coProcessingSelectAll != null)
		{
			StopCoroutine(_coProcessingSelectAll);
			_coProcessingSelectAll = null;
		}
		_coProcessingSelectAll = VespaCoroutine.Start(this, CoProcessingSelectAll());
		UpdateUnselectButtonsState();
	}

	public void OnClickUnselectAll()
	{
		if (_coProcessingUnselectAll != null)
		{
			StopCoroutine(_coProcessingUnselectAll);
			_coProcessingUnselectAll = null;
		}
		_coProcessingUnselectAll = VespaCoroutine.Start(this, CoProcessingUnselectAll());
	}

	private void SetUnSelectedEquipItemIconComponentStatus(ItemIconComponent component)
	{
		if (component != null)
		{
			component.SetSelected(selected: false);
			UIHelper.EnableButton(BtnKeep, enable: false, grayscaleIfDisable: true);
			UIHelper.EnableGameObject(BtnAllUnEquipmentItem, _itemListListener.GetTotalSelectedCount() > 0);
			if (_itemListListener.GetTotalSelectedCount() <= 0)
			{
				UIHelper.EnableButton(BtnAllUnEquipmentItem, enable: false);
				CloseSubMenu();
			}
		}
	}

	private void SetSelectedEquipItemIconComponentStatus(ItemIconComponent component)
	{
		if (component != null)
		{
			if (component.EquipItemInfo.Locked != 0)
			{
				component.ApplyEquipItemLocked();
			}
			else if (_itemListListener.CheckNotSellItem(component) && _itemListListener.CheckNotBreakItem(component))
			{
				component.SetDisabled(disabled: true);
			}
			else
			{
				component.SetSelected(selected: true);
			}
			if (!_itemListListener.SelectedItemIconList.Contains(component))
			{
				_itemListListener.SelectedItemIconList.Add(component);
				_itemListListener.SelectedItemIconTable.Add(component);
			}
			UIHelper.EnableButton(BtnKeep, enable: false, grayscaleIfDisable: true);
			UIHelper.EnableGameObject(BtnAllUnEquipmentItem, _itemListListener.GetTotalSelectedCount() > 0);
		}
	}

	private void SetUnSelectedChestItemIconComponentStatus(ItemIconComponent component)
	{
		if (component != null)
		{
			component.SetSelected(selected: false);
			UIHelper.EnableGameObject(BtnAllUnChestItem, _itemListListener.GetTotalSelectedCount() > 0);
			UIHelper.EnableButton(BtnBring, _itemListListener.GetTotalSelectedCount() > 0, grayscaleIfDisable: true);
			if (_itemListListener.GetTotalSelectedCount() <= 0)
			{
				UIHelper.EnableButton(BtnAllUnChestItem, enable: false);
				CloseSubMenu();
			}
		}
	}

	private void SetSelectedChestItemIconComponentStatus(ItemIconComponent component)
	{
		if (_itemListListener != null && component != null)
		{
			component.SetSelected(selected: true);
			if (!_itemListListener.SelectedItemIconList.Contains(component))
			{
				_itemListListener.SelectedItemIconList.Add(component);
				_itemListListener.SelectedItemIconTable.Add(component);
			}
			UIHelper.EnableGameObject(BtnAllUnChestItem, _itemListListener.GetTotalSelectedCount() > 0);
			UIHelper.EnableButton(BtnBring, _itemListListener.GetTotalSelectedCount() > 0, grayscaleIfDisable: true);
		}
	}

	private void SetEventBonusStatus()
	{
		bool enable = GlobalAutoBehaviour<DropBonusEventManager>.instance.IsEffectiveEvent(EventBonusType.BonusEquipSellReward);
		UISprite[] sprEventBonusSell = SprEventBonusSell;
		for (int i = 0; i < sprEventBonusSell.Length; i++)
		{
			UIHelper.EnableGameObject(sprEventBonusSell[i], enable);
		}
		enable = GlobalAutoBehaviour<DropBonusEventManager>.instance.IsEffectiveEvent(EventBonusType.BonusEquipBreakReward);
		sprEventBonusSell = SprEventBonusBreak;
		for (int i = 0; i < sprEventBonusSell.Length; i++)
		{
			UIHelper.EnableGameObject(sprEventBonusSell[i], enable);
		}
	}

	private bool IsSelectedItem()
	{
		if (_itemListListener == null)
		{
			return false;
		}
		if (__Extension_Method_ICollection__.COUNT(_itemListListener.SelectedItemIconTable) <= 0)
		{
			return false;
		}
		return true;
	}

	private void UpdateUnselectButtonsState()
	{
		bool enable = IsSelectedItem();
		int i = 0;
		for (int num = BtnUnSelectAlls.Length; i < num; i++)
		{
			UIButton uIButton = BtnUnSelectAlls[i];
			if (!(uIButton == null))
			{
				UIHelper.EnableGameObject(uIButton, enable);
			}
		}
	}

	private void SetBtnStatusChestItem(bool status)
	{
		UIHelper.EnableGameObject(BtnAllUnChestItem, status);
	}

	private IEnumerator CoProcessingSelectAll()
	{
		bool isReady = true;
		IEquipListListener _equipListListener = _itemListListener;
		if (_itemListListener == null || _equipListListener == null)
		{
			yield break;
		}
		List<Transform> itemList = equipListManager.EquipIconGrid.Grid.GetChildList();
		if (itemList == null)
		{
			yield break;
		}
		WaitUntil waitUntil = new WaitUntil(() => isReady);
		int i = 0;
		int len = itemList.Count;
		while (i < len)
		{
			ItemIconComponent component = itemList[i].GetComponent<ItemIconComponent>();
			if (!(component == null) && !_itemListListener.SelectedItemIconTable.Contains(component))
			{
				if (NeedToWaitPopup(component.EquipItemInfo))
				{
					isReady = false;
					_equipListListener.OnClick(component, delegate
					{
						isReady = true;
					});
					yield return waitUntil;
				}
				else
				{
					_equipListListener.OnClick(component);
				}
			}
			int num = i + 1;
			i = num;
		}
		UpdateUnselectButtonsState();
	}

	private IEnumerator CoProcessingUnselectAll()
	{
		IEquipListListener itemListListener = _itemListListener;
		if (_itemListListener == null || itemListListener == null)
		{
			yield break;
		}
		List<Transform> childList = equipListManager.EquipIconGrid.Grid.GetChildList();
		if (childList == null)
		{
			yield break;
		}
		int i = 0;
		for (int count = childList.Count; i < count; i++)
		{
			ItemIconComponent component = childList[i].GetComponent<ItemIconComponent>();
			if (!(component == null) && _itemListListener.SelectedItemIconTable.Contains(component))
			{
				itemListListener.OnClick(component);
			}
		}
		UpdateUnselectButtonsState();
	}

	private bool NeedToWaitPopup(EquipItemInfo equipInfo)
	{
		if (_toggle == EToggle.Chest)
		{
			return false;
		}
		if (equipInfo == null)
		{
			return false;
		}
		if (!equipInfo.IsBreakable)
		{
			return false;
		}
		if (!equipInfo.IsUniqueWeapon && !equipInfo.IsUniqueTreasure)
		{
			return false;
		}
		return true;
	}

	void EquipmentItemFilter.Listener.OnClickOK()
	{
		switch (_subMenuType)
		{
		case SubMenuType.EquipKeepSelect:
		case SubMenuType.EquipFilteredKeepSelect:
			CloseSubMenu(SubMenuType.EquipTypeSelect);
			OnClickKeep();
			break;
		case SubMenuType.EquipBreakSelect:
		case SubMenuType.EquipFilteredBreakSelect:
			CloseSubMenu(SubMenuType.EquipTypeSelect);
			OnClickBreak();
			break;
		case SubMenuType.EquipSellSelect:
		case SubMenuType.EquipFilteredSellSelect:
			CloseSubMenu(SubMenuType.EquipTypeSelect);
			OnClickSell();
			break;
		case SubMenuType.EquipBringSelect:
		case SubMenuType.EquipFilteredBringSelect:
			CloseSubMenu(SubMenuType.ChestTypeSelect);
			OnClickBring();
			break;
		}
		Refresh();
	}

	void CraftItemComponent.Listener.OnAddSlot(int index)
	{
		if (_listener != null)
		{
			_listener.OnAddSlot(index);
		}
	}

	void CraftItemComponent.Listener.OnCraftItem(int index)
	{
		if (_listener != null)
		{
			_listener.OnCraftItem(index);
		}
	}

	void CraftItemComponent.Listener.OnCancelCraft(int index)
	{
		if (_listener != null)
		{
			_listener.OnCancelCraft(index);
		}
	}

	void CraftItemComponent.Listener.OnFinishCraft(CraftItemComponent comp)
	{
		if (_listener != null)
		{
			_listener.OnFinishCraft(comp);
		}
	}

	void CraftItemComponent.Listener.OnGetItem(int index)
	{
		if (_listener != null)
		{
			_listener.OnGetItem(index);
		}
	}

	void IUserObserver<NGame2.NAccount.UserItemManager.ItemCountChangedInfo>.OnChanged(NGame2.NAccount.UserItemManager.ItemCountChangedInfo changedInfo)
	{
		_pendingInitNewItemCountTag = true;
	}

	public void OnClickCraft()
	{
		if (_listener != null)
		{
			_listener.OnCraftItem(1);
		}
	}
}

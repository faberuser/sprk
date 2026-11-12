using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NDropBonusEvent;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NHelper.NShop;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow.NShop;
using NGame2.NUtil;
using NGame2.NUtil.NStringProxy;
using NNguiExtension;
using NShared;
using NShared.BeginCampaign;
using NShared.BuyShopItem;
using NShared.NLocalization;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using NVespa.NTask;
using NVespa.NTask.NRuntime;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class ShopPanel : MonoBehaviour, ShopItemComponent.Listener, ShopItemComponent.IDataProvider, ShopItemGroupListComponent.Listener, IUserObserver<UserManager.UserStatInfo>, IUserObserver, IUserObserver<NGame2.NAccount.UserPlayerCurrencyManager.ChangedCurrencyInfo>, LobbyTopmostMenu.IUserStatObserver, LobbyTopmostMenu.ICustomStaminaObserver
{
	public enum ECompType
	{
		Normal,
		ChallengeRaid
	}

	public CurrencyType ShopCurrencyType;

	public UILabel Label_Currency;

	public UIButton Button_Buy;

	public UIWidget WidgetShopItemList;

	public UIScrollGrid ShopItemGrid;

	public UIWidget WidgetShopItemListWithShopItemGroup;

	public UIScrollGrid SmallShopitemListGrid;

	public ShopItemGroupListComponent ShopItemGroupList;

	public UILabel Label_SelectGroup;

	public UISimpleGrid BuffIconGrid;

	public Shop.RestockTimeUiData RestockTimeUi;

	[Header("[Filter]")]
	public UIButton Button_Filter;

	public UISpriteSelector Sprite_FilterArrow;

	public UIButton Button_FilterActive;

	public UISpriteSelector Sprite_FilterActiveArrow;

	public UILabel Label_EmptyItem;

	public ShopItemSortFilter ShopItemSortFilter;

	protected int _shopIndex;

	protected ShopData _shopData;

	protected int _selectedItemIndex;

	private int _selectedGroupIndex;

	private bool _isBootyItemEquip;

	private long _selectedItemId;

	protected Dictionary<int, ShopItemComponent> _shopItemDic = new Dictionary<int, ShopItemComponent>();

	private bool _restockPopup;

	private UIScrollGrid _curShopItemListGrid;

	private List<ShopItemGroupComponent> _ShopItemGroupComponentList = new List<ShopItemGroupComponent>();

	private List<GeneralIconComponent> _buffIcons = new List<GeneralIconComponent>();

	private List<GeneralIconComponent> _removebuffIcons = new List<GeneralIconComponent>();

	protected NPCData _NpcData;

	private bool _resetPositionAtOpen;

	private ECompType _compType;

	UserManager.UserStatType LobbyTopmostMenu.IUserStatObserver.StatType => ShopHelper.GetStatType(_shopData);

	StaminaType LobbyTopmostMenu.ICustomStaminaObserver.StaminaType => ShopHelper.GetStaminaType(_shopData);

	private bool SelectedItemBuyable()
	{
		if (!_shopItemDic.ContainsKey(_selectedItemIndex))
		{
			return false;
		}
		return ShopHelper.Buyable(_shopItemDic[_selectedItemIndex]);
	}

	private bool IsAllShopItemSold()
	{
		if (_shopItemDic.Count == 0)
		{
			return false;
		}
		foreach (ShopItemComponent value in _shopItemDic.Values)
		{
			if (!value.IsSoldOut())
			{
				return false;
			}
		}
		return true;
	}

	private ShopCostType GetSelectedItemCostType()
	{
		if (ShopHelper.IsNormalGuildShop(_shopData))
		{
			return ShopCostType.GuildPoint;
		}
		if (_shopItemDic.ContainsKey(_selectedItemIndex))
		{
			return _shopItemDic[_selectedItemIndex].GetCurrentCostType();
		}
		return _shopData.BuyCostType;
	}

	bool IUserObserver.IsGlobal()
	{
		return false;
	}

	public void OnChanged(UserManager.UserStatInfo userStatInfo)
	{
		bool flag = false;
		ShopCostType buyCostType = ShopHelper.GetBuyCostType(userStatInfo.UserStatType);
		if (__Extension_Method_ICollection__.COUNT(_shopData.BuyCostTypeUI) <= 1)
		{
			if (_shopData.BuyCostType == buyCostType)
			{
				flag = true;
			}
		}
		else
		{
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_shopData.BuyCostTypeUI); i++)
			{
				if (_shopData.BuyCostTypeUI[i] == buyCostType)
				{
					flag = true;
					break;
				}
			}
		}
		if (flag)
		{
			UpdateShopItems();
		}
	}

	public void OnChanged(NGame2.NAccount.UserPlayerCurrencyManager.ChangedCurrencyInfo changedInfo)
	{
		bool flag = false;
		switch (changedInfo.currencyType)
		{
		case CurrencyType.RankingPoint:
			flag = CheckUpdateCostType(ShopCostType.RankingPoint);
			break;
		case CurrencyType.EventDungeonPoint3:
			flag = CheckUpdateCostType(ShopCostType.EventDungeonPoint3);
			break;
		case CurrencyType.GuildArenaPoint:
			flag = CheckUpdateCostType(ShopCostType.GuildArenaPoint);
			break;
		case CurrencyType.OrdealArenaPoint:
			flag = CheckUpdateCostType(ShopCostType.OrdealArenaPoint);
			break;
		case CurrencyType.ChallengeRaidPoint:
			flag = CheckUpdateCostType(ShopCostType.ChallengeRaidPoint);
			break;
		}
		if (flag)
		{
			UpdateShopItems();
		}
		if (changedInfo.currencyType == ShopCurrencyType)
		{
			SetupCurrencyLabel();
			if (_selectedItemIndex != 1)
			{
				SetItemDescription();
			}
		}
	}

	private bool CheckUpdateCostType(ShopCostType curCostType)
	{
		if (__Extension_Method_ICollection__.COUNT(_shopData.BuyCostTypeUI) > 0)
		{
			return Array.Exists(_shopData.BuyCostTypeUI, (ShopCostType cost) => cost == curCostType);
		}
		return _shopData.BuyCostType == curCostType;
	}

	public void Init(ShopData shopData, NPCData npcData, ECompType compType = ECompType.Normal)
	{
		_shopData = shopData;
		if (_shopData != null)
		{
			SceneAutoBehaviour<ShopFacade>.instance.Reset(shopData.Index);
			_shopIndex = shopData.Index;
			_NpcData = npcData;
			_resetPositionAtOpen = GameRoot.Instance.ConstantDataContainer.GetValueBool("ResetPositionAtOpen");
			RestockTimeUi.SetVisible(_shopData.MaxStock > 0);
			_compType = compType;
			SetupBuffIconGroup();
			SetupShopItemGrid();
			SetupFilter();
			SetupSort();
			SetupCurrencyLabel();
			if (!SceneAutoBehaviour<ShopFacade>.instance.NeedRestock())
			{
				SetShopItemList();
			}
			else
			{
				RequestShopItemListAsync().Commit();
			}
			GlobalAutoBehaviour<AccountManager>.instance.UserManager.ObserverManager.AddObserver(this);
			GlobalAutoBehaviour<AccountManager>.instance.PlayerCurrencyManager.ObserverManager.AddObserver(this);
		}
	}

	public void UpdateShop()
	{
		UpdateRestockPopup();
		UpdateRestockTime();
		UpdateBuffIcons();
	}

	private void UpdateRestockPopup()
	{
		if (SceneAutoBehaviour<ShopFacade>.instance.IsValidRestockTime && !_restockPopup && SceneAutoBehaviour<ShopFacade>.instance.NeedRestock())
		{
			string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
			string message = LocalizationManager.GetString(NShared.NLocalization.Shop.SHOP_RESTOCK_MSG);
			MessagePopup.Open(title, message);
			_restockPopup = true;
			CanNotBuy();
		}
	}

	private void CanNotBuy()
	{
		foreach (ShopItemComponent value in _shopItemDic.Values)
		{
			value.SetNotForSale();
		}
		_selectedItemIndex = -1;
		SetItemDescription();
	}

	private void UpdateRestockTime()
	{
		if (_shopData == null)
		{
			return;
		}
		if (__Extension_Method_ICollection__.IsNullOrEmpty(_shopData.Timeline) || _shopData.MaxStock == 0)
		{
			RestockTimeUi.Clear();
			return;
		}
		TimeSpan timeSpan = TimeSpan.Zero;
		DateTime currentServerUTCTime = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime();
		if (currentServerUTCTime < SceneAutoBehaviour<ShopFacade>.instance.NextRestockTime)
		{
			timeSpan = SceneAutoBehaviour<ShopFacade>.instance.NextRestockTime - currentServerUTCTime;
		}
		RestockTimeUi.SetTimeSpan(timeSpan);
	}

	private void UpdateBuffIcons()
	{
		if (__Extension_Method_ICollection__.COUNT(_buffIcons) == 0)
		{
			return;
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_buffIcons); i++)
		{
			GeneralIconComponent generalIconComponent = _buffIcons[i];
			if (!generalIconComponent.IsEffective())
			{
				_removebuffIcons.Add(generalIconComponent);
			}
		}
		for (int j = 0; j < __Extension_Method_ICollection__.COUNT(_removebuffIcons); j++)
		{
			GeneralIconComponent comp = _removebuffIcons[j];
			_buffIcons.Remove(comp);
			FactoryBase.SafeReleaseComponent(ref comp);
		}
	}

	private void SetupShopItemGrid()
	{
		if (_shopData.MaxStock > 0)
		{
			UIHelper.EnableGameObject(WidgetShopItemList, enable: true);
			UIHelper.EnableGameObject(WidgetShopItemListWithShopItemGroup, enable: false);
			_curShopItemListGrid = ShopItemGrid;
			return;
		}
		int index = _shopData.Index;
		List<ShopItemGroupData> groupDataList = GameRoot.Instance.ShopItemGroupDataContainer.GetGroupDataList(index);
		if (groupDataList == null || __Extension_Method_ICollection__.COUNT(groupDataList) == 0)
		{
			UIHelper.EnableGameObject(WidgetShopItemList, enable: true);
			UIHelper.EnableGameObject(WidgetShopItemListWithShopItemGroup, enable: false);
			_curShopItemListGrid = ShopItemGrid;
		}
		else
		{
			UIHelper.EnableGameObject(WidgetShopItemListWithShopItemGroup, enable: true);
			UIHelper.EnableGameObject(WidgetShopItemList, enable: false);
			_curShopItemListGrid = SmallShopitemListGrid;
			ShopItemGroupList.Init(this, _shopData.Index);
		}
	}

	private void SetupBuffIconGroup()
	{
		if (BuffIconGrid == null)
		{
			return;
		}
		ClearBuffIcon();
		if (_shopData == null)
		{
			return;
		}
		if (_shopData.HeroIndex > 0)
		{
			CachedHeroInfo cachedHeroInfo = GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(_shopData.HeroIndex);
			if (cachedHeroInfo != null)
			{
				HeroBonusData data = GameRoot.Instance.HeroBonusDataContainer.GetData(cachedHeroInfo.HeroIndex, cachedHeroInfo.Star, cachedHeroInfo.Transcended);
				if (data != null && __Extension_Method_ICollection__.COUNT(data.GetHeroBonusTypeDatas(EventBonusType.DiscountShop)) > 0)
				{
					GeneralIconComponent generalIconComponent = BuffIconGrid.AddGridItem(GeneralIconComponent.PrefabAuto);
					generalIconComponent.Open(UIHelper.GetAttrTypeIconPath(data.Icon), data, GeneralIconComponent.DecoType.EntryMenuBuff);
					_buffIcons.Add(generalIconComponent);
				}
			}
		}
		UIHelper.CreatreNpcBuffIconComponent(_NpcData, _buffIcons, BuffIconGrid);
		UIHelper.RepositionGrid(BuffIconGrid);
	}

	private void SetupFilter()
	{
		bool flag = _shopData.ShopFilterGroupType != EShopItemFilterGroupType.None;
		UIHelper.EnableGameObject(Button_Filter, flag);
		UIHelper.EnableGameObject(Button_FilterActive, enable: false);
		if (flag && ShopItemSortFilter != null)
		{
			ShopItemSortFilter.SetFilterGroupComponent(_shopData.ShopFilterGroupType);
		}
	}

	private void SetFilterButton(bool isOn)
	{
		if (_shopData == null || _shopData.ShopFilterGroupType == EShopItemFilterGroupType.None)
		{
			UIHelper.EnableGameObject(Button_Filter, enable: false);
			UIHelper.EnableGameObject(Button_FilterActive, enable: false);
			return;
		}
		bool flag = !ShopItemSortFilter.IsNoneSelected();
		UIHelper.EnableGameObject(Button_Filter, !flag);
		UIHelper.EnableGameObject(Button_FilterActive, flag);
		(flag ? Sprite_FilterActiveArrow : Sprite_FilterArrow).Select(isOn.ToString());
	}

	private void SetupSort()
	{
		if (!(_curShopItemListGrid == null))
		{
			if (_shopData.ShopFilterGroupType == EShopItemFilterGroupType.None)
			{
				_curShopItemListGrid.Grid.sorting = UIGrid.Sorting.None;
				_curShopItemListGrid.Grid.onCustomSort = null;
			}
			else if (_shopData.ShopFilterGroupType == EShopItemFilterGroupType.Guild)
			{
				_curShopItemListGrid.Grid.sorting = UIGrid.Sorting.Custom;
				_curShopItemListGrid.Grid.onCustomSort = SortShopItemOrder;
			}
		}
	}

	private void SetupCurrencyLabel()
	{
		if (!(Label_Currency == null) && ShopCurrencyType != CurrencyType.None)
		{
			CurrencyTypeData data = GameRoot.Instance.CurrencyTypeDataContainer.GetData(ShopCurrencyType);
			if (data != null && !String_Extension_Method.IsNullOrEmpty(data.UserStatType))
			{
				long userStat = GlobalAutoBehaviour<AccountManager>.instance.UserManager.GetUserStat(data.UserStatType);
				UIHelper.EnableGameObject(Label_Currency, enable: true);
				UIHelper.SetLabelText(Label_Currency, userStat);
			}
			else
			{
				UIHelper.EnableGameObject(Label_Currency, enable: false);
			}
		}
	}

	private void SetShopItemList()
	{
		Clear();
		if (_shopData.MaxStock > 0)
		{
			SetRestockShopItemList();
		}
		else
		{
			SetCommonShopItemList();
		}
	}

	private void SetRestockShopItemList()
	{
		int num = 1;
		while (true)
		{
			ShopListItemInfo shopListItemInfo = GlobalAutoBehaviour<AccountManager>.instance.ShopItemManager.Get(_shopIndex, num);
			if (shopListItemInfo == null)
			{
				break;
			}
			CreateShopListItem(shopListItemInfo);
			num++;
		}
		UIHelper.RepositionGrid(_curShopItemListGrid);
		SetItemDescription();
		if (_resetPositionAtOpen)
		{
			ResetGridPosition(_curShopItemListGrid);
		}
	}

	private void SetCommonShopItemList()
	{
		List<ShopItemData> list = null;
		List<ShopItemData> list2 = null;
		if (__Extension_Method_ICollection__.COUNT(GameRoot.Instance.ShopItemGroupDataContainer.GetGroupDataList(_shopData.Index)) == 0)
		{
			if (_shopData.UseGroupIndexForLevel)
			{
				int currentShopLevel = ShopHelper.GetCurrentShopLevel(_shopData);
				list = GameRoot.Instance.ShopItemDataContainer.GetShopItemListByShopItemGroup(_shopData.Index, currentShopLevel);
			}
			else
			{
				list = GameRoot.Instance.ShopItemDataContainer.GetShopItemList(_shopData.Index);
			}
		}
		else
		{
			int? num = (SceneAutoBehaviour<EntryMenuManagement>.instance.ForceShopGroupIndex.HasValue ? SceneAutoBehaviour<EntryMenuManagement>.instance.ForceShopGroupIndex : new int?(ShopItemGroupList.SelectedGroupIndex));
			list = GameRoot.Instance.ShopItemDataContainer.GetShopItemListByShopItemGroup(_shopData.Index, num.Value);
			UIHelper.EnableGameObject(Label_EmptyItem, enable: false);
			UIHelper.EnableGameObject(Label_SelectGroup, __Extension_Method_ICollection__.COUNT(list) == 0);
		}
		bool flag = _shopData.ShopFilterGroupType != EShopItemFilterGroupType.None;
		if (!flag)
		{
			list2 = list;
		}
		else
		{
			list2 = new List<ShopItemData>();
			foreach (ShopItemData item in list)
			{
				ShopItemFilterData dataByCode = GameRoot.Instance.ShopItemFilterDataContainer.GetDataByCode(item.ItemCode);
				if (dataByCode != null && ShopItemFilter(dataByCode))
				{
					list2.Add(item);
				}
			}
		}
		if (__Extension_Method_ICollection__.COUNT(list2) > 0)
		{
			list2.Sort(SortWithShowIndex);
		}
		UIHelper.EnableGameObject(Label_EmptyItem, flag && __Extension_Method_ICollection__.COUNT(list2) == 0);
		CreateShopList(list2);
	}

	private void ResetGridPosition(UIScrollGrid scrollGrid)
	{
		if (!(scrollGrid == null))
		{
			UIHelper.ResetGridPosition(scrollGrid.Grid);
		}
	}

	protected void SetItemDescription()
	{
		UIHelper.EnableButton(Button_Buy, enable: false);
		if (IsAllShopItemSold())
		{
			return;
		}
		string code = string.Empty;
		ShopItemData shopItemData = null;
		if (_shopData.MaxStock > 0)
		{
			ShopListItemInfo shopListItemInfo = GlobalAutoBehaviour<AccountManager>.instance.ShopItemManager.Get(_shopIndex, _selectedItemIndex);
			if (shopListItemInfo != null)
			{
				code = shopListItemInfo.ItemCode;
			}
		}
		else
		{
			shopItemData = GameRoot.Instance.ShopItemDataContainer.GetData(_shopIndex, _selectedItemIndex);
			if (shopItemData != null)
			{
				code = shopItemData.ItemCode;
			}
		}
		if (GameRoot.Instance.ItemDataContainer.GetDataByCode(code) == null)
		{
			UIHelper.EnableButton(Button_Buy, enable: false);
			return;
		}
		bool enable = ShopHelper.Buyable(_shopItemDic[_selectedItemIndex]);
		UIHelper.EnableButton(Button_Buy, enable);
	}

	private void CreateShopListItem(ShopItemData shopItemData)
	{
		if (String_Extension_Method.IsNullOrEmpty(shopItemData.Condition) || GlobalAutoBehaviour<DropBonusEventManager>.instance.GetEffectiveEvent(EventBonusType.ShopList, shopItemData.Condition) != null)
		{
			ShopItemInfo shopItemPurchaseInfo = SceneAutoBehaviour<ShopManagement>.instance.GetShopItemPurchaseInfo(_shopData.Index, shopItemData.Index);
			ShopItemComponent shopItemComponent = ((_compType != ECompType.ChallengeRaid) ? UIHelper.AddGridItem(_curShopItemListGrid, ShopItemComponent.PrefabAuto) : UIHelper.AddGridItem(_curShopItemListGrid, ShopItemComponent.PrefabAuto2));
			shopItemComponent.Open(shopItemData, shopItemPurchaseInfo, this, this);
			_shopItemDic.Add(shopItemData.Index, shopItemComponent);
		}
	}

	private void CreateShopListItem(ShopListItemInfo info)
	{
		ShopItemComponent shopItemComponent = ((_compType != ECompType.ChallengeRaid) ? UIHelper.AddGridItem(_curShopItemListGrid, ShopItemComponent.PrefabAuto) : UIHelper.AddGridItem(_curShopItemListGrid, ShopItemComponent.PrefabAuto2));
		shopItemComponent.Open(info, this, this);
		_shopItemDic.Add(info.ListNo, shopItemComponent);
	}

	private void CreateShopList(List<ShopItemData> shopItemDataList)
	{
		if (shopItemDataList == null)
		{
			return;
		}
		Action CreateShopItemListFunc = delegate
		{
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(shopItemDataList); i++)
			{
				CreateShopListItem(shopItemDataList[i]);
			}
			UIHelper.RepositionGrid(_curShopItemListGrid);
			SetItemDescription();
			if (_resetPositionAtOpen)
			{
				ResetGridPosition(_curShopItemListGrid);
			}
		};
		bool flag = false;
		if (!SceneAutoBehaviour<ShopManagement>.instance.HasShopItemInfos(_shopData.Index))
		{
			flag = shopItemDataList.Exists((ShopItemData data) => data.PurchasableCount > 0);
			if (flag)
			{
				SceneAutoBehaviour<ShopManagement>.instance.RequestPurchasableCount(_shopData, delegate
				{
					CreateShopItemListFunc();
				});
			}
		}
		if (!flag)
		{
			CreateShopItemListFunc();
		}
	}

	public static EquipItemInfo CreateFakeEquipInfo(ItemData itemData, int star)
	{
		if (star == 0)
		{
			return null;
		}
		if (itemData.Type != ItemType.Equip)
		{
			return null;
		}
		return new EquipItemInfo
		{
			ItemIndex = itemData.Index,
			Star = star
		};
	}

	private ITask RequestShopItemListAsync()
	{
		return SceneAutoBehaviour<ShopFacade>.instance.FreeRestock(delegate
		{
			SetShopItemList();
			return (ITask)null;
		});
	}

	private bool ShopItemFilter(IShopItemFilterInfo info)
	{
		return ShopItemSortFilter.Filter(info);
	}

	private bool ShopItemFilterByType(EShopItemType type)
	{
		return ShopItemSortFilter.Filter(type);
	}

	private void ResetFilter()
	{
		if (ShopItemSortFilter != null)
		{
			ShopItemSortFilter.Reset();
		}
	}

	private int SortShopItemOrder(Transform l, Transform r)
	{
		ShopItemComponent component = l.GetComponent<ShopItemComponent>();
		ShopItemComponent component2 = r.GetComponent<ShopItemComponent>();
		if (component == null || component2 == null)
		{
			return 0;
		}
		int num = 0;
		num = SortSoldOut(component, component2);
		if (num != 0)
		{
			return num;
		}
		if (ShopItemSortFilter.Sort(EShopSortType.GradeLowToHigh))
		{
			ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(component.ShopItemData.ItemCode);
			ItemData dataByCode2 = GameRoot.Instance.ItemDataContainer.GetDataByCode(component2.ShopItemData.ItemCode);
			num = CompareValue(isAscending: true, dataByCode.Grade, dataByCode2.Grade);
			if (num != 0)
			{
				return num;
			}
		}
		if (ShopItemSortFilter.Sort(EShopSortType.GradeHighToLow))
		{
			ItemData dataByCode3 = GameRoot.Instance.ItemDataContainer.GetDataByCode(component.ShopItemData.ItemCode);
			ItemData dataByCode4 = GameRoot.Instance.ItemDataContainer.GetDataByCode(component2.ShopItemData.ItemCode);
			num = CompareValue(isAscending: false, dataByCode3.Grade, dataByCode4.Grade);
			if (num != 0)
			{
				return num;
			}
		}
		checked
		{
			if (ShopItemSortFilter.Sort(EShopSortType.PriceLowToHigh))
			{
				long price = component.GetPrice();
				long price2 = component2.GetPrice();
				long num2 = price;
				num = CompareValue(isAscending: true, (int)num2, (int)price2);
				if (num != 0)
				{
					return num;
				}
			}
			if (ShopItemSortFilter.Sort(EShopSortType.PriceHighToLow))
			{
				long price3 = component.GetPrice();
				long price4 = component2.GetPrice();
				long num3 = price3;
				num = CompareValue(isAscending: false, (int)num3, (int)price4);
				if (num != 0)
				{
					return num;
				}
			}
			return SortWithShowIndex(component.ShopItemData, component2.ShopItemData);
		}
	}

	private int SortSoldOut(ShopItemComponent lComp, ShopItemComponent rComp)
	{
		if (lComp.IsSoldOut())
		{
			return 1;
		}
		if (rComp.IsSoldOut())
		{
			return -1;
		}
		return 0;
	}

	private int SortWithShowIndex(ShopItemData itemL, ShopItemData itemR)
	{
		if (itemL.ShowIndex > itemR.ShowIndex)
		{
			return 1;
		}
		if (itemL.ShowIndex < itemR.ShowIndex)
		{
			return -1;
		}
		if (itemL.Index > itemR.Index)
		{
			return 1;
		}
		if (itemL.Index < itemR.Index)
		{
			return -1;
		}
		return 0;
	}

	private int CompareValue<T>(bool isAscending, T a, T b) where T : IComparable
	{
		if (isAscending)
		{
			return a.CompareTo(b);
		}
		return b.CompareTo(a);
	}

	private void UpdateShopItems()
	{
		foreach (ShopItemComponent value in _shopItemDic.Values)
		{
			value.SetPriceLabelColor();
		}
	}

	protected void OnClickShopItem(ShopItemComponent shopItem)
	{
		if (shopItem.ShopListItemInfo != null)
		{
			if (_selectedItemIndex == shopItem.ShopListItemInfo.ListNo)
			{
				return;
			}
			if (_shopItemDic.ContainsKey(_selectedItemIndex))
			{
				_shopItemDic[_selectedItemIndex].SetSelected(Select: false);
			}
			shopItem.SetSelected(Select: true);
			_selectedItemIndex = shopItem.ShopListItemInfo.ListNo;
		}
		if (shopItem.ShopItemData != null)
		{
			if (_selectedItemIndex == shopItem.ShopItemData.Index)
			{
				return;
			}
			if (_shopItemDic.ContainsKey(_selectedItemIndex))
			{
				_shopItemDic[_selectedItemIndex].SetSelected(Select: false);
			}
			shopItem.SetSelected(Select: true);
			_selectedItemIndex = shopItem.ShopItemData.Index;
		}
		SetItemDescription();
	}

	public void OnClickRestock()
	{
		string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
		string restock = LocalizationManager.GetString(NShared.NLocalization.Shop.RESTOCK);
		string text = LocalizationManager.GetString(NShared.NLocalization.Common.COST_TYPE_RUBY);
		string message = LocalizationManager.GetString(NShared.NLocalization.Shop.SHOP_RESTOCK_CONFIRM, text);
		int Value = _shopData.RestockGem;
		Action onYes = delegate
		{
			SceneAutoBehaviour<ShopFacade>.instance.PaidRestock(delegate
			{
				SetShopItemList();
				_restockPopup = false;
				return (ITask)null;
			}).Commit();
		};
		int num = Value;
		Action obj = delegate
		{
			BuyConfirmPopup.Open(title, message, restock, ShopCostType.Gem, Value, onYes);
		};
		Action skipAction = onYes;
		Action openAction = obj;
		int num2 = num;
		UIHelper.BuyConfirmPopupOpen(CurrencyType.Gem, num2, openAction, skipAction);
	}

	public void OnClickBuy()
	{
		if (!CheckSelectedIndex())
		{
			return;
		}
		if (!SelectedItemBuyable())
		{
			PopUpInsufficentMessage();
			return;
		}
		ShopItemComponent shopItemComponent = (_shopItemDic.ContainsKey(_selectedItemIndex) ? _shopItemDic[_selectedItemIndex] : null);
		bool flag = false;
		if (shopItemComponent != null)
		{
			_ = shopItemComponent.ShopItemData;
			_ = shopItemComponent.ShopListItemInfo;
			if (CanOpenMultipleBuyPopup(shopItemComponent))
			{
				MultipleBuyPopup.Open(shopItemComponent.GetPrice(), shopItemComponent, _shopData, delegate(int purchaseCount)
				{
					ExecBuySelectedItem(purchaseCount);
				});
				flag = true;
			}
			else if (_shopData.BeforeBuyConfirm)
			{
				string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
				string message = LocalizationManager.GetString(NShared.NLocalization.Shop.BUY_CONFIRM_MSG);
				MessagePopup.Open(title, message, delegate
				{
					ExecBuySelectedItem();
				}, null);
				flag = true;
			}
		}
		if (!flag)
		{
			ExecBuySelectedItem();
		}
	}

	private bool CanOpenMultipleBuyPopup(ShopItemComponent selectedShopItem)
	{
		if (!_shopData.MultiplePurchaseShop)
		{
			return false;
		}
		ShopItemData shopItemData = selectedShopItem.ShopItemData;
		ShopListItemInfo shopListItemInfo = selectedShopItem.ShopListItemInfo;
		if (shopItemData != null && selectedShopItem.GetRemainPurchasableCount() != 1)
		{
			return true;
		}
		if (shopListItemInfo != null && shopListItemInfo.PurchasableCount - shopListItemInfo.Purchased != 1)
		{
			return true;
		}
		return false;
	}

	private void ExecBuySelectedItem(int purchaseCount = 1)
	{
		ShopItemComponent selectedComponent = (_shopItemDic.ContainsKey(_selectedItemIndex) ? _shopItemDic[_selectedItemIndex] : null);
		ExecBuyItem(selectedComponent, purchaseCount);
	}

	private bool CheckSelectedIndex()
	{
		if (_shopData.MaxStock > 0)
		{
			if (_selectedItemIndex < 1 || _selectedItemIndex > _shopData.MaxStock)
			{
				return false;
			}
		}
		else if (_selectedItemIndex <= 0)
		{
			return false;
		}
		return true;
	}

	private void ExecBuyItem(ShopItemComponent SelectedComponent, int purchaseCount = 1)
	{
		NShared.BuyShopItem.Request request = new NShared.BuyShopItem.Request();
		request.ShopIndex = _shopIndex;
		if (_shopData.MaxStock > 0)
		{
			request.ListNo = _selectedItemIndex;
		}
		else
		{
			request.ShopItemIndex = _selectedItemIndex;
		}
		request.ShopItemPurchaseCount = purchaseCount;
		new WebServiceTask<NShared.BuyShopItem.Request, NShared.BuyShopItem.Response>(request, delegate(NShared.BuyShopItem.Request request2, NShared.BuyShopItem.Response response)
		{
			if (response.Result == NShared.BuyShopItem.ResultType.EquipItemFull)
			{
				return new DelegateTask(delegate
				{
					string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
					string message = LocalizationManager.GetString(NShared.BeginCampaign.ResultType.EquipItemFull);
					MessagePopup.OpenEnabled(title, message, delegate
					{
						SceneAutoBehaviour<InventoryManagement>.instance.OpenInventory();
					}, null, "MOVE_BAG", bYesEnabled: true);
				});
			}
			if (response.Result != NShared.BuyShopItem.ResultType.Success)
			{
				return new DelegateTask(delegate
				{
					MessagePopup.OpenError(response.Result);
				});
			}
			GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.CurrencyResults);
			GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.RoyalPointResult);
			GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.GuildPointResult);
			GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.FriendshipPointResult);
			GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.ItemResult);
			GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.Apply(response.EquipItems);
			GlobalAutoBehaviour<AccountManager>.instance.PlayerCurrencyManager.Apply(response.CurrencyResults);
			SceneAutoBehaviour<ShopManagement>.instance.UpdateShopItemInfo(_shopIndex, response.ShopItem);
			string code = "";
			int num = 0;
			ShopItemComponent shopItemComponent = _shopItemDic[_selectedItemIndex];
			if (_shopData.MaxStock > 0)
			{
				ShopListItemInfo shopListItemInfo = GlobalAutoBehaviour<AccountManager>.instance.ShopItemManager.Get(_shopIndex, _selectedItemIndex);
				if (shopListItemInfo != null)
				{
					shopListItemInfo.Purchased++;
					code = shopListItemInfo.ItemCode;
					num = shopListItemInfo.Count;
				}
				if (shopItemComponent != null)
				{
					shopItemComponent.SetSold();
				}
			}
			else if (shopItemComponent != null)
			{
				if (response.ShopItem != null)
				{
					shopItemComponent.UpdateShopItemInfo(response.ShopItem);
				}
				ShopItemData shopItemData = shopItemComponent.ShopItemData;
				if (shopItemData != null)
				{
					code = shopItemData.ItemCode;
					num = shopItemData.Count;
				}
			}
			num *= purchaseCount;
			if (shopItemComponent != null && shopItemComponent.IsSoldOut())
			{
				_selectedItemIndex = -1;
				SetItemDescription();
				ResetGridPosition(_curShopItemListGrid);
			}
			ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(code);
			if (dataByCode != null)
			{
				string itemName = ItemNameBuilder.GetItemName(dataByCode);
				CenterMessage.Open(LocalizationManager.GetString(NShared.NLocalization.Shop.SHOP_BOUGHT_MSG, itemName, num));
			}
			SetItemDescription();
			return (ITask)null;
		}).Commit();
	}

	private void PopUpInsufficentMessage()
	{
		string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
		Enum key = NShared.NLocalization.Common.UNKNOWN;
		Action onYes = null;
		switch (GetSelectedItemCostType())
		{
		case ShopCostType.Gold:
			key = NShared.NLocalization.System.MOVE_GOLD_SHOP_CONFIRM;
			onYes = delegate
			{
				SceneAutoBehaviour<PayShopManagement>.instance.OpenNewPayShop(NewPayShopCategoryType.Gold.ToString());
			};
			break;
		case ShopCostType.Gem:
			key = NShared.NLocalization.System.MOVE_GEM_SHOP_CONFIRM;
			onYes = delegate
			{
				SceneAutoBehaviour<PayShopManagement>.instance.OpenNewPayShop(NewPayShopCategoryType.Ruby.ToString());
			};
			break;
		case ShopCostType.GuildPoint:
			key = NShared.NLocalization.System.MOVE_GUILD_MENU_CONFIRM;
			onYes = delegate
			{
				CloseShop();
			};
			break;
		case ShopCostType.PvpCoin:
			key = NShared.NLocalization.System.MOVE_MATCH_MENU_CONFIRM;
			onYes = delegate
			{
				CloseShop();
				SceneAutoBehaviour<MatchManagement>.instance.OpenMatchSelect();
			};
			break;
		case ShopCostType.FriendshipPoint:
			MessagePopup.OpenError(NShared.BuyShopItem.ResultType.NotEnoughFriendshipPoint);
			return;
		case ShopCostType.RaidPoint:
			MessagePopup.OpenError(NShared.BuyShopItem.ResultType.NotEnoughRaidPoint);
			return;
		case ShopCostType.WorldBossPoint:
			MessagePopup.OpenError(NShared.BuyShopItem.ResultType.NotEnoughWorldBossPoint);
			return;
		case ShopCostType.EventDungeonPoint:
			MessagePopup.OpenError(NShared.BuyShopItem.ResultType.NotEnoughEventDungeonPoint);
			return;
		case ShopCostType.EventDungeonPoint2:
			MessagePopup.OpenError(NShared.BuyShopItem.ResultType.NotEnoughEventDungeonPoint2);
			return;
		case ShopCostType.GloryPoint:
			MessagePopup.OpenError(NShared.BuyShopItem.ResultType.NotEnoughGloryPoint);
			return;
		case ShopCostType.EventGiftPoint:
			MessagePopup.OpenError(NShared.BuyShopItem.ResultType.NotEnoughEventGift);
			return;
		case ShopCostType.RankingPoint:
			MessagePopup.OpenError(NShared.BuyShopItem.ResultType.NotEnoughRankingPoint);
			return;
		case ShopCostType.EventDungeonPoint3:
			MessagePopup.OpenError(NShared.BuyShopItem.ResultType.NotEnoughEventDungeonPoint3);
			return;
		case ShopCostType.GuildArenaPoint:
			MessagePopup.OpenError(NShared.BuyShopItem.ResultType.NotEnoughGuildArenaPoint);
			return;
		case ShopCostType.ShopEventPoint:
			MessagePopup.OpenError(NShared.BuyShopItem.ResultType.NotEnoughShopEventPoint);
			return;
		case ShopCostType.OrdealArenaPoint:
			MessagePopup.OpenError(NShared.BuyShopItem.ResultType.NotEnoughOrdealArenaPoint);
			return;
		}
		MessagePopup.Open(title, LocalizationManager.GetString(key), onYes, null);
	}

	protected void OnClickShopGroup(ShopItemGroupComponent clicked)
	{
		if (clicked.ShopItemGroupData.GroupIndex != _selectedGroupIndex)
		{
			_selectedGroupIndex = clicked.ShopItemGroupData.GroupIndex;
			Clear();
			List<ShopItemData> shopItemListByShopItemGroup = GameRoot.Instance.ShopItemDataContainer.GetShopItemListByShopItemGroup(_shopData.Index, clicked.ShopItemGroupData.GroupIndex);
			if (__Extension_Method_ICollection__.COUNT(shopItemListByShopItemGroup) > 0)
			{
				shopItemListByShopItemGroup.Sort(SortWithShowIndex);
			}
			CreateShopList(shopItemListByShopItemGroup);
		}
	}

	public void OnClickFilter()
	{
		if (ShopItemSortFilter != null)
		{
			if (ShopItemSortFilter.IsOpenFilterPanel())
			{
				OnClickResetFilter();
			}
			else
			{
				OnClickOpenFilter();
			}
		}
	}

	public void OnClickOpenFilter()
	{
		SetFilterButton(isOn: false);
		if (ShopItemSortFilter != null)
		{
			ShopItemSortFilter.OnOffRoot(isOn: true);
		}
	}

	public void OnClickCloseFilter()
	{
		SetFilterButton(isOn: true);
		if (ShopItemSortFilter != null)
		{
			ShopItemSortFilter.OnOffRoot(isOn: false);
		}
	}

	public void OnClickComfirmFilter()
	{
		SetShopItemList();
		OnClickCloseFilter();
	}

	public void OnClickResetFilter()
	{
		ResetFilter();
		SetShopItemList();
		OnClickCloseFilter();
	}

	void ShopItemComponent.Listener.OnClick(ShopItemComponent shopItem)
	{
		OnClickShopItem(shopItem);
	}

	bool ShopItemComponent.IDataProvider.IsGreaterThanPrice(ShopItemComponent comp)
	{
		return ShopHelper.Buyable(comp);
	}

	int ShopItemComponent.IDataProvider.GetCurrentMyDonatePoint()
	{
		if (_NpcData == null)
		{
			return 0;
		}
		return ShopHelper.GetCurrentMyDonatePoint(_NpcData.HeroIndex);
	}

	int ShopItemComponent.IDataProvider.GetCurrentMaxDonatePoint()
	{
		if (_NpcData == null)
		{
			return 0;
		}
		return ShopHelper.GetCurrentSelectedMaxDonatePoint(_NpcData.HeroIndex);
	}

	void ShopItemGroupListComponent.Listener.OnClickGroupComponent(ShopItemGroupComponent clicked)
	{
		OnClickShopGroup(clicked);
	}

	public void CloseShop()
	{
		SceneAutoBehaviour<EntryMenuManagement>.instance.ForceShopGroupIndex = null;
	}

	public void ResetShop()
	{
		_restockPopup = false;
		_selectedGroupIndex = -1;
		FactoryBase.ReleaseAndClearList(_ShopItemGroupComponentList);
		UIHelper.EnableGameObject(Label_SelectGroup, enable: false);
		UIHelper.EnableGameObject(Label_EmptyItem, enable: false);
		Clear();
		ClearFilter();
		GlobalAutoBehaviour<AccountManager>.instance.UserManager.ObserverManager.RemoveObserver(this);
		GlobalAutoBehaviour<AccountManager>.instance.PlayerCurrencyManager.ObserverManager.RemoveObserver(this);
	}

	private void Clear()
	{
		RestockTimeUi.Clear();
		foreach (ShopItemComponent value in _shopItemDic.Values)
		{
			value.Close();
		}
		FactoryBase.ReleaseAndClearList(_shopItemDic);
		_selectedItemId = -1L;
		_selectedItemIndex = -1;
	}

	private void ClearBuffIcon()
	{
		FactoryBase.ReleaseAndClearList(_buffIcons);
	}

	private void ClearFilter()
	{
		if (ShopItemSortFilter != null)
		{
			ShopItemSortFilter.Close();
			if (Sprite_FilterArrow != null)
			{
				Sprite_FilterArrow.transform.localEulerAngles = Vector3.zero;
			}
		}
	}
}

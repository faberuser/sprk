using System;
using System.Collections;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NDropBonusEvent;
using NGame2.NGuild;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NComponent2.NStat;
using NGame2.NUI.NHelper.NShop;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow.NShop;
using NGame2.NUtil;
using NGame2.NUtil.NStringProxy;
using NGameAsset;
using NNguiExtension;
using NShared;
using NShared.BeginCampaign;
using NShared.BuyGuildRaidBootyItem;
using NShared.BuyShopItem;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using NVespa.NTask;
using NVespa.NTask.NRuntime;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class Shop : BaseWindow, ShopItemComponent.Listener, ShopItemComponent.IDataProvider, GuildRaidBootyItemComponent.Listener, GuildRaidBootyItemComponent.IDataProvider, ShopItemGroupListComponent.Listener, IUserObserver<NGame2.NGuild.GuildRaidBootyEquipItemManager.ChangeItemInfo>, IUserObserver, IUserObserver<NGame2.NGuild.GuildRaidBootyItemManager.ChangeItemInfo>, IUserObserver<UserManager.UserStatInfo>, IUserObserver<NGame2.NAccount.UserPlayerCurrencyManager.ChangedCurrencyInfo>, LobbyTopmostMenu.IUserStatObserver, LobbyTopmostMenu.IMiniGuideButtonObserver, LobbyTopmostMenu.ICustomStaminaObserver
{
	public enum ENPCSpeechEvent
	{
		Buy
	}

	[Serializable]
	public class ItemDescription
	{
		public UISimpleGrid ItemGrid;

		public UILabel Label_ItemName;

		public UILabel Label_ItemType;

		public UILabel Label_RemainTime;

		public UILabel Label_PurchaseLimit;

		public UILabel Label_PurchaseCount;

		public UIWidget WidgetEquipSubInfo;

		public UILabel Label_EquipLevel;

		public UIWidget UniqueWidget;

		public UITexture UniqueHeroTexture;

		public UILabel Label_CreatureLimit;

		public UISimpleGrid ClassTagGrid;

		public UILabel Label_Description;

		public UIScrollView EquipItemDescScrollView;

		public UIWidget StatRootWidget;

		public UILabel Label_SelectItem;

		public UILabel Label_AllSoldOut;

		public void ActivateCommon(bool Active)
		{
			UIHelper.EnableGameObject(ItemGrid, Active);
			UIHelper.EnableGameObject(Label_ItemName, Active);
			UIHelper.EnableGameObject(Label_ItemType, Active);
			UIHelper.EnableGameObject(WidgetEquipSubInfo, Active);
			UIHelper.EnableGameObject(StatRootWidget, Active);
			UIHelper.EnableGameObject(Label_Description, Active);
			UIHelper.EnableGameObject(Label_SelectItem, !Active);
			UIHelper.EnableGameObject(Label_AllSoldOut, !Active);
			UIHelper.EnableGameObject(Label_RemainTime, Active);
		}

		public void SetEmpty()
		{
			ActivateCommon(Active: false);
			UIHelper.EnableGameObject(Label_SelectItem, enable: true);
			UIHelper.EnableGameObject(Label_AllSoldOut, enable: false);
		}

		public void SetAllSoldOut()
		{
			ActivateCommon(Active: false);
			UIHelper.EnableGameObject(Label_SelectItem, enable: false);
			UIHelper.EnableGameObject(Label_AllSoldOut, enable: true);
			UIHelper.EnableGameObject(Label_PurchaseCount, enable: false);
			UIHelper.EnableGameObject(Label_PurchaseLimit, enable: false);
		}
	}

	[Serializable]
	public class RestockTimeUiData
	{
		public UIWidget WidgetThis;

		public UILabel RestockTimeLabel;

		private TimeSpan _lastTimeSpan;

		public void SetVisible(bool Visible)
		{
			UIHelper.EnableGameObject(WidgetThis, Visible);
		}

		private bool Visible()
		{
			return UIHelper.IsEnabledGameObjet(WidgetThis);
		}

		public void Clear()
		{
			if (Visible())
			{
				_lastTimeSpan = TimeSpan.MinValue;
				UIHelper.SetLabelText(RestockTimeLabel, string.Empty);
			}
		}

		public void SetTimeSpan(TimeSpan time)
		{
			if (Visible() && (_lastTimeSpan.Hours != time.Hours || _lastTimeSpan.Minutes != time.Minutes || _lastTimeSpan.Seconds != time.Seconds))
			{
				_lastTimeSpan = time;
				string text = $"{time.Hours:00}:{time.Minutes:00}:{time.Seconds:00}";
				UIHelper.SetLabelText(RestockTimeLabel, text);
			}
		}
	}

	public interface Listener
	{
		bool OnCloseRequest(Shop closeThis);
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Shop/Shop";

	public UIScrollGrid ShopItemGrid;

	public RestockTimeUiData RestockTimeUi;

	public ItemDescription ShopItemDesc;

	public UIButton Button_Buy;

	public UIButton Button_Filter;

	public UISpriteSelector Sprite_FilterArrow;

	public UIButton Button_FilterActive;

	public UISpriteSelector Sprite_FilterActiveArrow;

	public UILabel Label_EmptyItem;

	public ShopItemSortFilter ShopItemSortFilter;

	public UIWidget IconRoot;

	public IconProxy IconProxy;

	public UILabel Label_Num;

	public UILabel Label_SelectGroup;

	public UIWidget WidgetShopItemList;

	public UIWidget WidgetShopItemListWithShopItemGroup;

	public UIScrollGrid SmallShopitemListGrid;

	public ShopItemGroupListComponent ShopItemGroupList;

	public UISimpleGrid BuffIconGrid;

	protected int _shopIndex;

	protected ShopData _shopData;

	protected int _selectedItemIndex;

	private int _selectedGroupIndex;

	private bool _isBootyItemEquip;

	private long _selectedItemId;

	protected Dictionary<int, ShopItemComponent> _shopItemDic = new Dictionary<int, ShopItemComponent>();

	private Dictionary<long, GuildRaidBootyItemComponent> _bootyEquipItemDic = new Dictionary<long, GuildRaidBootyItemComponent>();

	private Dictionary<long, GuildRaidBootyItemComponent> _bootyItemDic = new Dictionary<long, GuildRaidBootyItemComponent>();

	private DateTime _nextSpeechTime = DateTime.MinValue;

	private bool _restockPopup;

	private UIScrollGrid _curShopItemListGrid;

	private StatListComponent _statListComponent;

	private List<ClassTagIconComponent> _classTagIconComponentList = new List<ClassTagIconComponent>();

	private List<ShopItemGroupComponent> _ShopItemGroupComponentList = new List<ShopItemGroupComponent>();

	private ItemIconComponent _selectedItemIcon;

	private Listener _listener;

	private float _lastUpdateTime;

	private List<GeneralIconComponent> _buffIcons = new List<GeneralIconComponent>();

	private List<GeneralIconComponent> _removebuffIcons = new List<GeneralIconComponent>();

	protected NPCData _NpcData;

	private bool _resetPositionAtOpen;

	public StaminaType StaminaType => ShopHelper.GetStaminaType(_shopData);

	public new int WindowGuideIndex => ShopHelper.GetWindowGuideIndex(_shopData);

	UserManager.UserStatType LobbyTopmostMenu.IUserStatObserver.StatType => ShopHelper.GetStatType(_shopData);

	private bool SelectedItemBuyable()
	{
		if (ShopHelper.IsGuildBootyItemGroup(_shopData))
		{
			return ShopHelper.Buyable(GetSelectedGuildRaidBootyItemComponent(), _shopData);
		}
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

	private long GetCost()
	{
		if (ShopHelper.IsGuildBootyItemGroup(_shopData))
		{
			GuildRaidBootyItemComponent selectedGuildRaidBootyItemComponent = GetSelectedGuildRaidBootyItemComponent();
			if (selectedGuildRaidBootyItemComponent != null)
			{
				int price = selectedGuildRaidBootyItemComponent.GetPrice();
				return price;
			}
			int num = 0;
			return num;
		}
		return _shopItemDic[_selectedItemIndex].GetPrice();
	}

	private GuildRaidBootyItemComponent GetSelectedGuildRaidBootyItemComponent()
	{
		if (_isBootyItemEquip)
		{
			if (!_bootyEquipItemDic.ContainsKey(_selectedItemId))
			{
				return null;
			}
			return _bootyEquipItemDic[_selectedItemId];
		}
		if (!_bootyItemDic.ContainsKey(_selectedItemId))
		{
			return null;
		}
		return _bootyItemDic[_selectedItemId];
	}

	private ShopCostType GetSelectedItemCostType()
	{
		if (ShopHelper.IsGuildBootyItemGroup(_shopData) || ShopHelper.IsNormalGuildShop(_shopData))
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
		case CurrencyType.EventOrvelPoint:
			flag = CheckUpdateCostType(ShopCostType.EventOrvelPoint);
			break;
		case CurrencyType.ChallengeRaidPoint:
			flag = CheckUpdateCostType(ShopCostType.ChallengeRaidPoint);
			break;
		}
		if (flag)
		{
			UpdateShopItems();
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

	void IUserObserver<NGame2.NGuild.GuildRaidBootyEquipItemManager.ChangeItemInfo>.OnChanged(NGame2.NGuild.GuildRaidBootyEquipItemManager.ChangeItemInfo changeItemInfo)
	{
		if (!ShopHelper.IsGuildBootyItemGroup(_shopData))
		{
			return;
		}
		if (changeItemInfo.ChangeType == NGame2.NGuild.GuildRaidBootyEquipItemManager.EChangeType.Added)
		{
			if (_bootyEquipItemDic.ContainsKey(changeItemInfo.EquipItemInfo.Id))
			{
				VespaDebug.LogError("[GuildRaidBootyEquipItem] duplicated equip item id, id = " + changeItemInfo.EquipItemInfo.Id);
				return;
			}
			if (ShopItemSortFilter != null && !ShopItemSortFilter.IsNoneSelected())
			{
				return;
			}
			GuildRaidBootyItemComponent guildRaidBootyItemComponent = UIHelper.AddGridItem(_curShopItemListGrid, GuildRaidBootyItemComponent.PrefabAuto);
			guildRaidBootyItemComponent.Open(_shopData, changeItemInfo.EquipItemInfo, this, this);
			guildRaidBootyItemComponent.SetupNew();
			_bootyEquipItemDic.Add(changeItemInfo.EquipItemInfo.Id, guildRaidBootyItemComponent);
		}
		else if (changeItemInfo.ChangeType == NGame2.NGuild.GuildRaidBootyEquipItemManager.EChangeType.Removed)
		{
			if (!_bootyEquipItemDic.ContainsKey(changeItemInfo.EquipItemInfo.Id))
			{
				VespaDebug.LogError("[GuildRaidBootyEquipItem] not exist equip item id, id = " + changeItemInfo.EquipItemInfo.Id);
				return;
			}
			GuildRaidBootyItemComponent comp = _bootyEquipItemDic[changeItemInfo.EquipItemInfo.Id];
			if (_curShopItemListGrid != null)
			{
				_curShopItemListGrid.RemoveGridItem(comp);
			}
			_bootyEquipItemDic.Remove(changeItemInfo.EquipItemInfo.Id);
			if (_selectedItemId == changeItemInfo.EquipItemInfo.Id)
			{
				_selectedItemId = -1L;
				SetItemDescription();
			}
		}
		UIHelper.RepositionGrid(_curShopItemListGrid);
	}

	void IUserObserver<NGame2.NGuild.GuildRaidBootyItemManager.ChangeItemInfo>.OnChanged(NGame2.NGuild.GuildRaidBootyItemManager.ChangeItemInfo changeItemInfo)
	{
		if (!ShopHelper.IsGuildBootyItemGroup(_shopData))
		{
			return;
		}
		if (changeItemInfo.ChangeType == NGame2.NGuild.GuildRaidBootyItemManager.EChangeType.Added)
		{
			if (_bootyItemDic.ContainsKey(changeItemInfo.ItemInfo.Id))
			{
				VespaDebug.LogError("[GuildRaidBootyItem] duplicated item id, id = " + changeItemInfo.ItemInfo.Id);
				return;
			}
			if (ShopItemSortFilter != null && !ShopItemSortFilter.IsNoneSelected())
			{
				return;
			}
			GuildRaidBootyItemComponent guildRaidBootyItemComponent = UIHelper.AddGridItem(_curShopItemListGrid, GuildRaidBootyItemComponent.PrefabAuto);
			guildRaidBootyItemComponent.Open(_shopData, changeItemInfo.ItemInfo, this, this);
			guildRaidBootyItemComponent.SetupNew();
			_bootyItemDic.Add(changeItemInfo.ItemInfo.Id, guildRaidBootyItemComponent);
		}
		else if (changeItemInfo.ChangeType == NGame2.NGuild.GuildRaidBootyItemManager.EChangeType.Removed)
		{
			if (!_bootyItemDic.ContainsKey(changeItemInfo.ItemInfo.Id))
			{
				VespaDebug.LogError("[GuildRaidBootyItem] not exist item id, id = " + changeItemInfo.ItemInfo.Id);
				return;
			}
			GuildRaidBootyItemComponent comp = _bootyItemDic[changeItemInfo.ItemInfo.Id];
			if (_curShopItemListGrid != null)
			{
				_curShopItemListGrid.RemoveGridItem(comp);
			}
			_bootyItemDic.Remove(changeItemInfo.ItemInfo.Id);
			if (_selectedItemId == changeItemInfo.ItemInfo.Id)
			{
				_selectedItemId = -1L;
				SetItemDescription();
			}
		}
		UIHelper.RepositionGrid(_curShopItemListGrid);
	}

	public void Open(Listener listener, ShopData shopData, NPCData npcData)
	{
		Open(delegate
		{
			SetPlayerAssetInfo(shopData);
			Init(listener, shopData, npcData);
		}, null);
	}

	public void Init(Listener listener, ShopData shopData, NPCData npcData)
	{
		_shopData = shopData;
		if (_shopData != null)
		{
			SceneAutoBehaviour<ShopFacade>.instance.Reset(shopData.Index);
			_shopIndex = shopData.Index;
			_listener = listener;
			_NpcData = npcData;
			_resetPositionAtOpen = GameRoot.Instance.ConstantDataContainer.GetValueBool("ResetPositionAtOpen");
			RestockTimeUi.SetVisible(_shopData.MaxStock > 0);
			SetupObserver();
			SetupBuffIconGroup();
			SetupShopItemGrid();
			SetupFilter();
			SetupSort();
			if (!SceneAutoBehaviour<ShopFacade>.instance.NeedRestock())
			{
				SetShopItemList();
			}
			else
			{
				RequestShopItemListAsync().Commit();
			}
		}
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		UpdateRestockPopup();
		UpdateRestockTime();
		UpdateGuildBootyItem();
		UpdateBuffIcons();
	}

	private void UpdateRestockPopup()
	{
		if (SceneAutoBehaviour<ShopFacade>.instance.IsValidRestockTime && !_restockPopup && SceneAutoBehaviour<ShopFacade>.instance.NeedRestock())
		{
			string text = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
			string message = LocalizationManager.GetString(NShared.NLocalization.Shop.SHOP_RESTOCK_MSG);
			MessagePopup.Open(text, message);
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

	private void UpdateGuildBootyItem()
	{
		if (ShopHelper.IsGuildBootyItemGroup(_shopData) && !(Time.time - _lastUpdateTime < 1f))
		{
			_lastUpdateTime = Time.time;
			UpdateDestroyTime();
		}
	}

	private void UpdateDestroyTime()
	{
		GuildRaidBootyItemComponent selectedGuildRaidBootyItemComponent = GetSelectedGuildRaidBootyItemComponent();
		if (!(selectedGuildRaidBootyItemComponent == null))
		{
			string createdTimeUTC = string.Empty;
			if (selectedGuildRaidBootyItemComponent.BootyEquipItemInfo != null)
			{
				createdTimeUTC = selectedGuildRaidBootyItemComponent.BootyEquipItemInfo.CreatedTime;
			}
			if (selectedGuildRaidBootyItemComponent.BootyItemInfo != null)
			{
				createdTimeUTC = selectedGuildRaidBootyItemComponent.BootyItemInfo.CreatedTime;
			}
			string remainTimeUntilDestroy = RaidHelper.GetRemainTimeUntilDestroy(createdTimeUTC);
			string text = LocalizationManager.GetString(GuildRaid.GUILDRAID_BOOTY_ITEM_DESTROY, remainTimeUntilDestroy);
			UIHelper.SetLabelText(ShopItemDesc.Label_RemainTime, text);
		}
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

	public void SetPlayerAssetInfo(ShopData shopData)
	{
		UserManager.UserStatType o_value = UserManager.UserStatType.None;
		UserManager.UserStatType o_value2 = UserManager.UserStatType.None;
		string spriteName = string.Empty;
		string spriteNameSub = string.Empty;
		if (__Extension_Method_ICollection__.COUNT(shopData.BuyCostTypeUI) > 0)
		{
			if (EnumTools.TryParse<UserManager.UserStatType>(shopData.BuyCostTypeUI[0].ToString(), out o_value))
			{
				UserStat = o_value;
			}
			if (__Extension_Method_ICollection__.COUNT(shopData.BuyCostTypeUI) > 1)
			{
				EnumTools.TryParse<UserManager.UserStatType>(shopData.BuyCostTypeUI[1].ToString(), out o_value2);
			}
		}
		if (shopData.BuyCostTypeIconSprite != null)
		{
			if (shopData.BuyCostTypeIconSprite.Length != 0)
			{
				spriteName = shopData.BuyCostTypeIconSprite[0];
			}
			if (shopData.BuyCostTypeIconSprite.Length > 1)
			{
				spriteNameSub = shopData.BuyCostTypeIconSprite[1];
			}
		}
		if ((uint)(o_value - 35) <= 1u || o_value == UserManager.UserStatType.EventDungeonPoint3)
		{
			LobbyTopmostMenu.EventDungeonAssetInfo eventDungeonAssetInfo = new LobbyTopmostMenu.EventDungeonAssetInfo
			{
				StatType = o_value,
				StatTypeSub = o_value2,
				SpriteName = spriteName,
				SpriteNameSub = spriteNameSub
			};
			SetEventDungeonAssetInfo(eventDungeonAssetInfo);
		}
		else
		{
			LobbyTopmostMenu.SubStatAssetInfo subAssetInfo = new LobbyTopmostMenu.SubStatAssetInfo
			{
				StatTypeSub = o_value2
			};
			SetSubAssetInfo(subAssetInfo);
		}
	}

	private void SetupObserver()
	{
		if (ShopHelper.IsGuildBootyItemGroup(_shopData))
		{
			if (GlobalAutoBehaviour<GuildManager>.instance.GuildRaidBootyEquipItemManager != null)
			{
				GlobalAutoBehaviour<GuildManager>.instance.GuildRaidBootyEquipItemManager.ObserverManager.AddObserver(this);
			}
			if (GlobalAutoBehaviour<GuildManager>.instance.GuildRaidBootyItemManager != null)
			{
				GlobalAutoBehaviour<GuildManager>.instance.GuildRaidBootyItemManager.ObserverManager.AddObserver(this);
			}
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
			else if (_shopData.ShopFilterGroupType == EShopItemFilterGroupType.GuildRaid)
			{
				_curShopItemListGrid.Grid.sorting = UIGrid.Sorting.Custom;
				_curShopItemListGrid.Grid.onCustomSort = SortGuildRaidBootyItemOrder;
			}
			else if (_shopData.ShopFilterGroupType == EShopItemFilterGroupType.Guild)
			{
				_curShopItemListGrid.Grid.sorting = UIGrid.Sorting.Custom;
				_curShopItemListGrid.Grid.onCustomSort = SortShopItemOrder;
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
		else if (ShopHelper.IsGuildBootyItemGroup(_shopData))
		{
			SetGuildRaidBootyItemShopList();
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

	private void SetGuildRaidBootyItemShopList()
	{
		List<GuildRaidBootyEquipItemInfo> list = ((GlobalAutoBehaviour<GuildManager>.instance.GuildRaidBootyEquipItemManager != null) ? GlobalAutoBehaviour<GuildManager>.instance.GuildRaidBootyEquipItemManager.ToList() : null);
		List<GuildRaidBootyItemInfo> list2 = ((GlobalAutoBehaviour<GuildManager>.instance.GuildRaidBootyItemManager != null) ? GlobalAutoBehaviour<GuildManager>.instance.GuildRaidBootyItemManager.ToList() : null);
		if (__Extension_Method_ICollection__.COUNT(list) == 0)
		{
			_ = __Extension_Method_ICollection__.COUNT(list2) == 0;
		}
		else
			_ = 0;
		bool flag = true;
		if (ShopItemFilterByType(EShopItemType.EquipItem))
		{
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(list); i++)
			{
				flag = false;
				CreateGuildBootyItem(list[i]);
			}
		}
		for (int j = 0; j < __Extension_Method_ICollection__.COUNT(list2); j++)
		{
			GuildRaidBootyItemInfo guildRaidBootyItemInfo = list2[j];
			ItemData data = GameRoot.Instance.ItemDataContainer.GetData(guildRaidBootyItemInfo.ItemIndex);
			if (data != null)
			{
				ShopItemFilterData dataByCode = GameRoot.Instance.ShopItemFilterDataContainer.GetDataByCode(data.Code);
				if (dataByCode != null && ShopItemFilter(dataByCode))
				{
					flag = false;
					CreateGuildBootyItem(guildRaidBootyItemInfo);
				}
			}
		}
		_selectedItemId = -1L;
		UIHelper.RepositionGrid(_curShopItemListGrid);
		SetItemDescription();
		UIHelper.EnableGameObject(Label_EmptyItem, flag && !ShopItemSortFilter.IsNoneSelected());
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
		ResetItemDescription();
		if (IsAllShopItemSold())
		{
			ShopItemDesc.SetAllSoldOut();
			return;
		}
		string code = string.Empty;
		ShopItemData shopItemData = null;
		int starCount = 0;
		int num = 0;
		long itemCount = num;
		if (_shopData.MaxStock > 0)
		{
			ShopListItemInfo shopListItemInfo = GlobalAutoBehaviour<AccountManager>.instance.ShopItemManager.Get(_shopIndex, _selectedItemIndex);
			if (shopListItemInfo != null)
			{
				code = shopListItemInfo.ItemCode;
				starCount = shopListItemInfo.Star;
				int count = shopListItemInfo.Count;
				itemCount = count;
			}
		}
		else if (ShopHelper.IsGuildBootyItemGroup(_shopData))
		{
			code = ShopHelper.GetItemCodeFromBootyItem(_selectedItemId, _isBootyItemEquip, out starCount, out itemCount);
		}
		else
		{
			shopItemData = GameRoot.Instance.ShopItemDataContainer.GetData(_shopIndex, _selectedItemIndex);
			if (shopItemData != null)
			{
				code = shopItemData.ItemCode;
				starCount = shopItemData.Star;
				int count2 = shopItemData.Count;
				itemCount = count2;
			}
		}
		UIHelper.EnableGameObject(ShopItemDesc.Label_PurchaseLimit, enable: false);
		UIHelper.EnableGameObject(ShopItemDesc.Label_PurchaseCount, enable: false);
		if (_shopItemDic.ContainsKey(_selectedItemIndex) && _shopItemDic[_selectedItemIndex].ShopItemData != null && _shopItemDic[_selectedItemIndex].ShopItemData.PurchasableCount > 0)
		{
			UIHelper.EnableGameObject(ShopItemDesc.Label_PurchaseLimit, enable: true);
			UIHelper.EnableGameObject(ShopItemDesc.Label_PurchaseCount, enable: true);
			int remainPurchasableCount = _shopItemDic[_selectedItemIndex].GetRemainPurchasableCount();
			UIHelper.SetLabelText(ShopItemDesc.Label_PurchaseCount, $"{remainPurchasableCount}/{_shopItemDic[_selectedItemIndex].ShopItemData.PurchasableCount}");
		}
		ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(code);
		if (dataByCode == null)
		{
			ResetItemDescriptionUI();
			return;
		}
		ShopItemDesc.ActivateCommon(Active: true);
		UIHelper.SetLabelText(ShopItemDesc.Label_ItemName, ItemNameBuilder.GetItemName(dataByCode));
		EquipItemInfo equipItemInfo = CreateFakeEquipInfo(dataByCode, starCount);
		_selectedItemIcon = UIHelper.AddGridItem(ShopItemDesc.ItemGrid, ItemIconComponent.PrefabAuto);
		_selectedItemIcon.Open(dataByCode, equipItemInfo, null);
		_selectedItemIcon.SetItemCount(itemCount);
		UIHelper.EnableGameObject(ShopItemDesc.Label_Description, dataByCode.Type != ItemType.Equip);
		if (dataByCode.Type == ItemType.Equip)
		{
			SetupEquipItem(dataByCode, equipItemInfo);
		}
		else
		{
			SetupItem(dataByCode);
		}
		UIHelper.EnableButton(Button_Buy, enable: true);
		ShopCostType costType = ShopCostType.None;
		if (ShopHelper.IsGuildBootyItemGroup(_shopData))
		{
			costType = ShopCostType.GuildPoint;
		}
		else if (_shopItemDic.ContainsKey(_selectedItemIndex))
		{
			ShopItemComponent shopItemComponent = _shopItemDic[_selectedItemIndex];
			costType = shopItemComponent.GetCurrentCostType();
			shopItemComponent.GetSpriteNameByCostType(costType);
		}
		long cost = GetCost();
		SetupIconProxy(costType.ToString());
		SetLabelNum(cost);
		if (ShopHelper.IsGuildBootyItemGroup(_shopData))
		{
			UpdateDestroyTime();
		}
		else
		{
			UIHelper.SetLabelText(ShopItemDesc.Label_RemainTime, string.Empty);
		}
	}

	private void SetupIconProxy(string costTypeName)
	{
		if (IconProxy == null)
		{
			IconProxy = IconProxy.Create(IconRoot);
		}
		if (IconProxy != null)
		{
			IconProxy.SetupCurrencyNoBg(costTypeName);
		}
	}

	public void SetLabelNum(long Num)
	{
		UIHelper.EnableGameObject(Label_Num, enable: true);
		UIHelper.SetLabelText(Label_Num, Vespa_Long_Extension_Method.ToCommaString(Num));
	}

	private void SetupClassTagIcon(EquipItemData itemData)
	{
		if (ShopItemDesc.ClassTagGrid == null || itemData == null)
		{
			return;
		}
		CreatureTagType[] tagType = itemData.TagType;
		if (__Extension_Method_ICollection__.IsNullOrEmpty(tagType))
		{
			return;
		}
		if (GameRoot.Instance.EquipItemDataContainer.ContainsAllClassTag(itemData.EquipCode))
		{
			ClassTagIconComponent classTagIconComponent = UIHelper.AddGridItem(ShopItemDesc.ClassTagGrid, ClassTagIconComponent.PrefabAuto);
			_classTagIconComponentList.Add(classTagIconComponent);
			classTagIconComponent.OpenAllClassTag(null);
			UIHelper.RepositionGrid(ShopItemDesc.ClassTagGrid);
			return;
		}
		for (int num = tagType.Length - 1; num >= 0; num--)
		{
			CreatureTagType tagType2 = tagType[num];
			ClassTagIconComponent classTagIconComponent2 = UIHelper.AddGridItem(ShopItemDesc.ClassTagGrid, ClassTagIconComponent.PrefabAuto);
			if (!(classTagIconComponent2 == null))
			{
				_classTagIconComponentList.Add(classTagIconComponent2);
				classTagIconComponent2.Open(tagType2, null);
			}
		}
		UIHelper.RepositionGrid(ShopItemDesc.ClassTagGrid);
	}

	private void SetupUniqueWidget(int heroIndex)
	{
		CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(heroIndex);
		UIHelper.SetLabelText(ShopItemDesc.Label_CreatureLimit, data.Name);
		AssetTexture heroIcon = UIHelper.GetHeroIcon2(heroIndex, 3);
		UIHelper.ResetPlaneAssetTextureAsync(this, ShopItemDesc.UniqueHeroTexture, heroIcon);
	}

	private void SetupUniqueAndClass(EquipItemData itemData)
	{
		if (itemData != null)
		{
			bool flag = !__Extension_Method_ICollection__.IsNullOrEmpty(itemData.CreatureIndex);
			if (flag)
			{
				SetupUniqueWidget(itemData.CreatureIndex[0]);
			}
			else
			{
				SetupClassTagIcon(itemData);
			}
			UIHelper.EnableGameObject(ShopItemDesc.ClassTagGrid, !flag);
			UIHelper.EnableGameObject(ShopItemDesc.UniqueWidget, flag);
		}
	}

	private void SetupEquipItem(ItemData itemData, EquipItemInfo equipItemInfo)
	{
		UIHelper.EnableGameObject(ShopItemDesc.WidgetEquipSubInfo, enable: true);
		UIHelper.EnableGameObject(ShopItemDesc.Label_ItemType, enable: false);
		UIHelper.EnableGameObject(ShopItemDesc.StatRootWidget, enable: true);
		ItemData<EquipItemData> data = GameRoot.Instance.ItemDataContainer.GetData<EquipItemData>(itemData.Code);
		if (data != null)
		{
			SetupUniqueAndClass(data.SubData);
			UIHelper.SetLabelText(ShopItemDesc.Label_EquipLevel, (data.SubData.ReqLevel <= 0) ? 1 : data.SubData.ReqLevel);
			StatData statData = new StatData();
			statData.AddEquipItemStat(data.SubData, equipItemInfo?.Star ?? 0, data.SubData.DetailIndex, 0);
			_statListComponent = StatListComponent.Create(ShopItemDesc.StatRootWidget, 5);
			_statListComponent.AddStat(statData);
			int num = GameRoot.Instance.EquipItemDetailDataContainer.GetData(data.SubData.DetailIndex)?.OptionCount ?? 0;
			if (num > 0)
			{
				_statListComponent.AddOptionSubject(LocalizationManager.GetString(EquipEnchant.RANDOM_OPTION));
			}
			for (int i = 1; i <= num; i++)
			{
				GameRoot.Instance.EquipItemLevelDataContainer.GetEquipItemOptionLevel(i);
				string typeName = LocalizationManager.GetString(Equip.EQUIP_OPTION_STAT_TEXT);
				_statListComponent.AddCustomStat(typeName, null, _statListComponent.OptionClosedStatValueColor);
			}
			StatListComponent.AddOptionStat(_statListComponent, data.GetSubData<EquipItemData>(), equipItemInfo);
			StatListComponent.AddExtraOptionStat(_statListComponent, data.GetSubData<EquipItemData>(), equipItemInfo);
			StatListComponent.AddSetOptionStat(_statListComponent, null, data.GetSubData<EquipItemData>(), equipItemInfo);
			if (ShopItemDesc.EquipItemDescScrollView != null)
			{
				ShopItemDesc.EquipItemDescScrollView.ResetPosition();
			}
		}
	}

	private void SetupItem(ItemData itemData)
	{
		UIHelper.EnableGameObject(ShopItemDesc.WidgetEquipSubInfo, enable: false);
		UIHelper.EnableGameObject(ShopItemDesc.Label_ItemType, enable: true);
		UIHelper.EnableGameObject(ShopItemDesc.StatRootWidget, enable: false);
		switch (itemData.Type)
		{
		case ItemType.Recipe:
		case ItemType.Material:
			UIHelper.SetLabelText(ShopItemDesc.Label_ItemType, LocalizationManager.GetString(Item.ITEM_MATERIAL));
			break;
		case ItemType.UpgradeBook:
			UIHelper.SetLabelText(ShopItemDesc.Label_ItemType, LocalizationManager.GetString(Item.ITEM_UPGRADEBOOK));
			break;
		default:
			UIHelper.SetLabelText(ShopItemDesc.Label_ItemType, string.Empty);
			break;
		}
		if (itemData.Type == ItemType.Potion)
		{
			UIHelper.SetLabelText(ShopItemDesc.Label_Description, itemData.Description2);
		}
		else if (itemData.Type == ItemType.Flask)
		{
			ItemData<FlaskItemData> data = GameRoot.Instance.ItemDataContainer.GetData<FlaskItemData>(itemData.Index);
			if (data != null)
			{
				string text = itemData.Description2.Replace("{1}", data.GetSubData<FlaskItemData>().FlaskExp.ToString());
				UIHelper.SetLabelText(ShopItemDesc.Label_Description, text);
			}
			else
			{
				UIHelper.SetLabelText(ShopItemDesc.Label_Description, itemData.Description2);
			}
		}
		else
		{
			UIHelper.SetLabelText(ShopItemDesc.Label_Description, itemData.Description2);
		}
	}

	private void ResetItemDescription()
	{
		FactoryBase.ReleaseComponent(_selectedItemIcon);
		FactoryBase.SafeReleaseComponent(ref _statListComponent);
		FactoryBase.ReleaseAndClearList(_classTagIconComponentList);
		ResetItemDescriptionUI();
		_selectedItemIcon = null;
	}

	private void ResetItemDescriptionUI()
	{
		ShopItemDesc.SetEmpty();
		UIHelper.EnableButton(Button_Buy, enable: false);
		SetupIconProxy(ShopCostType.None.ToString());
		UIHelper.EnableGameObject(Label_Num, enable: false);
	}

	private void CreateShopListItem(ShopItemData shopItemData)
	{
		if (String_Extension_Method.IsNullOrEmpty(shopItemData.Condition) || GlobalAutoBehaviour<DropBonusEventManager>.instance.GetEffectiveEvent(EventBonusType.ShopList, shopItemData.Condition) != null)
		{
			ShopItemInfo shopItemPurchaseInfo = SceneAutoBehaviour<ShopManagement>.instance.GetShopItemPurchaseInfo(_shopData.Index, shopItemData.Index);
			ShopItemComponent shopItemComponent = UIHelper.AddGridItem(_curShopItemListGrid, ShopItemComponent.PrefabAuto);
			shopItemComponent.Open(shopItemData, shopItemPurchaseInfo, this, this);
			_shopItemDic.Add(shopItemData.Index, shopItemComponent);
		}
	}

	private void CreateShopListItem(ShopListItemInfo info)
	{
		ShopItemComponent shopItemComponent = UIHelper.AddGridItem(_curShopItemListGrid, ShopItemComponent.PrefabAuto);
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

	private void CreateGuildBootyItem(GuildRaidBootyEquipItemInfo info)
	{
		GuildRaidBootyItemComponent guildRaidBootyItemComponent = UIHelper.AddGridItem(_curShopItemListGrid, GuildRaidBootyItemComponent.PrefabAuto);
		guildRaidBootyItemComponent.Open(_shopData, info, this, this);
		_bootyEquipItemDic.Add(info.Id, guildRaidBootyItemComponent);
	}

	private void CreateGuildBootyItem(GuildRaidBootyItemInfo info)
	{
		GuildRaidBootyItemComponent guildRaidBootyItemComponent = UIHelper.AddGridItem(_curShopItemListGrid, GuildRaidBootyItemComponent.PrefabAuto);
		guildRaidBootyItemComponent.Open(_shopData, info, this, this);
		_bootyItemDic.Add(info.Id, guildRaidBootyItemComponent);
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

	private int SortGuildRaidBootyItemOrder(Transform l, Transform r)
	{
		GuildRaidBootyItemComponent component = l.GetComponent<GuildRaidBootyItemComponent>();
		GuildRaidBootyItemComponent component2 = r.GetComponent<GuildRaidBootyItemComponent>();
		if (component == null || component2 == null)
		{
			return 0;
		}
		int num = 0;
		if (ShopItemSortFilter.Sort(EShopSortType.TimeLowToHigh))
		{
			num = CompareValue(isAscending: true, component.GetCreatedTime(), component2.GetCreatedTime());
			if (num != 0)
			{
				return num;
			}
		}
		if (ShopItemSortFilter.Sort(EShopSortType.TimeHighToLow))
		{
			num = CompareValue(isAscending: false, component.GetCreatedTime(), component2.GetCreatedTime());
			if (num != 0)
			{
				return num;
			}
		}
		if (ShopItemSortFilter.Sort(EShopSortType.GradeLowToHigh))
		{
			ItemData data = GameRoot.Instance.ItemDataContainer.GetData(component.GetItmeIndex());
			ItemData data2 = GameRoot.Instance.ItemDataContainer.GetData(component2.GetItmeIndex());
			num = CompareValue(isAscending: true, data.Grade, data2.Grade);
			if (num != 0)
			{
				return num;
			}
		}
		if (ShopItemSortFilter.Sort(EShopSortType.GradeHighToLow))
		{
			ItemData data3 = GameRoot.Instance.ItemDataContainer.GetData(component.GetItmeIndex());
			ItemData data4 = GameRoot.Instance.ItemDataContainer.GetData(component2.GetItmeIndex());
			num = CompareValue(isAscending: false, data3.Grade, data4.Grade);
			if (num != 0)
			{
				return num;
			}
		}
		if (ShopItemSortFilter.Sort(EShopSortType.PriceLowToHigh))
		{
			num = CompareValue(isAscending: true, component.GetPrice(), component2.GetPrice());
			if (num != 0)
			{
				return num;
			}
		}
		if (ShopItemSortFilter.Sort(EShopSortType.PriceHighToLow))
		{
			num = CompareValue(isAscending: false, component.GetPrice(), component2.GetPrice());
			if (num != 0)
			{
				return num;
			}
		}
		return component.GetItmeId().CompareTo(component2.GetItmeId());
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
		if (ShopHelper.IsGuildBootyItemGroup(_shopData))
		{
			foreach (GuildRaidBootyItemComponent value in _bootyEquipItemDic.Values)
			{
				value.SetPriceLabelColor();
			}
			foreach (GuildRaidBootyItemComponent value2 in _bootyItemDic.Values)
			{
				value2.SetPriceLabelColor();
			}
		}
		foreach (ShopItemComponent value3 in _shopItemDic.Values)
		{
			value3.SetPriceLabelColor();
		}
	}

	public void OnEnable()
	{
		VespaCoroutine.Start(RefreshUI());
	}

	private IEnumerator RefreshUI()
	{
		for (int i = 0; i < 5; i++)
		{
			UIHelper.EnableGameObject(WidgetShopItemListWithShopItemGroup.transform.parent.gameObject, enable: false);
			UIHelper.EnableGameObject(WidgetShopItemListWithShopItemGroup.transform.parent.gameObject, enable: true);
			yield return null;
		}
	}

	private void NPCSpeak(ENPCSpeechEvent speechEvent)
	{
		if (_shopData != null)
		{
			_ = string.Empty;
			if (speechEvent == ENPCSpeechEvent.Buy)
			{
				SceneAutoBehaviour<EntryMenuManagement>.instance.SetNPCAction(_shopData.Buyline);
			}
			SetNextSpeechTime((speechEvent == ENPCSpeechEvent.Buy) ? 3f : 1f);
		}
	}

	public void SetNextSpeechTime(double seconds)
	{
		_nextSpeechTime = DateTime.UtcNow.AddSeconds(seconds);
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
				string text = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
				string message = LocalizationManager.GetString(NShared.NLocalization.Shop.BUY_CONFIRM_MSG);
				MessagePopup.Open(text, message, delegate
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
		if (ShopHelper.IsGuildBootyItemGroup(_shopData))
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
		if (ShopHelper.IsGuildBootyItemGroup(_shopData))
		{
			GuildRaidBootyItemComponent selectedGuildRaidBootyItemComponent = GetSelectedGuildRaidBootyItemComponent();
			ExecBuyItem(selectedGuildRaidBootyItemComponent);
		}
		else
		{
			ShopItemComponent selectedComponent = (_shopItemDic.ContainsKey(_selectedItemIndex) ? _shopItemDic[_selectedItemIndex] : null);
			ExecBuyItem(selectedComponent, purchaseCount);
		}
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
		else if (ShopHelper.IsGuildBootyItemGroup(_shopData))
		{
			if (_selectedItemId <= 0)
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

	private void ExecBuyItem(GuildRaidBootyItemComponent SelectedBootyItemComponent)
	{
		if (SelectedBootyItemComponent == null)
		{
			return;
		}
		long itemId = 0L;
		int itemIndex = 0;
		int itemCount = 0;
		bool isEquip = true;
		if (SelectedBootyItemComponent.BootyEquipItemInfo != null)
		{
			itemId = SelectedBootyItemComponent.BootyEquipItemInfo.Id;
			itemIndex = SelectedBootyItemComponent.BootyEquipItemInfo.ItemIndex;
			itemCount = 1;
		}
		if (SelectedBootyItemComponent.BootyItemInfo != null)
		{
			itemId = SelectedBootyItemComponent.BootyItemInfo.Id;
			itemIndex = SelectedBootyItemComponent.BootyItemInfo.ItemIndex;
			itemCount = SelectedBootyItemComponent.BootyItemInfo.Count;
			isEquip = false;
		}
		new WebServiceTask<NShared.BuyGuildRaidBootyItem.Request, NShared.BuyGuildRaidBootyItem.Response>(new NShared.BuyGuildRaidBootyItem.Request
		{
			ShopIndex = _shopIndex,
			Id = itemId,
			ItemIndex = itemIndex,
			Count = itemCount
		}, delegate(NShared.BuyGuildRaidBootyItem.Request request, NShared.BuyGuildRaidBootyItem.Response response)
		{
			if (response.Result == NShared.BuyGuildRaidBootyItem.ResultType.EquipItemFull)
			{
				return new DelegateTask(delegate
				{
					string text = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
					string message = LocalizationManager.GetString(NShared.BeginCampaign.ResultType.EquipItemFull);
					MessagePopup.OpenEnabled(text, message, delegate
					{
						SceneAutoBehaviour<InventoryManagement>.instance.OpenInventory();
					}, null, "MOVE_BAG", bYesEnabled: true);
				});
			}
			if (response.Result != NShared.BuyGuildRaidBootyItem.ResultType.Success)
			{
				return new DelegateTask(delegate
				{
					MessagePopup.OpenError(response.Result);
				});
			}
			GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.ItemResult);
			GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.GuildPointResult);
			GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.Apply(response.EquipItemInfo);
			ItemData data = GameRoot.Instance.ItemDataContainer.GetData(itemIndex);
			if (data != null)
			{
				CenterMessage.Open(LocalizationManager.GetString(NShared.NLocalization.Shop.SHOP_BOUGHT_MSG, ItemNameBuilder.GetItemName(data), itemCount));
			}
			if (isEquip)
			{
				GlobalAutoBehaviour<GuildManager>.instance.GuildRaidBootyEquipItemManager.RemoveItem(itemId);
			}
			else
			{
				GlobalAutoBehaviour<GuildManager>.instance.GuildRaidBootyItemManager.RemoveItem(itemId);
			}
			NPCSpeak(ENPCSpeechEvent.Buy);
			return (ITask)null;
		}).Commit();
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
					string text = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
					string message = LocalizationManager.GetString(NShared.BeginCampaign.ResultType.EquipItemFull);
					MessagePopup.OpenEnabled(text, message, delegate
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
			NPCSpeak(ENPCSpeechEvent.Buy);
			return (ITask)null;
		}).Commit();
	}

	private void PopUpInsufficentMessage()
	{
		string text = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
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
				Close();
			};
			break;
		case ShopCostType.PvpCoin:
			key = NShared.NLocalization.System.MOVE_MATCH_MENU_CONFIRM;
			onYes = delegate
			{
				Close();
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
		case ShopCostType.EventOrvelPoint:
		{
			string stringDirect2 = LocalizationManager.GetStringDirect("EVENT_FORGE_SHOP_CONDITION");
			MessagePopup.Open(text, stringDirect2);
			return;
		}
		case ShopCostType.ChallengeRaidPoint:
		{
			string stringDirect = LocalizationManager.GetStringDirect("@챌린지레이드 포인트 부족");
			MessagePopup.Open(text, stringDirect);
			return;
		}
		}
		MessagePopup.Open(text, LocalizationManager.GetString(key), onYes, null);
	}

	public void OnClickExitButton()
	{
		if (_listener == null || _listener.OnCloseRequest(this))
		{
			Close();
		}
	}

	public override void OnSystemBackButton()
	{
		OnClickExitButton();
	}

	protected void OnClickShopGroup(ShopItemGroupComponent clicked)
	{
		if (clicked.ShopItemGroupData.GroupIndex == _selectedGroupIndex)
		{
			return;
		}
		_selectedGroupIndex = clicked.ShopItemGroupData.GroupIndex;
		Clear();
		if (ShopHelper.IsGuildBootyItemGroup(_shopData))
		{
			SetGuildRaidBootyItemShopList();
			return;
		}
		List<ShopItemData> shopItemListByShopItemGroup = GameRoot.Instance.ShopItemDataContainer.GetShopItemListByShopItemGroup(_shopData.Index, clicked.ShopItemGroupData.GroupIndex);
		if (__Extension_Method_ICollection__.COUNT(shopItemListByShopItemGroup) > 0)
		{
			shopItemListByShopItemGroup.Sort(SortWithShowIndex);
		}
		CreateShopList(shopItemListByShopItemGroup);
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

	bool GuildRaidBootyItemComponent.IDataProvider.IsGreaterThanPrice(GuildRaidBootyItemComponent caller)
	{
		return ShopHelper.Buyable(caller, _shopData);
	}

	bool GuildRaidBootyItemComponent.IDataProvider.IsNew(GuildRaidBootyItemComponent caller)
	{
		if (caller.BootyEquipItemInfo != null)
		{
			return GlobalAutoBehaviour<GuildManager>.instance.GuildRaidBootyEquipItemManager.IsNewItem(caller.BootyEquipItemInfo.Id);
		}
		if (caller.BootyItemInfo != null)
		{
			return GlobalAutoBehaviour<GuildManager>.instance.GuildRaidBootyItemManager.IsNewItem(caller.BootyItemInfo.Id);
		}
		return false;
	}

	void GuildRaidBootyItemComponent.Listener.OnClickComponent(GuildRaidBootyItemComponent caller)
	{
		GuildRaidBootyItemComponent selectedGuildRaidBootyItemComponent = GetSelectedGuildRaidBootyItemComponent();
		if (selectedGuildRaidBootyItemComponent != null)
		{
			selectedGuildRaidBootyItemComponent.SetSelected(Select: false);
		}
		caller.SetSelected(Select: true);
		if (caller.BootyEquipItemInfo != null)
		{
			GlobalAutoBehaviour<GuildManager>.instance.GuildRaidBootyEquipItemManager.RemoveNew(caller.BootyEquipItemInfo.Id);
			_isBootyItemEquip = true;
			_selectedItemId = caller.BootyEquipItemInfo.Id;
		}
		if (caller.BootyItemInfo != null)
		{
			GlobalAutoBehaviour<GuildManager>.instance.GuildRaidBootyItemManager.RemoveNew(caller.BootyItemInfo.Id);
			_isBootyItemEquip = false;
			_selectedItemId = caller.BootyItemInfo.Id;
		}
		caller.SetupNew();
		SetItemDescription();
	}

	public override void Close()
	{
		base.Close();
		SceneAutoBehaviour<EntryMenuManagement>.instance.ForceShopGroupIndex = null;
	}

	public override void Reset()
	{
		base.Reset();
		_nextSpeechTime = DateTime.MinValue;
		_restockPopup = false;
		_selectedGroupIndex = -1;
		FactoryBase.ReleaseAndClearList(_ShopItemGroupComponentList);
		UIHelper.EnableGameObject(Label_SelectGroup, enable: false);
		UIHelper.EnableGameObject(Label_EmptyItem, enable: false);
		Clear();
		ClearObserver();
		ClearFilter();
	}

	private void Clear()
	{
		RestockTimeUi.Clear();
		foreach (ShopItemComponent value in _shopItemDic.Values)
		{
			value.Close();
		}
		FactoryBase.ReleaseAndClearList(_shopItemDic);
		FactoryBase.ReleaseAndClearList(_bootyEquipItemDic);
		FactoryBase.ReleaseAndClearList(_bootyItemDic);
		FactoryBase.SafeReleaseComponent(ref IconProxy);
		_selectedItemId = -1L;
		_selectedItemIndex = -1;
	}

	private void ClearBuffIcon()
	{
		FactoryBase.ReleaseAndClearList(_buffIcons);
	}

	private void ClearObserver()
	{
		if (ShopHelper.IsGuildBootyItemGroup(_shopData))
		{
			if (GlobalAutoBehaviour<GuildManager>.instance.GuildRaidBootyEquipItemManager != null)
			{
				GlobalAutoBehaviour<GuildManager>.instance.GuildRaidBootyEquipItemManager.ObserverManager.RemoveObserver(this);
			}
			if (GlobalAutoBehaviour<GuildManager>.instance.GuildRaidBootyItemManager != null)
			{
				GlobalAutoBehaviour<GuildManager>.instance.GuildRaidBootyItemManager.ObserverManager.RemoveObserver(this);
			}
		}
	}

	private void ClearFilter()
	{
		if (ShopItemSortFilter != null)
		{
			ShopItemSortFilter.Close();
			Sprite_FilterArrow.transform.localEulerAngles = Vector3.zero;
		}
	}

	public override void OnClosing()
	{
		base.OnClosing();
		Clear();
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using NGame2.NAccount;
using NGame2.NDropBonusEvent;
using NGame2.NScene.NPreloader;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NHelper;
using NGame2.NUtil.NStringProxy;
using NShared;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using NVespa.NUtil;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class InvenItemDesc : BaseWindow, EquipRuneSlotInfoComponent.IDataProvider, IUserObserver<ItemInfo>, IUserObserver, IUserObserver<NGame2.NAccount.UserEquipItemManager.EquipStatInfo>
{
	public interface Listener
	{
		bool OnCloseRequest(InvenItemDesc CloseThis);

		void OnSell(InvenItemDesc caller);

		void OnToHero(InvenItemDesc caller);

		void OnShortcut(InvenItemDesc caller);

		void OnUseItem(InvenItemDesc caller);

		void OnOpenItem(InvenItemDesc caller, long ItemCount);

		void OnLockRuneItem(InvenItemDesc caller);

		void OnUnlockRuneItem(InvenItemDesc caller);

		void OnBreakRuneItem(InvenItemDesc caller);

		void OnBreakItem(InvenItemDesc caller, bool isSoulStone);

		void OpenRestoreSoulStone();

		void MoveDirect(MoveDirectRegistInspector moveDirect);
	}

	[Serializable]
	public class ButtonGroup
	{
		public ItemType Type;

		public GameObject Button;
	}

	[Serializable]
	public class DualStatUiData
	{
		public UILabel Label_DualStatType;

		public UILabel Label_DualStatValue;
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Inventory/InvenItemDesc2";

	public UILabel Label_ItemName;

	public UIWidget Widget_Desc;

	public UIScrollView DescScrollView;

	public UILabel Label_Description;

	public UILabel Label_Description2;

	public Transform ItemParent;

	public UILabel Label_SellPrice;

	public ButtonGroup[] Buttons;

	public UIPanel StatPanel;

	public UIScrollView StatScrollView;

	public UIButton Button_Sell;

	public UIButton Button_OpenPackage;

	public UILabel Label_OpenPackageCount;

	public UILabel Label_Shortcut;

	public UIButton Button_Shortcut;

	public UIToggle Toggle_ETC_Shortcut;

	public GameObject PackageItemList;

	public UIScrollView PackageItemListScrollView;

	public UIWidget WidgetRune;

	[Header("Rune Part")]
	public UIWidget Widget_SingleStat;

	public UILabel Label_StatType;

	public UILabel Label_StatValue;

	public UIWidget Widget_AddSkill;

	public UILabel Label_AddSkill;

	public UIWidget Widget_DualStat;

	public List<DualStatUiData> DualStatList = new List<DualStatUiData>();

	[Header("[Rune Slot]")]
	public EquipRuneSlotInfoComponent EquipRuneSlotInfoComponent;

	public UIPlayAssetSound ItemUseSound;

	public UIButton Button_Lock;

	public UIButton Button_Unlock;

	public InvenItemMoveDirect InvenItemMoveDirect;

	private Listener _listener;

	private ItemIconComponent _itemIconComponent;

	private List<PackageRewardItemListComponent> _ListPackageRewardItemListComponent = new List<PackageRewardItemListComponent>();

	public ItemData ItemData { get; private set; }

	public EquipItemInfo EquipItemInfo { get; private set; }

	public long ItemCount { get; private set; }

	public void Open(ItemData itemData, long itemCount, Listener listener)
	{
		Open(delegate
		{
			Init(itemData, itemCount, listener);
		}, null);
	}

	public void Init(ItemData itemData, long itemCount, Listener listener)
	{
		_listener = listener;
		ItemData = itemData;
		ItemCount = itemCount;
		EquipItemInfo = null;
		GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.ObserverManager.AddObserver(this);
		if (EquipRuneSlotInfoComponent != null)
		{
			EquipRuneSlotInfoComponent.Open(this);
		}
		SetItemInfo();
		SetMoveDirectInfo();
	}

	public void Init(EquipItemInfo equipItemInfo, Listener listener)
	{
		_listener = listener;
		ItemData = GameRoot.Instance.ItemDataContainer.GetData(equipItemInfo.ItemIndex);
		int num = 1;
		ItemCount = num;
		EquipItemInfo = equipItemInfo;
		GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.ObserverManager.AddObserver(this);
		if (EquipRuneSlotInfoComponent != null)
		{
			EquipRuneSlotInfoComponent.Open(this);
		}
		SetItemInfo();
		SetMoveDirectInfo();
	}

	public override void ReserveCache(UiPreloader preloader)
	{
		base.ReserveCache(preloader);
	}

	public override void Reset()
	{
		base.Reset();
		FactoryBase.SafeReleaseComponent(ref _itemIconComponent);
		FactoryBase.ReleaseAndClearList(_ListPackageRewardItemListComponent);
	}

	public void SetItemInfo()
	{
		if (ItemData == null)
		{
			return;
		}
		SetItemIcon(ItemCount);
		UIHelper.SetLabelText(Label_ItemName, ItemNameBuilder.GetItemName(ItemData));
		UIHelper.EnableGameObject(WidgetRune, ItemData.Type == ItemType.Rune || ItemData.Type == ItemType.PunishmentRune);
		if (ItemData.Type == ItemType.Potion)
		{
			string text = new PotionItemDataBuilder(ItemData, appendLine: false).Build();
			UIHelper.EnableGameObject(Widget_Desc, enable: true);
			UIHelper.SetLabelText(Label_Description, text);
			DescScrollView.ResetPosition();
		}
		else if (ItemData.Type == ItemType.Flask)
		{
			UIHelper.EnableGameObject(Widget_Desc, enable: true);
			ItemData<FlaskItemData> data = GameRoot.Instance.ItemDataContainer.GetData<FlaskItemData>(ItemData.Index);
			if (data != null)
			{
				string text2 = ItemData.Description2.Replace("{1}", data.GetSubData<FlaskItemData>().FlaskExp.ToString());
				UIHelper.SetLabelText(Label_Description, text2);
			}
			else
			{
				UIHelper.SetLabelText(Label_Description, ItemData.Description2);
			}
			DescScrollView.ResetPosition();
		}
		else if (ItemData.Type == ItemType.Rune || ItemData.Type == ItemType.PunishmentRune)
		{
			_ = ItemData;
			string empty = string.Empty;
			string empty2 = string.Empty;
			_ = string.Empty;
			Dictionary<Enum, long> runeStat = RuneHelper.GetRuneStat(ItemData, (EquipItemInfo != null) ? EquipItemInfo.PunishmentRuneOption : null);
			if (runeStat != null)
			{
				List<KeyValuePair<Enum, long>> list = runeStat.ToList();
				bool flag = list.Count == 1;
				UIHelper.EnableGameObject(Widget_SingleStat, flag);
				UIHelper.EnableGameObject(Widget_DualStat, !flag);
				for (int i = 0; i < list.Count; i++)
				{
					empty = LocalizationManager.GetString(list[i].Key);
					empty2 = StatData.GetStatDisplayValue(list[i].Key, list[i].Value);
					if (flag)
					{
						UIHelper.SetLabelText(Label_StatType, empty);
						UIHelper.SetLabelText(Label_StatValue, empty2);
						continue;
					}
					if (i >= DualStatList.Count)
					{
						break;
					}
					UIHelper.SetLabelText(DualStatList[i].Label_DualStatType, empty);
					UIHelper.SetLabelText(DualStatList[i].Label_DualStatValue, empty2);
				}
				UIHelper.EnableGameObject(Widget_Desc, enable: false);
				UIHelper.SetLabelText(Label_Description, string.Empty);
			}
			else
			{
				UIHelper.SetLabelText(Label_StatType, string.Empty);
				UIHelper.SetLabelText(Label_StatValue, string.Empty);
			}
			string runeAddSkill = RuneHelper.GetRuneAddSkill(ItemData, (EquipItemInfo != null) ? EquipItemInfo.PunishmentRuneOption : null);
			if (!String_Extension_Method.IsNullOrEmpty(runeAddSkill))
			{
				UIHelper.EnableGameObject(Widget_AddSkill, enable: true);
				UIHelper.SetLabelText(Label_AddSkill, runeAddSkill);
			}
			else
			{
				UIHelper.EnableGameObject(Widget_AddSkill, enable: false);
			}
			if (EquipRuneSlotInfoComponent != null)
			{
				EquipRuneSlotInfoComponent.Refresh();
			}
			SetupRuneLockButtons();
			SingletonUpdater<Executer>.instance.FrameDelayRun(2, this, delegate
			{
				if (StatScrollView != null)
				{
					StatScrollView.ResetPosition();
				}
			});
		}
		else if (ItemData.Type == ItemType.Package)
		{
			SetupPackageObtainableItemList();
			UIHelper.EnableGameObject(Widget_Desc, enable: false);
			UIHelper.SetLabelText(Label_Description, string.Empty);
		}
		else
		{
			UIHelper.EnableGameObject(Widget_Desc, enable: true);
			UIHelper.SetLabelText(Label_Description, ItemData.Description2);
			DescScrollView.ResetPosition();
		}
		UIHelper.SetLabelText(Label_Description2, ItemData.Description);
		UILabel label_SellPrice = Label_SellPrice;
		int checkEquipSellEventBonus = GlobalAutoBehaviour<DropBonusEventManager>.instance.GetCheckEquipSellEventBonus(ItemData);
		UIHelper.SetLabelText(label_SellPrice, Vespa_Int_Extension_Method.ToCommaString(checkEquipSellEventBonus));
		EnableButtonByItemType(ItemData.Type);
		SetupShortcutButton();
		SetupSellButton();
	}

	private void SetupShortcutButton()
	{
		_ = string.Empty;
		UIHelper.EnableGameObject(Button_Shortcut, enable: true);
		switch (ItemData.MoveTaskType)
		{
		case ClientActivityType.OpenInventory:
			UIHelper.SetLabelTextLocalizeDirect(Label_Shortcut, "ITEMSHORTCUT_MAKING");
			break;
		case ClientActivityType.MoveToNode:
			UIHelper.SetLabelTextLocalizeDirect(Label_Shortcut, "ITEMSHORTCUT_SHORTCUT");
			break;
		default:
			UIHelper.EnableGameObject(Button_Shortcut, enable: false);
			break;
		}
	}

	private void SetupSellButton()
	{
		if (ItemData == null || ItemData.NotForSale)
		{
			UIHelper.EnableGameObject(Button_Sell, enable: false);
			return;
		}
		ItemInfo itemInfo = GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Get(ItemData.Index);
		UIHelper.EnableGameObject(Button_Sell, enable: true);
		UIHelper.EnableButton(Button_Sell, (itemInfo != null && itemInfo.Locked == 0) || (EquipItemInfo != null && EquipItemInfo.Locked == 0));
	}

	public void DisableAllButtons()
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(Buttons); i++)
		{
			ButtonGroup buttonGroup = Buttons[i];
			if (buttonGroup != null)
			{
				UIHelper.EnableGameObject(buttonGroup.Button, enable: false);
			}
		}
	}

	private void EnableButtonByItemType(ItemType itemType)
	{
		ButtonGroup buttonGroup = null;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(Buttons); i++)
		{
			ButtonGroup buttonGroup2 = Buttons[i];
			if (buttonGroup2 != null)
			{
				if (buttonGroup2.Type == itemType)
				{
					buttonGroup = buttonGroup2;
				}
				UIHelper.EnableGameObject(buttonGroup2.Button, enable: false);
			}
		}
		if (buttonGroup != null)
		{
			UIHelper.EnableGameObject(buttonGroup.Button, enable: true);
			UIButton component = buttonGroup.Button.GetComponent<UIButton>();
			switch (buttonGroup.Type)
			{
			case ItemType.Rune:
			{
				ItemInfo itemInfo = GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Get(ItemData.Index);
				UIHelper.EnableButton(component, itemInfo != null && itemInfo.Locked == 0);
				break;
			}
			case ItemType.PunishmentRune:
				UIHelper.EnableButton(component, EquipItemInfo != null && EquipItemInfo.Locked == 0);
				break;
			}
		}
	}

	private void SetupOpenPackageButton()
	{
		int num2;
		if (ItemData.Type == ItemType.Package || ItemData.Type == ItemType.SoulStonePackage)
		{
			long itemCount = ItemCount;
			int num = 1;
			num2 = ((itemCount > num) ? 1 : 0);
		}
		else
		{
			num2 = 0;
		}
		bool flag = (byte)num2 != 0;
		UIHelper.EnableGameObject(Button_OpenPackage, flag);
		if (flag)
		{
			long maxOpenPackageItem = GetMaxOpenPackageItem();
			UIHelper.SetLabelText(Label_OpenPackageCount, LocalizationManager.GetString(Inventory.OPEN_PACKAGE_COUNT, maxOpenPackageItem));
		}
	}

	private void SetupPackageObtainableItemList()
	{
		PackageItemData data = GameRoot.Instance.PackageItemDataContainer.GetData(ItemData.Code);
		if (data == null)
		{
			return;
		}
		RewardData data2 = GameRoot.Instance.RewardDataContainer.GetData(data.RewardIndex1);
		if (data2 == null)
		{
			return;
		}
		int GroupCount;
		List<RewardElementData> rewardItemListWithGroupCount = data2.GetRewardItemListWithGroupCount(out GroupCount);
		if (__Extension_Method_ICollection__.COUNT(rewardItemListWithGroupCount) == 0 && GroupCount == 0)
		{
			return;
		}
		List<RewardElementData> list = new List<RewardElementData>();
		List<RewardElementData> list2 = new List<RewardElementData>();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(rewardItemListWithGroupCount); i++)
		{
			if (rewardItemListWithGroupCount[i].ItemRate == 1000)
			{
				list.Add(rewardItemListWithGroupCount[i]);
			}
			else
			{
				list2.Add(rewardItemListWithGroupCount[i]);
			}
		}
		Vector3 localPosition = PackageItemList.transform.localPosition;
		if (__Extension_Method_ICollection__.COUNT(list) > 0)
		{
			PackageRewardItemListComponent packageRewardItemListComponent = PackageRewardItemListComponent.PrefabAuto.Value.Factory.Create(PackageItemList.transform);
			Vespa_Unity_Transform_Extension_Method.ResetLocalTM(packageRewardItemListComponent.transform);
			packageRewardItemListComponent.transform.localScale = Vector3.one;
			SingletonAuto<LinkedTransform<UIDragScrollView>>.instance.GetAuto(packageRewardItemListComponent.transform).scrollView = PackageItemListScrollView;
			packageRewardItemListComponent.Open(PackageRewardItemListComponent.ERewardType.Fixed, list);
			packageRewardItemListComponent.transform.localPosition = localPosition;
			localPosition.y -= packageRewardItemListComponent.GetComponentHeight();
			_ListPackageRewardItemListComponent.Add(packageRewardItemListComponent);
		}
		if (__Extension_Method_ICollection__.COUNT(list2) > 0)
		{
			PackageRewardItemListComponent packageRewardItemListComponent2 = PackageRewardItemListComponent.PrefabAuto.Value.Factory.Create(PackageItemList.transform);
			Vespa_Unity_Transform_Extension_Method.ResetLocalTM(packageRewardItemListComponent2.transform);
			packageRewardItemListComponent2.transform.localScale = Vector3.one;
			SingletonAuto<LinkedTransform<UIDragScrollView>>.instance.GetAuto(packageRewardItemListComponent2.transform).scrollView = PackageItemListScrollView;
			packageRewardItemListComponent2.Open(PackageRewardItemListComponent.ERewardType.RandomSelect, list2, GroupCount);
			packageRewardItemListComponent2.transform.localPosition = localPosition;
			_ListPackageRewardItemListComponent.Add(packageRewardItemListComponent2);
		}
		PackageItemListScrollView.ResetPosition();
	}

	public void SetItemIcon(long itemCount)
	{
		ItemCount = itemCount;
		if (_itemIconComponent == null)
		{
			_itemIconComponent = ItemIconComponent.PrefabAuto.Factory.Create(ItemParent);
			Vespa_Unity_Transform_Extension_Method.ResetLocalTM(_itemIconComponent.transform);
		}
		_itemIconComponent.Open(ItemData);
		_itemIconComponent.SetItemCount_with_MaxCap(ItemCount);
		if (ItemData.AwakenStar > 0)
		{
			_itemIconComponent.SetStar(ItemData.AwakenStar);
		}
		SetupOpenPackageButton();
	}

	private void SetupRuneLockButtons()
	{
		if (ItemData == null || ItemData.Type == ItemType.Rune || ItemData.Type == ItemType.PunishmentRune)
		{
			switch (ItemData.Type)
			{
			case ItemType.Rune:
			{
				ItemInfo itemInfo = GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Get(ItemData.Index);
				UIHelper.EnableGameObject(Button_Lock, itemInfo != null && itemInfo.Locked == 0);
				UIHelper.EnableGameObject(Button_Unlock, itemInfo != null && itemInfo.Locked != 0);
				break;
			}
			case ItemType.PunishmentRune:
				UIHelper.EnableGameObject(Button_Lock, EquipItemInfo != null && EquipItemInfo.Locked == 0);
				UIHelper.EnableGameObject(Button_Unlock, EquipItemInfo != null && EquipItemInfo.Locked != 0);
				break;
			}
		}
	}

	private void SetMoveDirectInfo()
	{
		if (InvenItemMoveDirect != null)
		{
			InvenItemMoveDirect.Init(ItemData);
		}
	}

	bool IUserObserver.IsGlobal()
	{
		return false;
	}

	void IUserObserver<ItemInfo>.OnChanged(ItemInfo itemInfo)
	{
		if (itemInfo != null && ItemData != null && itemInfo.ItemIndex == ItemData.Index)
		{
			if (ItemData.Type == ItemType.Rune)
			{
				SetupRuneLockButtons();
				EnableButtonByItemType(ItemData.Type);
				SetupSellButton();
			}
			if (itemInfo.Count > 0)
			{
				int count = itemInfo.Count;
				SetItemIcon(count);
			}
			else
			{
				OnClickExitButton();
			}
		}
	}

	public void OnClickToHero()
	{
		if (_listener != null)
		{
			_listener.OnToHero(this);
		}
	}

	public void OnClickUseItem()
	{
		if (_listener != null)
		{
			_listener.OnUseItem(this);
		}
	}

	public void OnClickOpenPackageItem()
	{
		if (_listener != null)
		{
			Listener listener = _listener;
			int num = 1;
			listener.OnOpenItem(this, num);
		}
	}

	public void OnClickOpenPackages()
	{
		if (_listener != null)
		{
			long maxOpenPackageItem = GetMaxOpenPackageItem();
			_listener.OnOpenItem(this, maxOpenPackageItem);
		}
	}

	public void OnClickSell()
	{
		if (_listener != null)
		{
			_listener.OnSell(this);
		}
	}

	public void OnClickShortcut()
	{
		if (!SprkRoomShortcut.Intercept(this, 0) && _listener != null)
		{
			_listener.OnShortcut(this);
		}
	}

	public void OnClickExitButton()
	{
		if (_listener == null || _listener.OnCloseRequest(this))
		{
			Close();
		}
	}

	public override void Close()
	{
		GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.ObserverManager.RemoveObserver(this);
		base.Close();
	}

	public void OnClickLockRuneButton()
	{
		if ((ItemData == null || ItemData.Type == ItemType.Rune || ItemData.Type == ItemType.PunishmentRune) && _listener != null)
		{
			_listener.OnLockRuneItem(this);
		}
	}

	public void OnClickUnlockRuneButton()
	{
		if ((ItemData == null || ItemData.Type == ItemType.Rune || ItemData.Type == ItemType.PunishmentRune) && _listener != null)
		{
			_listener.OnUnlockRuneItem(this);
		}
	}

	public void OnClickBreakRuneButton()
	{
		if ((ItemData == null || ItemData.Type == ItemType.Rune || ItemData.Type == ItemType.PunishmentRune) && _listener != null)
		{
			_listener.OnBreakRuneItem(this);
		}
	}

	public void OnClickRestoreSoulStone()
	{
		if (!SprkRoomShortcut.Intercept(this, 2) && _listener != null)
		{
			_listener.OpenRestoreSoulStone();
			Close();
		}
	}

	public void OnClickItemBreakButton()
	{
		if (ItemData != null && (ItemData == null || ItemData.Breakable) && GameRoot.Instance.ItemBreakDataContainer.GetData(ItemData.Index) != null && _listener != null)
		{
			_listener.OnBreakItem(this, ItemData.Type == ItemType.SoulStone);
		}
	}

	public void OnClickMoveDirect()
	{
		if (!SprkRoomShortcut.Intercept(this, 1) && _listener != null && InvenItemMoveDirect != null)
		{
			_listener.MoveDirect(InvenItemMoveDirect.MoveDirect);
		}
	}

	public override void OnSystemBackButton()
	{
		OnClickExitButton();
	}

	private long GetMaxOpenPackageItem()
	{
		int valueInt = GameRoot.Instance.ConstantDataContainer.GetValueInt("MaxPackageItemOpen");
		long itemCount = ItemCount;
		int num = valueInt;
		return System.Math.Min(num, itemCount);
	}

	public void ExecUseItemEffect(Action onEndEffect)
	{
		if (_itemIconComponent != null)
		{
			_itemIconComponent.PlayUseEffect(onEndEffect);
		}
		if (ItemUseSound != null)
		{
			ItemUseSound.Play();
		}
	}

	void IUserObserver<NGame2.NAccount.UserEquipItemManager.EquipStatInfo>.OnChanged(NGame2.NAccount.UserEquipItemManager.EquipStatInfo changedInfo)
	{
		if (changedInfo != null && EquipItemInfo != null && changedInfo.EquipItemInfo != null && changedInfo.SlotIndex == EquipItemInfo.SlotIndex && changedInfo.EquipItemInfo.ItemIndex == EquipItemInfo.ItemIndex && changedInfo.StatType == NGame2.NAccount.UserEquipItemManager.EquipStatType.Locked && EquipItemInfo.ItemData != null)
		{
			SetupRuneLockButtons();
			EnableButtonByItemType(EquipItemInfo.ItemData.Type);
			SetupSellButton();
		}
	}

	ItemData EquipRuneSlotInfoComponent.IDataProvider.GetItemData()
	{
		return ItemData;
	}
}

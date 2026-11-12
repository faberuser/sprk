using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NDispatchBattle;
using NGame2.NUI.NComponent2;
using NNguiExtension;
using NShared;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class SoulWeaponAbilityTicket : BaseWindow, ItemIconComponent.Listener
{
	public interface IListener
	{
		void OpenConfirmPopup(SoulWeaponAbilityItemDataInfo soulWeaponAbilityItemDataInfo, SoulWeaponAbilityTicketData soulWeaponAbilityTicketData);
	}

	public class SoulWeaponAbilityItemDataInfo
	{
		public ItemData UseItemData { get; set; }

		public EquipItemInfo EquipItemInfo { get; set; }
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Inventory/SoulWeaponAbilityTicket";

	public UILabel Label_Title;

	public EquipTransitionTicket.TransitionItemInfoAndComponent SelectItemInfo;

	public UILabel Label_BeforeItem;

	public UILabel Label_AfterItem;

	public UIComboBox ComboBox;

	public UIScrollGrid EquipItemGrid;

	public UILabel Label_NoneEquipItem;

	public UIButton Button_UseItem;

	public UILabel Label_UseItemLabelDesc;

	public UILabel Label_NonItemLabelDesc;

	public UILabel Label_Button;

	public GameObject RewardGroup;

	public UISimpleGrid RewardIconGrid;

	private List<ItemIconComponent> _rewardItemIconComponentList = new List<ItemIconComponent>();

	private IListener _listener;

	public SoulWeaponAbilityItemDataInfo AbilityItemDataInfo = new SoulWeaponAbilityItemDataInfo();

	private List<EquipItemInfo> _soulWeaponAbilityItemList;

	protected List<ItemIconComponent> _abilityItemList = new List<ItemIconComponent>();

	public SoulWeaponAbilityTicketData SoulWeaponAbilityTicketData { get; private set; }

	public void Open(IListener listener, ItemData itemData)
	{
		if (listener == null)
		{
			VespaDebug.LogError("Listener is null");
			return;
		}
		_listener = listener;
		base.Open();
		Init(itemData);
	}

	private void Init(ItemData itemData)
	{
		if (itemData == null)
		{
			VespaDebug.LogError("itemData is null");
			return;
		}
		SoulWeaponAbilityTicketData data = GameRoot.Instance.SoulWeaponAbilityTicketDataContainer.GetData(itemData.Code);
		if (data != null)
		{
			AbilityItemDataInfo.UseItemData = itemData;
			AbilityItemDataInfo.EquipItemInfo = null;
			SoulWeaponAbilityTicketData = data;
			InitLabelTitle();
			InitLabelDesc();
			InitButtonLabel();
			InitComboBox();
			SetSoulWeaponAbilityTicketStep();
		}
	}

	private void InitComboBox()
	{
		ComboBox.Clear();
		ComboBox.AddItem(0, LocalizationManager.GetString(FilterSelect.FILTER_SELECT_ALL));
		foreach (CreatureTagType item in FilterHelper.GetListOrderCreatureTagType())
		{
			ComboBox.AddItem((int)item, LocalizationManager.GetString(item));
		}
		ComboBox.OnChanged = OnChangeEquipItemList;
		ComboBox.Reposition();
		ComboBox.SelectFirstItem();
	}

	private void SetSoulWeaponAbilityTicketStep()
	{
		SetSelectItemIcon();
		SetIconLabel();
		SetSoulWeaponAbilityItemList();
		SetUseItemButtonEnable();
		SetRewards();
	}

	private void SetSelectItemIcon()
	{
		if (AbilityItemDataInfo.EquipItemInfo == null)
		{
			SelectItemInfo.ClearEquipItemIcons();
			return;
		}
		SelectItemInfo.SetGiveEquipItemIcon(AbilityItemDataInfo.EquipItemInfo, this);
		EquipItemInfo afterEquipItemInfo = GetAfterEquipItemInfo(AbilityItemDataInfo.EquipItemInfo, SoulWeaponAbilityTicketData);
		SelectItemInfo.SetGetEquipItemIcon(afterEquipItemInfo);
	}

	public static EquipItemInfo GetAfterEquipItemInfo(EquipItemInfo equipItemInfo, SoulWeaponAbilityTicketData soulWeaponAbilityTicketData)
	{
		if (equipItemInfo == null)
		{
			return null;
		}
		EquipItemInfo equipItemInfo2 = null;
		if (equipItemInfo.SoulWeaponInfo == null)
		{
			equipItemInfo2 = UIHelper.GetFakeSoulWeaponInfo(equipItemInfo);
		}
		else
		{
			equipItemInfo2 = equipItemInfo.Clone();
			equipItemInfo2.SoulWeaponInfo = equipItemInfo.SoulWeaponInfo.Clone();
			equipItemInfo2.SoulWeaponInfo.ReinforceRatio = equipItemInfo.SoulWeaponInfo.ReinforceRatio;
		}
		switch (soulWeaponAbilityTicketData.ItemType)
		{
		case ESoulWeaponAbilityTicketType.Grade:
			equipItemInfo2.SoulWeaponInfo.Grade = soulWeaponAbilityTicketData.SoulWeaponGrade;
			break;
		case ESoulWeaponAbilityTicketType.Ether:
			equipItemInfo2.SoulWeaponInfo.ReinforceLevel = soulWeaponAbilityTicketData.EtherReinforceLevel;
			equipItemInfo2.SoulWeaponInfo.Exp = 0;
			equipItemInfo2.SoulWeaponInfo.ReinforceRatio = 0;
			break;
		case ESoulWeaponAbilityTicketType.Jumping:
			if (equipItemInfo2.SoulWeaponInfo.Grade < soulWeaponAbilityTicketData.SoulWeaponGrade)
			{
				equipItemInfo2.SoulWeaponInfo.Grade = soulWeaponAbilityTicketData.SoulWeaponGrade;
			}
			if (equipItemInfo2.SoulWeaponInfo.ReinforceLevel < soulWeaponAbilityTicketData.EtherReinforceLevel)
			{
				equipItemInfo2.SoulWeaponInfo.Exp = 0;
				equipItemInfo2.SoulWeaponInfo.ReinforceRatio = 0;
				equipItemInfo2.SoulWeaponInfo.ReinforceLevel = soulWeaponAbilityTicketData.EtherReinforceLevel;
			}
			break;
		}
		return equipItemInfo2;
	}

	private void SetSoulWeaponAbilityItemList()
	{
		ClearTransItemList();
		_soulWeaponAbilityItemList = GlobalAutoBehaviour<AccountManager>.instance.GetSoulWeaponAbilityItemList(SoulWeaponAbilityTicketData, AbilityItemDataInfo.EquipItemInfo);
		switch (SoulWeaponAbilityTicketData.ItemType)
		{
		case ESoulWeaponAbilityTicketType.Ether:
			_soulWeaponAbilityItemList.Sort((EquipItemInfo l, EquipItemInfo r) => l.ItemIndex.CompareTo(r.ItemIndex));
			break;
		case ESoulWeaponAbilityTicketType.Grade:
		case ESoulWeaponAbilityTicketType.Jumping:
			_soulWeaponAbilityItemList.Sort(delegate(EquipItemInfo l, EquipItemInfo r)
			{
				if (l.ItemIndex != r.ItemIndex)
				{
					return l.ItemIndex.CompareTo(r.ItemIndex);
				}
				int num = (l.IsSoulWeapon() ? 1 : (-1));
				int num2 = (r.IsSoulWeapon() ? 1 : (-1));
				return (num != num2) ? num.CompareTo(num2) : r.Star.CompareTo(l.Star);
			});
			break;
		}
		_soulWeaponAbilityItemList.ForEach(delegate(EquipItemInfo equipItemInfo)
		{
			if (SelectFilter.CheckSameTagType(equipItemInfo.EquipItemData, ComboBox.CurrentKey))
			{
				ItemIconComponent itemIconComponent = UIHelper.AddGridItem(EquipItemGrid, ItemIconComponent.PrefabAuto);
				itemIconComponent.Open(equipItemInfo, this);
				if (AbilityItemDataInfo.EquipItemInfo != null || !CheckEnableTransEquipItem(equipItemInfo))
				{
					itemIconComponent.SetIconGrayScale(grayScale: true);
				}
				_abilityItemList.Add(itemIconComponent);
			}
		});
		bool flag = __Extension_Method_ICollection__.COUNT(_abilityItemList) > 0;
		if (flag)
		{
			UIHelper.RepositionGrid(EquipItemGrid);
		}
		UIHelper.EnableGameObject(Label_NoneEquipItem, !flag);
	}

	private void ClearTransItemList()
	{
		FactoryBase.ReleaseAndClearList(_abilityItemList);
		_abilityItemList.Clear();
		if (_soulWeaponAbilityItemList != null)
		{
			_soulWeaponAbilityItemList.Clear();
		}
	}

	private bool CheckEnableTransEquipItem(EquipItemInfo equipItemInfo)
	{
		if (IsLockedEquipItem(equipItemInfo))
		{
			return false;
		}
		if (IsStorageEquipItem(equipItemInfo))
		{
			return false;
		}
		if (IsDispatchedHeroEquipItem(equipItemInfo))
		{
			return false;
		}
		return true;
	}

	private string GetUnableTransEquipItemMsg(EquipItemInfo equipItemInfo)
	{
		if (IsLockedEquipItem(equipItemInfo))
		{
			return LocalizationManager.GetStringDirect("TRANSITION_AWAKEN_NOT_ENABLE_LOCK_ITEM_MSG");
		}
		if (IsStorageEquipItem(equipItemInfo))
		{
			return LocalizationManager.GetStringDirect("TRANSITION_AWAKEN_NOT_ENABLE_CHEST_ITEM_MSG");
		}
		if (IsDispatchedHeroEquipItem(equipItemInfo))
		{
			return LocalizationManager.GetStringDirect("TRANSITION_AWAKEN_NOT_ENABLE_DISPATCH_ITEM_MSG");
		}
		return string.Empty;
	}

	private bool IsLockedEquipItem(EquipItemInfo equipItemInfo)
	{
		return equipItemInfo.Locked != 0;
	}

	private bool IsStorageEquipItem(EquipItemInfo equipItemInfo)
	{
		return equipItemInfo.GetInventoryType() == InventoryType.Chest;
	}

	private bool IsDispatchedHeroEquipItem(EquipItemInfo equipItemInfo)
	{
		return DispatchHelper.IsDispatchedHero(equipItemInfo.EquippedHeroInfo);
	}

	private void InitLabelTitle()
	{
		UIHelper.SetLabelText(Label_Title, LocalizationManager.GetStringDirect(AbilityItemDataInfo.UseItemData.Name));
	}

	private void InitLabelDesc()
	{
		UIHelper.EnableGameObject(Label_UseItemLabelDesc, SoulWeaponAbilityTicketData.ItemType != ESoulWeaponAbilityTicketType.Grade);
		UIHelper.EnableGameObject(Label_NonItemLabelDesc, SoulWeaponAbilityTicketData.ItemType == ESoulWeaponAbilityTicketType.Grade);
		UILabel label = ((SoulWeaponAbilityTicketData.ItemType == ESoulWeaponAbilityTicketType.Grade) ? Label_NonItemLabelDesc : Label_UseItemLabelDesc);
		switch (SoulWeaponAbilityTicketData.ItemType)
		{
		case ESoulWeaponAbilityTicketType.Grade:
			UIHelper.SetLabelText(label, LocalizationManager.GetStringDirect("SOULWEAPON_UPGRADE_TICKET_DESC"));
			break;
		case ESoulWeaponAbilityTicketType.Ether:
			UIHelper.SetLabelText(label, LocalizationManager.GetStringDirect("SOULWEAPON_REINFORCE_TICKET_DESC"));
			break;
		case ESoulWeaponAbilityTicketType.Jumping:
			UIHelper.SetLabelText(label, LocalizationManager.GetStringDirect("SOULWEAPON_JUMPING_TICKET_DESC"));
			break;
		}
	}

	private void InitButtonLabel()
	{
		UIHelper.SetLabelText(Label_Button, LocalizationManager.GetStringDirect("RAID_READY_COMPLETE"));
	}

	private void SetIconLabel()
	{
		bool flag = SoulWeaponAbilityTicketData.ItemType != ESoulWeaponAbilityTicketType.Grade;
		UIHelper.EnableGameObject(Label_BeforeItem, flag);
		UIHelper.EnableGameObject(Label_AfterItem, flag);
		if (flag)
		{
			SetSoulWeaponIconLabel();
		}
	}

	private void SetSoulWeaponIconLabel()
	{
		UIHelper.SetLabelText(Label_BeforeItem, GetSoulWeaponInfoLabelString(AbilityItemDataInfo.EquipItemInfo));
		UIHelper.SetLabelText(Label_AfterItem, GetSoulWeaponInfoLabelString(AbilityItemDataInfo.EquipItemInfo, isAfterSoulWeaponInfo: true));
	}

	private string GetSoulWeaponInfoLabelString(EquipItemInfo equipItemInfo, bool isAfterSoulWeaponInfo = false)
	{
		return LocalizationManager.GetStringDirect("TRANSITION_SOULWEAPON_INFO_STAT", GetSoulWeaponCurrentExp(equipItemInfo, isAfterSoulWeaponInfo), GetSoulWeaponRequireExpLabelString(equipItemInfo, isAfterSoulWeaponInfo), GetEtherSuccessRateLabelString(equipItemInfo, isAfterSoulWeaponInfo));
	}

	private int GetSoulWeaponRequireExpLabelString(EquipItemInfo equipItemInfo, bool isAfterSoulWeaponInfo = false)
	{
		int result = 0;
		if (isAfterSoulWeaponInfo)
		{
			SoulWeaponReinforceData data = GameRoot.Instance.SoulWeaponReinforceDataContainer.GetData(SoulWeaponAbilityTicketData.EtherReinforceLevel);
			if (data != null)
			{
				result = data.RequiredExp;
			}
			return result;
		}
		if (equipItemInfo != null && equipItemInfo.IsSoulWeapon())
		{
			SoulWeaponReinforceData soulWeaponReinforceData = equipItemInfo.SoulWeaponInfo.GetSoulWeaponReinforceData();
			if (soulWeaponReinforceData != null)
			{
				result = soulWeaponReinforceData.RequiredExp;
			}
		}
		return result;
	}

	private int GetSoulWeaponCurrentExp(EquipItemInfo equipItemInfo, bool isAfterSoulWeaponInfo = false)
	{
		int result = 0;
		if (equipItemInfo != null && equipItemInfo.IsSoulWeapon())
		{
			if (isAfterSoulWeaponInfo && equipItemInfo.SoulWeaponInfo.ReinforceLevel < SoulWeaponAbilityTicketData.EtherReinforceLevel)
			{
				return result;
			}
			result = equipItemInfo.SoulWeaponInfo.Exp;
		}
		return result;
	}

	private float GetEtherSuccessRateLabelString(EquipItemInfo equipItemInfo, bool isAfterSoulWeaponInfo = false)
	{
		float num = 0f;
		if (equipItemInfo != null && equipItemInfo.IsSoulWeapon())
		{
			if (isAfterSoulWeaponInfo && equipItemInfo.SoulWeaponInfo.ReinforceLevel < SoulWeaponAbilityTicketData.EtherReinforceLevel)
			{
				return num;
			}
			if (equipItemInfo.SoulWeaponInfo.ReinforceRatio != 0)
			{
				num = (float)System.Math.Ceiling((float)equipItemInfo.SoulWeaponInfo.ReinforceRatio / 10f) / 100f;
			}
			if (num > 100f)
			{
				num = 100f;
			}
		}
		return num;
	}

	private void SetUseItemButtonEnable()
	{
		UIHelper.EnableButton(Button_UseItem, AbilityItemDataInfo.EquipItemInfo != null);
	}

	private void SetRewards()
	{
		FactoryBase.ReleaseAndClearList(_rewardItemIconComponentList);
		if (AbilityItemDataInfo.EquipItemInfo == null || AbilityItemDataInfo.EquipItemInfo.SoulWeaponInfo == null)
		{
			UIHelper.EnableGameObject(RewardGroup, enable: false);
			return;
		}
		ESoulWeaponAbilityTicketType eSoulWeaponAbilityTicketType = SoulWeaponAbilityTicketData.ItemType;
		if (eSoulWeaponAbilityTicketType == ESoulWeaponAbilityTicketType.Jumping)
		{
			if (AbilityItemDataInfo.EquipItemInfo.SoulWeaponInfo.Grade >= SoulWeaponAbilityTicketData.SoulWeaponGrade)
			{
				eSoulWeaponAbilityTicketType = ESoulWeaponAbilityTicketType.Ether;
			}
			else if (AbilityItemDataInfo.EquipItemInfo.SoulWeaponInfo.ReinforceLevel >= SoulWeaponAbilityTicketData.EtherReinforceLevel)
			{
				eSoulWeaponAbilityTicketType = ESoulWeaponAbilityTicketType.Grade;
			}
		}
		SoulWeaponAbilityRewardData soulWeaponAbilityRewardData = AbilityItemDataInfo.EquipItemInfo.SoulWeaponInfo.GetSoulWeaponAbilityRewardData(eSoulWeaponAbilityTicketType);
		if (soulWeaponAbilityRewardData == null)
		{
			UIHelper.EnableGameObject(RewardGroup, enable: false);
			return;
		}
		IList<string> itemCodeList = soulWeaponAbilityRewardData.GetItemCodeList();
		if (__Extension_Method_ICollection__.IsNullOrEmpty(itemCodeList))
		{
			UIHelper.EnableGameObject(RewardGroup, enable: false);
			return;
		}
		UIHelper.EnableGameObject(RewardGroup, enable: true);
		ItemData itemData = null;
		ItemIconComponent itemIconComponent = null;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(itemCodeList); i++)
		{
			itemData = GameRoot.Instance.ItemDataContainer.GetDataByCode(itemCodeList[i]);
			if (itemData != null)
			{
				itemIconComponent = UIHelper.AddGridItem(RewardIconGrid, ItemIconComponent.PrefabAuto);
				if (!(itemIconComponent == null))
				{
					int itemCount = soulWeaponAbilityRewardData.GetItemCount(i);
					long count = itemCount;
					itemIconComponent.Open(itemData);
					itemIconComponent.SetItemCount(count);
					_rewardItemIconComponentList.Add(itemIconComponent);
				}
			}
		}
		UIHelper.RepositionGrid(RewardIconGrid);
	}

	private void OnChangeEquipItemList(int value)
	{
		SetSoulWeaponAbilityItemList();
	}

	public void OpenConfirmPopup()
	{
		if (_listener != null)
		{
			_listener.OpenConfirmPopup(AbilityItemDataInfo, SoulWeaponAbilityTicketData);
		}
	}

	public override void Close()
	{
		base.Close();
		ResetSoulWeaponAbiltiyTicket();
	}

	private void ResetSoulWeaponAbiltiyTicket()
	{
		FactoryBase.ReleaseAndClearList(_rewardItemIconComponentList);
		ComboBox.Clear();
		SelectItemInfo.ClearEquipItemIcons();
		ClearTransItemList();
	}

	void ItemIconComponent.Listener.OnClick(ItemIconComponent itemIconComponent)
	{
		if (itemIconComponent.EquipItemInfo == null)
		{
			return;
		}
		if (AbilityItemDataInfo.EquipItemInfo != null)
		{
			if (AbilityItemDataInfo.EquipItemInfo.SlotIndex == itemIconComponent.EquipItemInfo.SlotIndex)
			{
				AbilityItemDataInfo.EquipItemInfo = null;
				SetSoulWeaponAbilityTicketStep();
			}
			return;
		}
		string empty = string.Empty;
		empty = GetUnableTransEquipItemMsg(itemIconComponent.EquipItemInfo);
		if (!String_Extension_Method.IsNullOrEmpty(empty))
		{
			CenterMessage.Open(empty);
			return;
		}
		AbilityItemDataInfo.EquipItemInfo = itemIconComponent.EquipItemInfo;
		SetSoulWeaponAbilityTicketStep();
	}
}

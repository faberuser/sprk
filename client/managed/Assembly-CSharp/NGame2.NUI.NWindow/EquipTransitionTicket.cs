using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NComponent2.NItemIconSub;
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

public class EquipTransitionTicket : BaseWindow, ItemIconComponent.Listener
{
	public interface IListener
	{
		void OpenTransitionConfirmPopup(TransitionItemDataInfo transItemDataInfo, EquipTransitionType transitionType);
	}

	public enum EquipTransitionStep
	{
		SelectGiveItem,
		SelectGetItem,
		SelectCompleted
	}

	[Serializable]
	public class TransitionItemInfoAndComponent
	{
		public GameObject ItemInfoComponent;

		public Transform GiveIconPos;

		public Transform GetIconPos;

		private ItemIconComponent _giveItemIcon;

		private ItemIconComponent _getItemIcon;

		public void SetGiveEquipItemIcon(EquipItemInfo equipItemInfo, ItemIconComponent.Listener owner = null)
		{
			if (equipItemInfo != null && (!(_giveItemIcon != null) || _giveItemIcon.EquipItemInfo.SlotIndex != equipItemInfo.SlotIndex))
			{
				_giveItemIcon = ItemIconComponent.PrefabAuto.Factory.Create(GiveIconPos);
				_giveItemIcon.Open(equipItemInfo, owner);
				_giveItemIcon.ResetPosition();
			}
		}

		public void SetGetEquipItemIcon(EquipItemInfo equipItemInfo, ItemIconComponent.Listener owner = null)
		{
			if (equipItemInfo != null && (!(_getItemIcon != null) || _getItemIcon.EquipItemInfo.SlotIndex != equipItemInfo.SlotIndex))
			{
				_getItemIcon = ItemIconComponent.PrefabAuto.Factory.Create(GetIconPos);
				_getItemIcon.Open(equipItemInfo, owner);
				_getItemIcon.ResetPosition();
			}
		}

		public void SetGetEquipItemIconByCustom(EquipItemInfo equipItemInfo, int grade = 0, int reinforceLevel = 0, ItemIconComponent.Listener owner = null)
		{
			if (equipItemInfo != null && !(_getItemIcon != null))
			{
				_getItemIcon = ItemIconComponent.PrefabAuto.Factory.Create(GetIconPos);
				_getItemIcon.Open(equipItemInfo, owner);
				_getItemIcon.SetSoulWeaponGrade(grade, reinforceLevel);
				_getItemIcon.ResetPosition();
			}
		}

		public void SetGiveEquipItemIconSelected(bool selected)
		{
			_giveItemIcon.SetSelected(selected, SelectSubComponent.ESelectType.Select_BorderBook);
		}

		public void SetGetEquipItemIconSelected(bool selected)
		{
			_getItemIcon.SetSelected(selected, SelectSubComponent.ESelectType.Select_BorderBook);
		}

		public void ClearEquipItemIcons()
		{
			ClearGiveEquipItemIcon();
			ClearGetEquipItemIcon();
		}

		private void ClearGiveEquipItemIcon()
		{
			if (!(_giveItemIcon == null))
			{
				FactoryBase.SafeReleaseComponent(ref _giveItemIcon);
				_giveItemIcon = null;
			}
		}

		public void ClearGetEquipItemIcon()
		{
			if (!(_getItemIcon == null))
			{
				FactoryBase.SafeReleaseComponent(ref _getItemIcon);
				_getItemIcon = null;
			}
		}
	}

	public class TransitionItemDataInfo
	{
		public ItemData UseItemData { get; set; }

		public EquipItemInfo GiveEquipItemInfo { get; set; }

		public EquipItemInfo GetEquipItemInfo { get; set; }
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Inventory/EquipTransitionTicket";

	public UILabel Label_Title;

	public UILabel Label_TransitionStep;

	public TransitionItemInfoAndComponent TransItemInfo;

	public UILabel Label_GiveItem;

	public UILabel Label_GetItem;

	public UIComboBox ComboBox;

	public UIScrollGrid EquipItemGrid;

	public UILabel Label_NoneEquipItem;

	public UIButton Button_Transition;

	public UILabel Label_TransitionDesc;

	public UILabel Label_Button;

	private IListener _listener;

	public TransitionItemDataInfo TransItemDataInfo = new TransitionItemDataInfo();

	private List<EquipItemInfo> _equipTransItemList;

	private string _equipTransitionTypeStr;

	private EquipTransitionTicketData _transitionTicketData;

	protected List<ItemIconComponent> _transItemList = new List<ItemIconComponent>();

	public EquipTransitionTicketData EquipTransTicketData { get; private set; }

	public EquipTransitionStep CurrentTransitionStep { get; private set; }

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
		_transitionTicketData = GameRoot.Instance.EquipTransitionTicketDataContainer.GetData(itemData.Index);
		if (_transitionTicketData != null && _transitionTicketData.SelectAbleDetailIndex.Length != 0)
		{
			TransItemDataInfo.UseItemData = itemData;
			TransItemDataInfo.GiveEquipItemInfo = null;
			TransItemDataInfo.GetEquipItemInfo = null;
			_equipTransitionTypeStr = _transitionTicketData.Type.ToString().ToUpper();
			EquipTransTicketData = _transitionTicketData;
			InitLabelTitle();
			InitLabelDesc();
			InitButtonLabel();
			InitComboBox();
			SetEquipTransitionStep(EquipTransitionStep.SelectGiveItem);
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
		ComboBox.OnChanged = OnChangeTransitionEquipItemList;
		ComboBox.Reposition();
		ComboBox.SelectFirstItem();
	}

	private void SetEquipTransitionStep(EquipTransitionStep equipTransitionStep)
	{
		CurrentTransitionStep = equipTransitionStep;
		SetSelectItemIcon();
		SetIconLabel();
		SetTransitionStepLabel();
		SetTransItemList();
		SetTransitionButtonEnable();
	}

	private void SetSelectItemIcon()
	{
		switch (CurrentTransitionStep)
		{
		case EquipTransitionStep.SelectGiveItem:
			TransItemInfo.ClearEquipItemIcons();
			break;
		case EquipTransitionStep.SelectGetItem:
			TransItemInfo.ClearGetEquipItemIcon();
			TransItemInfo.SetGiveEquipItemIcon(TransItemDataInfo.GiveEquipItemInfo, this);
			break;
		case EquipTransitionStep.SelectCompleted:
			TransItemInfo.SetGetEquipItemIcon(TransItemDataInfo.GetEquipItemInfo, this);
			break;
		}
	}

	private void SetTransItemList()
	{
		if (_transitionTicketData.Type == EquipTransitionType.Awaken)
		{
			SetAwakenTransItemList();
		}
		else if (_transitionTicketData.Type == EquipTransitionType.SoulWeapon)
		{
			SetSoulWeaponTransItemList();
		}
	}

	private void SetAwakenTransItemList()
	{
		ClearTransItemList();
		_equipTransItemList = GlobalAutoBehaviour<AccountManager>.instance.GetCanEquipAwakenTransitionItemList(EquipTransTicketData, TransItemDataInfo.GiveEquipItemInfo, TransItemDataInfo.GetEquipItemInfo, CheckSelectExceptItemGroup());
		_equipTransItemList.Sort((EquipItemInfo l, EquipItemInfo r) => l.ItemIndex.CompareTo(r.ItemIndex));
		_equipTransItemList.ForEach(delegate(EquipItemInfo equipItemInfo)
		{
			if (SelectFilter.CheckSameTagType(equipItemInfo.EquipItemData, ComboBox.CurrentKey))
			{
				ItemIconComponent itemIconComponent = UIHelper.AddGridItem(EquipItemGrid, ItemIconComponent.PrefabAuto);
				itemIconComponent.Open(equipItemInfo, this);
				if (CurrentTransitionStep == EquipTransitionStep.SelectCompleted || !CheckEnableTransEquipItem(equipItemInfo))
				{
					itemIconComponent.SetIconGrayScale(grayScale: true);
				}
				_transItemList.Add(itemIconComponent);
			}
		});
		UIHelper.RepositionGrid(EquipItemGrid);
		UIHelper.EnableGameObject(Label_NoneEquipItem, _transItemList.Count == 0);
	}

	private void SetSoulWeaponTransItemList()
	{
		ClearTransItemList();
		_equipTransItemList = GlobalAutoBehaviour<AccountManager>.instance.GetCanEquipSoulWeaponTransitionItemList(EquipTransTicketData, TransItemDataInfo.GiveEquipItemInfo, TransItemDataInfo.GetEquipItemInfo, CheckSelectExceptItemGroup());
		_equipTransItemList.Sort((EquipItemInfo l, EquipItemInfo r) => l.ItemIndex.CompareTo(r.ItemIndex));
		_equipTransItemList.ForEach(delegate(EquipItemInfo equipItemInfo)
		{
			if (SelectFilter.CheckSameTagType(equipItemInfo.EquipItemData, ComboBox.CurrentKey))
			{
				ItemIconComponent itemIconComponent = UIHelper.AddGridItem(EquipItemGrid, ItemIconComponent.PrefabAuto);
				itemIconComponent.Open(equipItemInfo, this);
				if (CurrentTransitionStep == EquipTransitionStep.SelectCompleted || !CheckEnableTransEquipItem(equipItemInfo))
				{
					itemIconComponent.SetIconGrayScale(grayScale: true);
				}
				_transItemList.Add(itemIconComponent);
			}
		});
		bool flag = __Extension_Method_ICollection__.COUNT(_transItemList) > 0;
		if (flag)
		{
			UIHelper.RepositionGrid(EquipItemGrid);
		}
		UIHelper.EnableGameObject(Label_NoneEquipItem, !flag);
	}

	private void ClearTransItemList()
	{
		FactoryBase.ReleaseAndClearList(_transItemList);
		_transItemList.Clear();
		if (_equipTransItemList != null)
		{
			_equipTransItemList.Clear();
		}
	}

	private bool CheckSelectExceptItemGroup()
	{
		if (TransItemDataInfo.GiveEquipItemInfo == null)
		{
			return false;
		}
		if (__Extension_Method_ICollection__.IsNullOrEmpty(EquipTransTicketData.ExceptItemGroup1))
		{
			return false;
		}
		return TypeIterator.Contains(EquipTransTicketData.ExceptItemGroup1, TransItemDataInfo.GiveEquipItemInfo.ItemIndex);
	}

	private bool CheckEnableTransEquipItem(EquipItemInfo equipItemInfo)
	{
		if (equipItemInfo.CLI_IsLocked())
		{
			return false;
		}
		if (equipItemInfo.CLI_IsSavedInChest())
		{
			return false;
		}
		if (equipItemInfo.CLI_IsDispatchedHeroItem())
		{
			return false;
		}
		if (equipItemInfo.CLI_IsLimitBreaked())
		{
			return false;
		}
		return true;
	}

	private string GetUnableTransEquipItemMsg(EquipItemInfo equipItemInfo)
	{
		if (equipItemInfo.CLI_IsLocked())
		{
			return LocalizationManager.GetStringDirect("TRANSITION_AWAKEN_NOT_ENABLE_LOCK_ITEM_MSG");
		}
		if (equipItemInfo.CLI_IsSavedInChest())
		{
			return LocalizationManager.GetStringDirect("TRANSITION_AWAKEN_NOT_ENABLE_CHEST_ITEM_MSG");
		}
		if (equipItemInfo.CLI_IsDispatchedHeroItem())
		{
			return LocalizationManager.GetStringDirect("TRANSITION_AWAKEN_NOT_ENABLE_DISPATCH_ITEM_MSG");
		}
		if (equipItemInfo.CLI_IsLimitBreaked())
		{
			return LocalizationManager.GetStringDirect("TRANSITION_AWAKEN_NOT_ENABLE_TRANSCENDENCE_ITEM_MSG");
		}
		return string.Empty;
	}

	private void InitLabelTitle()
	{
		UIHelper.SetLabelText(Label_Title, LocalizationManager.GetStringDirect(TransItemDataInfo.UseItemData.Name));
	}

	private void InitLabelDesc()
	{
		if (_transitionTicketData.Type == EquipTransitionType.Awaken)
		{
			UIHelper.SetLabelText(Label_TransitionDesc, LocalizationManager.GetStringDirect("TRANSITION_AWAKEN_INFO_DESC"));
		}
		else if (_transitionTicketData.Type == EquipTransitionType.SoulWeapon)
		{
			UIHelper.SetLabelText(Label_TransitionDesc, LocalizationManager.GetStringDirect("TRANSITION_SOULWEAPON_INFO_DESC"));
		}
	}

	private void InitButtonLabel()
	{
		if (_transitionTicketData.Type == EquipTransitionType.Awaken)
		{
			UIHelper.SetLabelText(Label_Button, LocalizationManager.GetStringDirect("TRANSITION_AWAKEN_BUTTON"));
		}
		else if (_transitionTicketData.Type == EquipTransitionType.SoulWeapon)
		{
			UIHelper.SetLabelText(Label_Button, LocalizationManager.GetStringDirect("TRANSITION_SOULWEAPON_READY_BUTTON"));
		}
	}

	private void SetTransitionStepLabel()
	{
		string empty = string.Empty;
		empty = GetTransitionStepLabelString(CurrentTransitionStep);
		if (!String_Extension_Method.IsNullOrEmpty(empty))
		{
			UIHelper.SetLabelText(Label_TransitionStep, empty);
		}
	}

	private string GetTransitionStepLabelString(EquipTransitionStep step)
	{
		bool flag = _transitionTicketData.Type == EquipTransitionType.Awaken;
		string key = "";
		switch (step)
		{
		case EquipTransitionStep.SelectGiveItem:
			key = (flag ? "TRANSITION_AWAKEN_INFO_SELECT_BASE_EQUIP" : "TRANSITION_SOULWEAPON_INFO_SELECT_BASE");
			break;
		case EquipTransitionStep.SelectGetItem:
			key = (flag ? "TRANSITION_AWAKEN_INFO_SELECT_RECIPIENT_EQUIP" : "TRANSITION_SOULWEAPON_INFO_SELECT_RECIPIENT");
			break;
		case EquipTransitionStep.SelectCompleted:
			key = (flag ? "TRANSITION_AWAKEN_INFO_READY_TRANSITION" : "TRANSITION_SOULWEAPON_INFO_READY");
			break;
		}
		return LocalizationManager.GetStringDirect(key);
	}

	private void SetIconLabel()
	{
		if (_transitionTicketData.Type == EquipTransitionType.Awaken)
		{
			SetFailBonusLabel();
		}
		else if (_transitionTicketData.Type == EquipTransitionType.SoulWeapon)
		{
			SetSoulWeaponIconLabel();
		}
	}

	private void SetFailBonusLabel()
	{
		UIHelper.SetLabelText(Label_GiveItem, GetLabelFailBonusStr(TransItemDataInfo.GiveEquipItemInfo));
		UIHelper.SetLabelText(Label_GetItem, GetLabelFailBonusStr(TransItemDataInfo.GetEquipItemInfo));
	}

	private string GetLabelFailBonusStr(EquipItemInfo equipItemInfo)
	{
		int num = 0;
		if (equipItemInfo != null)
		{
			num = equipItemInfo.UpgradeStarFailBonus;
		}
		string stringDirect = LocalizationManager.GetStringDirect("COMMON_EQUIP_FAIL_BONUS_NOTICE");
		return $"{stringDirect}\n+{num}%";
	}

	private void SetSoulWeaponIconLabel()
	{
		UIHelper.SetLabelText(Label_GiveItem, GetSoulWeaponInfoLabelString(TransItemDataInfo.GiveEquipItemInfo));
		UIHelper.SetLabelText(Label_GetItem, GetSoulWeaponInfoLabelString(TransItemDataInfo.GetEquipItemInfo));
	}

	private string GetSoulWeaponInfoLabelString(EquipItemInfo equipItemInfo)
	{
		return LocalizationManager.GetStringDirect("TRANSITION_SOULWEAPON_INFO_STAT", GetSoulWeaponCurrentExp(equipItemInfo), GetSoulWeaponRequireExpLabelString(equipItemInfo), GetEtherSuccessRateLabelString(equipItemInfo));
	}

	private int GetSoulWeaponRequireExpLabelString(EquipItemInfo equipItemInfo)
	{
		int result = 0;
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

	private int GetSoulWeaponCurrentExp(EquipItemInfo equipItemInfo)
	{
		int result = 0;
		if (equipItemInfo != null && equipItemInfo.IsSoulWeapon())
		{
			result = equipItemInfo.SoulWeaponInfo.Exp;
		}
		return result;
	}

	private float GetEtherSuccessRateLabelString(EquipItemInfo equipItemInfo)
	{
		float num = 0f;
		if (equipItemInfo != null && equipItemInfo.IsSoulWeapon())
		{
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

	private float Ceiling(float value, int point)
	{
		float num = value * Mathf.Pow(10f, point);
		float num2 = (float)decimal.Ceiling((decimal)num);
		num = Mathf.Pow(10f, point);
		return num2 / num;
	}

	private void SetTransitionButtonEnable()
	{
		UIHelper.EnableButton(Button_Transition, CurrentTransitionStep == EquipTransitionStep.SelectCompleted);
	}

	private void OnChangeTransitionEquipItemList(int value)
	{
		SetTransItemList();
	}

	public void OpenTransitionConfirmPopup()
	{
		if (_listener != null)
		{
			_listener.OpenTransitionConfirmPopup(TransItemDataInfo, _transitionTicketData.Type);
		}
	}

	public override void Close()
	{
		base.Close();
		ResetEquipTransitionTicket();
	}

	private void ResetEquipTransitionTicket()
	{
		ComboBox.Clear();
		TransItemInfo.ClearEquipItemIcons();
		ClearTransItemList();
	}

	void ItemIconComponent.Listener.OnClick(ItemIconComponent itemIconComponent)
	{
		if (itemIconComponent.EquipItemInfo == null)
		{
			return;
		}
		if (TransItemDataInfo.GiveEquipItemInfo != null && TransItemDataInfo.GiveEquipItemInfo.SlotIndex == itemIconComponent.EquipItemInfo.SlotIndex)
		{
			TransItemDataInfo.GiveEquipItemInfo = null;
			TransItemDataInfo.GetEquipItemInfo = null;
			SetEquipTransitionStep(EquipTransitionStep.SelectGiveItem);
			return;
		}
		if (TransItemDataInfo.GetEquipItemInfo != null && TransItemDataInfo.GetEquipItemInfo.SlotIndex == itemIconComponent.EquipItemInfo.SlotIndex)
		{
			TransItemDataInfo.GetEquipItemInfo = null;
			SetEquipTransitionStep(EquipTransitionStep.SelectGetItem);
			return;
		}
		string empty = string.Empty;
		if (CurrentTransitionStep != EquipTransitionStep.SelectCompleted)
		{
			empty = GetUnableTransEquipItemMsg(itemIconComponent.EquipItemInfo);
			if (!String_Extension_Method.IsNullOrEmpty(empty))
			{
				CenterMessage.Open(empty);
				return;
			}
		}
		switch (CurrentTransitionStep)
		{
		case EquipTransitionStep.SelectGiveItem:
			TransItemDataInfo.GiveEquipItemInfo = itemIconComponent.EquipItemInfo;
			SetEquipTransitionStep(EquipTransitionStep.SelectGetItem);
			break;
		case EquipTransitionStep.SelectGetItem:
			TransItemDataInfo.GetEquipItemInfo = itemIconComponent.EquipItemInfo;
			SetEquipTransitionStep(EquipTransitionStep.SelectCompleted);
			break;
		case EquipTransitionStep.SelectCompleted:
			break;
		}
	}
}

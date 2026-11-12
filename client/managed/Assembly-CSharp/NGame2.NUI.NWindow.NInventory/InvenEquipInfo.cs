using NGame2.NAccount;
using NGame2.NContents;
using NGame2.NDispatchBattle;
using NGame2.NUI.NManager.NLobby;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow.NInventory;

[UIEvent(AllowDuringPatchDownloading = true)]
public class InvenEquipInfo : HeroEquipInfo
{
	public new interface Listener : HeroEquipInfo.Listener
	{
		void OnMove(InvenEquipInfo component);

		void OnSell(InvenEquipInfo component);

		void OnBreak(InvenEquipInfo component);

		void OnKeep(InvenEquipInfo component);

		void OnBring(InvenEquipInfo component);

		void OpenSoulWeaponLibearationWindow(EquipItemInfo equipItemInfo);

		void OpenSoulWeaponManageWindow(EquipItemInfo equipItemInfo);

		void CancelSoulWeapon(EquipItemInfo equipItemInfo);

		void ClickHeroSpecialEquipInfo(InvenEquipInfoUIContainer.WindowPosType windowPosType, EquipItemInfo equipItemInfo);

		void TechnoMagicIdentified(EquipItemInfo equipItemInfo);
	}

	public interface IInvenModeListener
	{
		void OnClickClose();
	}

	public new const string PrefabPath = "Resources/Prefab2/Window/Inventory/InvenEquipInfo";

	[Space(10f)]
	[Header("InvenEquipInfo")]
	public GameObject BackgroundObject;

	public UIButton MoveButton;

	public UIButton SellButton;

	public UIButton BreakButton;

	public UILabel LabelEquip;

	public UILabel LabelUnEquip;

	public UIButton SoulWeaponManageButton;

	public UIButton SoulWeaponLiberationButton;

	public UIButton SoulWeaponCancelButton;

	public UIButton SpecialEquipInfoButton;

	[Space(10f)]
	[Header("TechnoMagic")]
	public UIButton TechnoMagicIdentifiedButton;

	private bool _isInHeroEquip;

	private Listener _listener;

	private IInvenModeListener _uiEventListener;

	private bool _inventoryMode;

	private bool _isBreakable
	{
		get
		{
			if (!EquipItemInfo.IsBreakable)
			{
				return false;
			}
			return true;
		}
	}

	private bool _isSellable
	{
		get
		{
			if (!EquipItemInfo.IsSellable)
			{
				return false;
			}
			if (EquipItemInfo.IsTechnoMagic)
			{
				return false;
			}
			return true;
		}
	}

	private bool _isEquipable
	{
		get
		{
			if (EquipItemData != null && EquipItemData.PartType == EquipPartType.EnhanceStone)
			{
				return false;
			}
			return EquipItemInfo.IsIdentified;
		}
	}

	private bool _isKeepable
	{
		get
		{
			if (!IsInventoryMode())
			{
				return false;
			}
			if (!EquipItemInfo.IsKeepable)
			{
				return false;
			}
			return true;
		}
	}

	private bool CheckSpecialEquipInfoButtonState
	{
		get
		{
			if (EquipItemData == null)
			{
				return false;
			}
			if (!EquipItemData.IsSpecialWeapon)
			{
				return EquipItemData.IsSpecialTreasure;
			}
			return true;
		}
	}

	public override bool IsInventoryMode()
	{
		return !_isInHeroEquip;
	}

	public void Open(CachedHeroInfo cachedHeroInfo, EquipItemInfo equipItemInfo, HeroEquipInfo.Listener listener, IHeroIterator heroIterator, InvenEquipInfoUIContainer.WindowPosType windowPosType)
	{
		_isInHeroEquip = true;
		if (equipItemInfo != null)
		{
			_listener = listener as Listener;
			Open(cachedHeroInfo, equipItemInfo, listener, heroIterator);
			_windowPosType = windowPosType;
			UIHelper.EnableButton(BreakButton, _isBreakable);
			UIHelper.EnableButton(SellButton, _isSellable);
			UIHelper.EnableButton(Button_Keep, _isKeepable);
		}
	}

	public void OpenInventoryMode(EquipItemInfo equipItemInfo, Listener listener, IInvenModeListener uiEventListener)
	{
		_isInHeroEquip = false;
		if (equipItemInfo != null)
		{
			Open(delegate
			{
				Init(equipItemInfo, listener);
			}, null);
			_uiEventListener = uiEventListener;
			_windowPosType = InvenEquipInfoUIContainer.WindowPosType.Right;
		}
	}

	public void Init(EquipItemInfo equipItemInfo, Listener listener)
	{
		Init(null, equipItemInfo, listener, null);
		_listener = listener;
		_inventoryMode = IsInventoryMode();
		SetupEquipState();
		UIHelper.EnableGameObject(BackgroundObject, enable: true);
	}

	protected override void OnChangeLockState()
	{
		base.OnChangeLockState();
		SetupEquipState();
	}

	protected override void SetupEquipState()
	{
		UIHelper.EnableGameObject(MoveButton, _inventoryMode);
		UIHelper.EnableGameObject(LabelEquip, !EquipItemInfo.IsEquipped);
		UIHelper.EnableGameObject(LabelUnEquip, EquipItemInfo.IsEquipped);
		UIHelper.EnableGameObject(BreakButton, enable: true);
		UIHelper.EnableButton(BreakButton, _isBreakable);
		UIHelper.EnableButton(SellButton, _isSellable);
		UIHelper.EnableButton(MoveButton, _isEquipable);
		UIHelper.EnableButton(Button_Keep, _isKeepable);
		UIHelper.EnableGameObject(SpecialEquipInfoButton, CheckSpecialEquipInfoButtonState);
		SetTechnoMagicButtonState();
		SetSoulWeaponButtonState();
		if (_inventoryMode)
		{
			UIHelper.EnableGameObject(AwakenButton, enable: false);
			UIHelper.EnableGameObject(EquipButton, enable: false);
			UIHelper.EnableGameObject(UnequipButton, enable: false);
			SetupChestItemState();
		}
		else
		{
			base.SetupEquipState();
		}
	}

	private void SetTechnoMagicButtonState()
	{
		if (EquipItemData == null || EquipItemData.EquipType != EquipItemEquipType.TechnoMagic)
		{
			UIHelper.EnableGameObject(TechnoMagicIdentifiedButton, enable: false);
			return;
		}
		if (EquipItemInfo == null || EquipItemInfo.Identified != 0)
		{
			UIHelper.EnableGameObject(TechnoMagicIdentifiedButton, enable: false);
			return;
		}
		UIHelper.EnableGameObject(TechnoMagicIdentifiedButton, enable: true);
		if (EquipItemInfo.GetInventoryType() == InventoryType.Chest)
		{
			UIHelper.EnableButton(TechnoMagicIdentifiedButton, enable: false);
		}
		else
		{
			UIHelper.EnableButton(TechnoMagicIdentifiedButton, enable: true);
		}
	}

	private void SetSoulWeaponButtonState()
	{
		if (EquipItemInfo == null || EquipItemInfo.EquipItemData == null || __Extension_Method_ICollection__.IsNullOrEmpty(EquipItemInfo.EquipItemData.CreatureIndex))
		{
			UIHelper.EnableGameObject(SoulWeaponLiberationButton, enable: false);
			UIHelper.EnableGameObject(SoulWeaponManageButton, enable: false);
			UIHelper.EnableGameObject(SoulWeaponCancelButton, enable: false);
			return;
		}
		bool flag = false;
		NGame2.NAccount.UserSoulWeaponManager soulWeaponManager = GlobalAutoBehaviour<AccountManager>.instance.SoulWeaponManager;
		bool num = SoulWeaponManagement.IsOpenSoulweapon() && SoulWeaponManagement.IsOpenGodKingTemple();
		if (EquipItemInfo != null && EquipItemInfo.IsSoulWeapon())
		{
			flag = true;
		}
		UIHelper.EnableGameObject(SoulWeaponLiberationButton, EquipItemInfo.EquipItemData != null && EquipItemInfo.EquipItemData.SubType == EquipItemSubType.SpecialWeapon && !flag);
		UIHelper.EnableGameObject(SoulWeaponManageButton, flag);
		UIHelper.EnableGameObject(SoulWeaponCancelButton, flag);
		UIHelper.EnableGameObject(BreakButton, !flag);
		bool flag2 = EquipItemInfo.GetInventoryType() == InventoryType.Chest;
		bool flag3 = DispatchHelper.IsDispatchedHero(EquipItemInfo.EquippedHeroInfo);
		bool enable = !flag2 && !flag3;
		bool flag4 = num && !flag2 && !flag3 && EquipItemInfo.EquipItemData.IsOpenSoulWeapon;
		if (flag4)
		{
			flag4 = !soulWeaponManager.CheckAlreadySoulWeapon(EquipItemInfo) && soulWeaponManager.HaveSoulWeaponHero(EquipItemInfo);
		}
		UIHelper.EnableButton(SoulWeaponLiberationButton, flag4);
		UIHelper.EnableButton(SoulWeaponManageButton, enable);
		SetEnableSoulWeaponBreakButtonState(EquipItemInfo);
	}

	private void SetEnableSoulWeaponBreakButtonState(EquipItemInfo equipItemInfo)
	{
		if (equipItemInfo != null)
		{
			bool flag = !equipItemInfo.IsEquipped && EquipItemInfo.Locked == 0;
			bool flag2 = equipItemInfo.GetInventoryType() == InventoryType.Chest;
			UIHelper.EnableButton(SoulWeaponCancelButton, !flag2 && flag);
		}
	}

	public void SetupChestItemState()
	{
		if (EquipItemInfo.GetInventoryType() == InventoryType.Chest)
		{
			UIHelper.EnableButton(BreakButton, enable: false);
			UIHelper.EnableButton(SellButton, enable: false);
			UIHelper.EnableButton(MoveButton, enable: false);
			UIHelper.EnableButton(Button_Keep, enable: true);
			UIHelper.EnableGameObject(UpgradeButtonTween, enable: false);
			UIHelper.EnableGameObject(EquipButton, enable: false);
			UIHelper.EnableGameObject(AwakenButton, enable: false);
			UIHelper.EnableGameObject(UnequipButton, enable: false);
		}
	}

	private bool IsExistsUniqueItemPartType()
	{
		if (EquipItemData == null)
		{
			return false;
		}
		if (EquipItemData.PartType == EquipPartType.Weapon || EquipItemData.PartType == EquipPartType.Treasure)
		{
			return true;
		}
		return false;
	}

	public override void Reset()
	{
		base.Reset();
		UIHelper.EnableGameObject(BackgroundObject, enable: false);
		UIHelper.EnableGameObject(MoveButton, enable: false);
	}

	public void OnClickMoveButton()
	{
		if (_listener != null)
		{
			_listener.OnMove(this);
		}
	}

	public override void OnClickSellButton()
	{
		if (_listener != null)
		{
			_listener.OnSell(this);
		}
		else
		{
			base.OnClickSellButton();
		}
	}

	public void OnClickBreak()
	{
		if (_listener != null)
		{
			_listener.OnBreak(this);
		}
	}

	public void OnClickKeep()
	{
		if (_listener != null)
		{
			if (EquipItemInfo.GetInventoryType() == InventoryType.Inventory)
			{
				_listener.OnKeep(this);
			}
			else
			{
				_listener.OnBring(this);
			}
		}
	}

	public void OnClickTechnoMagicIdentified()
	{
		if (!SingletonAuto<ContentsOpenChecker>.instance.CheckContentsOpen(ContentsType.Valance))
		{
			CenterMessage.Open(LocalizationManager.GetStringDirect("TECHNOMAGIC_ITEM_NOTICE_QUICKBUTTON"));
		}
		else if (_listener != null)
		{
			_listener.TechnoMagicIdentified(EquipItemInfo);
		}
	}

	public void OnClickSoulWeaponLiberation()
	{
		if (_listener != null)
		{
			_listener.OpenSoulWeaponLibearationWindow(EquipItemInfo);
		}
	}

	public void OnClickSoulWeaponManage()
	{
		if (_listener != null)
		{
			_listener.OpenSoulWeaponManageWindow(EquipItemInfo);
		}
	}

	public void OnClickSoulWeaponCancel()
	{
		if (_listener != null)
		{
			_listener.CancelSoulWeapon(EquipItemInfo);
		}
	}

	public override void OnClosing()
	{
		if (_uiEventListener != null)
		{
			_uiEventListener.OnClickClose();
		}
		base.OnClosing();
	}

	public override void OnClickClose()
	{
		if (_uiEventListener != null)
		{
			_uiEventListener.OnClickClose();
		}
		base.OnClickClose();
	}

	public void OnClickHeroSpecialEquipInfo()
	{
		if (_listener != null)
		{
			_listener.ClickHeroSpecialEquipInfo(_windowPosType, EquipItemInfo);
		}
	}
}

using NGame2.NAccount;
using NGame2.NDropBonusEvent;
using NGame2.NPayShopItemEvent;
using NShared;
using NVespa;
using UnityEngine;

namespace NGame2.NUI.NWindow.NCustomizing;

public class WidgetBase : MonoBehaviour
{
	protected IRenderFacadeController _renderFacadeController;

	protected IWidgetUiController _widgetUiController;

	protected string LocaleKeyBuyButton = "CONTENTS_CUSTOMIZE_SINGLE_BUY_BTN";

	protected ToggleType _toggleType;

	protected HeroCostumeView.CostumeInfoUI CostumeInfoUI => _widgetUiController.GetCostumeInfoUI();

	protected HeroCostumeView.CostumeButtonInfo[] ButtonInfos => _widgetUiController.GetCostumeButtons();

	public UILabel Label_CostumeName => CostumeInfoUI.Label_CostumeName;

	public UILabel Label_CostumeNameOutline => CostumeInfoUI.Label_CostumeNameOutline;

	protected UIWidget WidgetObtainDesc => CostumeInfoUI.WidgetObtainDesc;

	protected UIWidget WidgetPriceDesc => CostumeInfoUI.WidgetPriceDesc;

	protected UIWidget WidgetMileageDesc => CostumeInfoUI.WidgetMileageDesc;

	protected UIWidget WidgetOwned => CostumeInfoUI.WidgetOwned;

	protected UIWidget WidgetEquipped => CostumeInfoUI.WidgetEquipped;

	protected UIWidget WidgetNeedCostume => CostumeInfoUI.WidgetNeedCostume;

	protected UILabel Label_Requirement => CostumeInfoUI.Label_Requirement;

	protected UILabel Label_Price => CostumeInfoUI.Label_Price;

	protected UILabel Label_DiscountPrice => CostumeInfoUI.Label_DiscountPrice;

	protected UILabel Label_MileageValue => CostumeInfoUI.Label_MileageValue;

	protected UILabel Label_AbilityDesc => CostumeInfoUI.Label_AbilityDesc;

	public CachedHeroInfo HeroInfo
	{
		get
		{
			if (_renderFacadeController != null)
			{
				return _renderFacadeController.GetHeroInfo();
			}
			return null;
		}
	}

	public CreatureData CreatureData
	{
		get
		{
			if (HeroInfo != null)
			{
				return HeroInfo.CreatureData;
			}
			return null;
		}
	}

	public virtual void OnChanged(CostumeStorageSlotInfo changedInfo)
	{
	}

	public virtual void OnChanged(NGame2.NAccount.UserHeroManager.ChangeHeroInfo changeHeroInfo)
	{
	}

	public virtual void OnChanged(PayShopItemEventManager.EventChangedInfo info)
	{
	}

	public virtual void OnChanged(DropBonusEventManager.DropBonusEventChangeInfo dropBonusEventChangeInfo)
	{
	}

	public virtual void Open(ToggleType toggleType)
	{
		_toggleType = toggleType;
	}

	public virtual void OnClose()
	{
	}

	public virtual void Reset()
	{
	}

	public virtual WidgetBase SetRenderFacadeController(IRenderFacadeController renderFacadeController)
	{
		_renderFacadeController = renderFacadeController;
		return this;
	}

	public virtual WidgetBase SetUiProvider(IWidgetUiController widgetUiProvider)
	{
		_widgetUiController = widgetUiProvider;
		return this;
	}

	public virtual void OnButtonEvent(ButtonClickEvent buttonEvent)
	{
		switch (buttonEvent)
		{
		case ButtonClickEvent.Equip:
			OnEquip();
			break;
		case ButtonClickEvent.UnEquip:
			OnUnEquip();
			break;
		case ButtonClickEvent.BuyCostume:
			OnBuyCostume();
			break;
		case ButtonClickEvent.BuyHero:
			OnBuyHero();
			break;
		case ButtonClickEvent.GoSubstory:
			OnSubStory();
			break;
		case ButtonClickEvent.SetSave:
			OnSaveSet();
			break;
		case ButtonClickEvent.SetEquip:
			OnEquipSet();
			break;
		}
	}

	public virtual void OnEquip()
	{
		VespaDebug.LogError("OnEquip");
	}

	public virtual void OnUnEquip()
	{
		VespaDebug.LogError("OnUnEquip");
	}

	public virtual void OnBuyCostume()
	{
		VespaDebug.LogError("OnBuyCostume");
	}

	public virtual void OnBuyHero()
	{
		VespaDebug.LogError("OnBuyHero");
	}

	public virtual void OnSubStory()
	{
		VespaDebug.LogError("OnSubStory");
	}

	public virtual void OnSaveSet()
	{
		VespaDebug.LogError("OnSaveSet");
	}

	public virtual void OnEquipSet()
	{
		VespaDebug.LogError("OnEquipSet");
	}
}

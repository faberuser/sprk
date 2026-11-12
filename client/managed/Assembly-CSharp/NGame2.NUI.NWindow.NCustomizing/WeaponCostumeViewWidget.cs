using System;
using System.Collections.Generic;
using System.Linq;
using NGame2.NAccount;
using NGame2.NDropBonusEvent;
using NGame2.NPayShopItemEvent;
using NGame2.NUI.NComponent2.NCostume;
using NGame2.NUtil.NCostume;
using NShared;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow.NCustomizing;

public class WeaponCostumeViewWidget : WidgetBase, WeaponCostumeComponent.IListener
{
	public interface IListener
	{
		void RequestBuyHeroCostume(CostumeData costumeData, Action<CostumeData> OnComplete);

		void OnClickEquipCostume(HeroCustomizingDataGetter getter);

		void OnClickUnequipCostume(HeroCustomizingDataGetter getter);

		void OnClickBuyCostume(HeroCustomizingDataGetter getter);

		void OnClickBuyHero(CreatureData creatureData);

		void OnClickSubStory(int heroIndex);
	}

	[Serializable]
	public class CostumeButtonInfo
	{
		public CostumeState State;

		public UIButton Button;
	}

	public UIBaseGrid CostumeGrid;

	private IListener _listener;

	private List<WeaponCostumeComponent> _componentList = new List<WeaponCostumeComponent>();

	public WeaponCostumeComponent.OpenData CurrentComponentOpenData { get; private set; }

	private NGame2.NAccount.UserAccessoryCostumeManager UserAccessoryCostumeManager => GlobalAutoBehaviour<AccountManager>.instance.AccessoryCostumeManager;

	private NGame2.NAccount.UserHeroManager UserHeroManager => GlobalAutoBehaviour<AccountManager>.instance.HeroManager;

	void WeaponCostumeComponent.IListener.OnClick(WeaponCostumeComponent component)
	{
		SelectCostume(component.CurrentComponentOpenData);
	}

	private void ClearCostumeComponentList()
	{
		FactoryBase.ReleaseAndClearList(_componentList);
	}

	private void Open()
	{
		if (CurrentComponentOpenData.IsValid())
		{
			SetCostumeName();
			UpdateCostumeListState();
			UpdateButtonState();
			UpdateCurrentCostumeStatus();
		}
		else
		{
			InitCostumeList();
			InitHeroCostumeInfo();
		}
	}

	private void InitCostumeList()
	{
		if (base.HeroInfo != null)
		{
			ClearCostumeComponentList();
			CreateCostumeList();
			UpdateCostumeListState();
		}
	}

	private void AddWeaponOptionComponent()
	{
		WeaponCostumeComponent weaponCostumeComponent = CostumeGrid.AddGridItem(WeaponCostumeComponent.PrefabAuto);
		weaponCostumeComponent.Open(WeaponCostumeComponent.OpenData.CreateSelectUnique(), this);
		WeaponCostumeComponent weaponCostumeComponent2 = CostumeGrid.AddGridItem(WeaponCostumeComponent.PrefabAuto);
		weaponCostumeComponent2.Open(WeaponCostumeComponent.OpenData.CreateSelectThema(), this);
		_componentList.Add(weaponCostumeComponent);
		_componentList.Add(weaponCostumeComponent2);
	}

	private void CreateCostumeList()
	{
		if (base.HeroInfo == null)
		{
			return;
		}
		AddWeaponOptionComponent();
		int num = 0;
		foreach (WeaponCostumeData item in GameRoot.Instance.WeaponCostumeDataContainer.GetDataListByHeroIndex(base.HeroInfo.HeroIndex))
		{
			if (item.IsOpen && (!item.PreviewableWhenNeedCostumeOwned || (!__Extension_Method_ICollection__.IsNullOrEmpty(item.NeedCostume) && BaseCostume.IsOwnedCostume(item.NeedCostume))) && (!item.PreviewableWhenNeedWeaponOwned || GlobalAutoBehaviour<AccountManager>.instance.WeaponCostumeManager.Get(item.Index) != null))
			{
				WeaponCostumeComponent weaponCostumeComponent = CostumeGrid.AddGridItem(WeaponCostumeComponent.PrefabAuto);
				weaponCostumeComponent.Open(WeaponCostumeComponent.OpenData.CreateSelectCostume(item), this);
				_componentList.Add(weaponCostumeComponent);
				num++;
			}
		}
		if (num == 0)
		{
			WeaponCostumeComponent weaponCostumeComponent2 = CostumeGrid.AddGridItem(WeaponCostumeComponent.PrefabAuto);
			weaponCostumeComponent2.Open(WeaponCostumeComponent.OpenData.CreatureNoProduct(), this);
			_componentList.Add(weaponCostumeComponent2);
		}
		CostumeGrid.RepositionGrid();
	}

	private void UpdateCostumeListState()
	{
		if (base.HeroInfo == null)
		{
			return;
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_componentList); i++)
		{
			WeaponCostumeComponent weaponCostumeComponent = _componentList[i];
			WeaponCostumeComponent.OpenData currentComponentOpenData = weaponCostumeComponent.CurrentComponentOpenData;
			if (currentComponentOpenData.IsValid())
			{
				CostumeState costumeState = WeaponCostume.GetCostumeState(base.HeroInfo, currentComponentOpenData);
				if (costumeState == CostumeState.UnEquipSelect)
				{
					costumeState = CostumeState.Equipped;
				}
				weaponCostumeComponent.SetState(costumeState);
			}
		}
	}

	private void UpdateCostumeListPrice()
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_componentList); i++)
		{
			WeaponCostumeComponent weaponCostumeComponent = _componentList[i];
			if (weaponCostumeComponent.CurrentComponentOpenData.IsValid())
			{
				weaponCostumeComponent.SetCostumePrice();
			}
		}
	}

	private void UpdateCostumeListSelectDeselect(WeaponCostumeComponent.OpenData selectCostumeData, WeaponCostumeComponent.OpenData DeselectCostumeData, List<WeaponCostumeComponent> costumeComponentList)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(costumeComponentList); i++)
		{
			WeaponCostumeComponent.OpenData currentComponentOpenData = costumeComponentList[i].CurrentComponentOpenData;
			if (currentComponentOpenData.IsEqual(DeselectCostumeData))
			{
				costumeComponentList[i].SetSelected(Selected: false);
			}
			if (currentComponentOpenData.IsEqual(selectCostumeData))
			{
				costumeComponentList[i].SetSelected(Selected: true);
			}
		}
	}

	public void InitHeroCostumeInfo()
	{
		if (base.HeroInfo != null)
		{
			WeaponCostumeComponent.OpenData selectedOpenData = WeaponCostumeComponent.OpenData.CreateSelectUnique();
			if (base.HeroInfo.WeaponCostumeIndex != 0)
			{
				selectedOpenData = WeaponCostumeComponent.OpenData.CreateSelectCostume(GameRoot.Instance.WeaponCostumeDataContainer.GetData(base.HeroInfo.WeaponCostumeIndex));
			}
			else if (base.HeroInfo.HideUniqueWeapon == 1)
			{
				selectedOpenData = WeaponCostumeComponent.OpenData.CreateSelectThema();
			}
			SelectCostume(selectedOpenData);
		}
	}

	public void SelectCostume(WeaponCostumeComponent.OpenData selectedOpenData)
	{
		if (!CurrentComponentOpenData.IsValid() && __Extension_Method_ICollection__.COUNT(_componentList) == 0)
		{
			InitCostumeList();
		}
		CachedHeroInfo localHeroInfo = _renderFacadeController.GetLocalHeroInfo();
		string weaponPrefix = string.Empty;
		int star = 0;
		if (localHeroInfo.CostumeIndex != 0)
		{
			if (selectedOpenData.ComponentType != WeaponCostumeComponent.ComponentType.SelectThema)
			{
				CheckLegendHeroCostume();
			}
			CostumeData costumeData = GameRoot.Instance.CostumeDataContainer.GetData(localHeroInfo.CostumeIndex);
			if (costumeData == null)
			{
				costumeData = GameRoot.Instance.CostumeDataContainer.GetDataByHeroStar(localHeroInfo.HeroIndex, localHeroInfo.Star);
			}
			weaponPrefix = costumeData.WeaponPrefix;
			star = costumeData.HeroStar;
		}
		if (selectedOpenData.ComponentType == WeaponCostumeComponent.ComponentType.SelectThema)
		{
			localHeroInfo.HideUniqueWeapon = 1;
			localHeroInfo.WeaponCostumeIndex = 0;
		}
		else if (selectedOpenData.ComponentType == WeaponCostumeComponent.ComponentType.SelectUnique)
		{
			ItemData<EquipItemData> heroEquippedWeapon = GetHeroEquippedWeapon();
			if (heroEquippedWeapon != null && !String_Extension_Method.IsNullOrEmpty(heroEquippedWeapon.SubData.WeaponPrefix))
			{
				weaponPrefix = heroEquippedWeapon.SubData.WeaponPrefix;
				star = 0;
			}
			localHeroInfo.HideUniqueWeapon = 0;
			localHeroInfo.WeaponCostumeIndex = 0;
		}
		else if (selectedOpenData.ComponentType == WeaponCostumeComponent.ComponentType.SelectCostume)
		{
			WeaponCostumeData weaponCostumeData = selectedOpenData.WeaponCostumeData;
			weaponPrefix = weaponCostumeData.WeaponPrefix;
			star = weaponCostumeData.Star;
			localHeroInfo.HideUniqueWeapon = 1;
			localHeroInfo.WeaponCostumeIndex = weaponCostumeData.Index;
		}
		WeaponCostumeComponent.OpenData currentComponentOpenData = CurrentComponentOpenData;
		CurrentComponentOpenData = selectedOpenData;
		bool isEquipSoulWeapon = false;
		int soulWeaponGrade = -1;
		GetEquipSoulWeaponInfo(out isEquipSoulWeapon, out soulWeaponGrade);
		_renderFacadeController.ChangeRenderFacadeWeapon(weaponPrefix, star, isEquipSoulWeapon, soulWeaponGrade);
		UpdateCostumeListSelectDeselect(CurrentComponentOpenData, currentComponentOpenData, _componentList);
		SetCostumeName();
		UpdateCostumeListState();
		UpdateButtonState();
		UpdateCurrentCostumeStatus();
		_renderFacadeController.OnSelectCostume();
	}

	private void CheckLegendHeroCostume()
	{
		CachedHeroInfo localHeroInfo = _renderFacadeController.GetLocalHeroInfo();
		CostumeData data = GameRoot.Instance.CostumeDataContainer.GetData(localHeroInfo.CostumeIndex);
		if (data != null && data.CostumeType == EHeroCostumeType.Legend)
		{
			CostumeData dataByHeroStar = GameRoot.Instance.CostumeDataContainer.GetDataByHeroStar(localHeroInfo.HeroIndex, localHeroInfo.Star);
			HeroCostumeViewWidget heroCostumeViewWidget = _widgetUiController.GetCostumeViewWidget(ToggleType.Costume) as HeroCostumeViewWidget;
			if (heroCostumeViewWidget != null)
			{
				heroCostumeViewWidget.SelectCostume(dataByHeroStar);
			}
		}
	}

	private ItemData<EquipItemData> GetHeroEquippedWeapon()
	{
		int equipSlotIndex = EquipPartType.Weapon.GetEquipSlotIndex();
		int equipItemSlotIndex = base.HeroInfo.GetEquipItemSlotIndex(equipSlotIndex);
		EquipItemInfo equipItemInfo = GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.Get(equipItemSlotIndex);
		if (equipItemInfo != null)
		{
			return GameRoot.Instance.ItemDataContainer.GetData<EquipItemData>(equipItemInfo.ItemIndex);
		}
		return null;
	}

	public void GetEquipSoulWeaponInfo(out bool isEquipSoulWeapon, out int soulWeaponGrade)
	{
		isEquipSoulWeapon = false;
		soulWeaponGrade = -1;
		EquipItemInfo equipItemInfoByPartType = base.HeroInfo.GetEquipItemInfoByPartType(EquipPartType.Weapon);
		if (equipItemInfoByPartType != null && equipItemInfoByPartType.IsSoulWeapon())
		{
			isEquipSoulWeapon = true;
			soulWeaponGrade = equipItemInfoByPartType.SoulWeaponInfo.Grade;
		}
	}

	private void SetCostumeName()
	{
		UIHelper.EnableGameObject(base.Label_CostumeName, enable: true);
		string text = string.Empty;
		if (CurrentComponentOpenData.ComponentType == WeaponCostumeComponent.ComponentType.SelectUnique)
		{
			text = LocalizationManager.GetStringDirect("CONTENT_CUSTOMIZE_LIST_WEAPON_UNIQUE_DESC");
		}
		else if (CurrentComponentOpenData.ComponentType == WeaponCostumeComponent.ComponentType.SelectThema)
		{
			text = LocalizationManager.GetStringDirect("CONTENT_CUSTOMIZE_LIST_WEAPON_THEME_DESC");
		}
		else if (CurrentComponentOpenData.ComponentType == WeaponCostumeComponent.ComponentType.SelectCostume)
		{
			text = ((CurrentComponentOpenData.WeaponCostumeData == null) ? "(empty)" : CurrentComponentOpenData.WeaponCostumeData.Desc);
		}
		UIHelper.SetLabelText(base.Label_CostumeName, text);
		UIHelper.SetLabelText(base.Label_CostumeNameOutline, text);
	}

	public void UpdateButtonState()
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(base.ButtonInfos); i++)
		{
			UIHelper.EnableGameObject(base.ButtonInfos[i].Button, enable: false);
		}
		if (base.HeroInfo == null || !CurrentComponentOpenData.IsValid())
		{
			return;
		}
		CostumeState costumeState = WeaponCostume.GetCostumeState(base.HeroInfo, CurrentComponentOpenData);
		HeroCostumeView.CostumeButtonInfo costumeButtonInfo = ((base.ButtonInfos != null) ? base.ButtonInfos.FirstOrDefault((HeroCostumeView.CostumeButtonInfo e) => e.State == costumeState) : null);
		if (costumeButtonInfo == null)
		{
			return;
		}
		UIHelper.EnableGameObject(costumeButtonInfo.Button, enable: true);
		bool enable = true;
		if (costumeState == CostumeState.NeedHero)
		{
			enable = base.CreatureData != null && base.CreatureData.OpenType == CreatureOpenType.Opened;
		}
		else if (costumeState == CostumeState.NeedAwaken)
		{
			enable = false;
		}
		else if (costumeState != CostumeState.Owned)
		{
			if (costumeState == CostumeState.Equipped)
			{
				enable = true;
			}
			else if (costumeState == CostumeState.UnEquipSelect)
			{
				enable = false;
			}
			else if (costumeState == CostumeState.CannotBuy)
			{
				enable = false;
			}
			else if (costumeState == CostumeState.NeedSubStory)
			{
				enable = true;
			}
			else if (costumeState == CostumeState.Buyable)
			{
				UILabel componentInChildren = costumeButtonInfo.Button.GetComponentInChildren<UILabel>();
				if (CurrentComponentOpenData.WeaponCostumeData != null && CurrentComponentOpenData.WeaponCostumeData.ReqBuyGem > 0)
				{
					UIHelper.SetLabelText(componentInChildren, LocalizationManager.GetStringDirect(LocaleKeyBuyButton));
				}
				else
				{
					UIHelper.SetLabelText(componentInChildren, LocalizationManager.GetStringDirect("CONTENT_CUSTOMIZE_NEED_COSTUME_BTN"));
				}
			}
		}
		UIHelper.EnableButton(costumeButtonInfo.Button, enable);
	}

	public void UpdateCurrentCostumeStatus()
	{
		UIHelper.EnableGameObject(base.Label_AbilityDesc, enable: false);
		UIHelper.EnableGameObject(base.WidgetPriceDesc, enable: false);
		UIHelper.EnableGameObject(base.WidgetMileageDesc, enable: false);
		CostumeState costumeState = WeaponCostume.GetCostumeState(base.HeroInfo, CurrentComponentOpenData, checkHeroOwn: false);
		UIHelper.EnableGameObject(base.WidgetNeedCostume, costumeState == CostumeState.Buyable);
		UIHelper.EnableGameObject(base.WidgetObtainDesc, costumeState == CostumeState.NeedAwaken || costumeState == CostumeState.CannotBuy || costumeState == CostumeState.NeedSubStory);
		UIHelper.SetLabelText(base.Label_Requirement, LocalizationManager.GetStringDirect("COSTUMESHOP_NEEDAWAKE"));
		UIHelper.EnableGameObject(base.WidgetOwned, costumeState == CostumeState.Owned && CurrentComponentOpenData.ComponentType == WeaponCostumeComponent.ComponentType.SelectCostume);
		bool enable = costumeState == CostumeState.Equipped || (base.HeroInfo.CostumeIndex == 0 && costumeState == CostumeState.UnEquipSelect);
		UIHelper.EnableGameObject(base.WidgetEquipped, enable);
		if (costumeState != CostumeState.NeedAwaken && costumeState != CostumeState.CannotBuy && costumeState != CostumeState.NeedSubStory)
		{
			return;
		}
		WeaponCostumeData weaponCostumeData = CurrentComponentOpenData.WeaponCostumeData;
		if (weaponCostumeData != null)
		{
			int priceGem = WeaponCostume.GetPriceGem(weaponCostumeData);
			long i_value = priceGem;
			int obtainMileage = WeaponCostume.GetObtainMileage(weaponCostumeData);
			long i_value2 = obtainMileage;
			int priceDiscountPercentage = BaseCostume.GetPriceDiscountPercentage(weaponCostumeData);
			UIHelper.EnableGameObject(base.Label_DiscountPrice, priceDiscountPercentage > 0);
			UIHelper.EnableGameObject(base.Label_Price, priceDiscountPercentage == 0);
			if (priceDiscountPercentage == 0)
			{
				UIHelper.SetLabelText(base.Label_Price, Vespa_Int_Extension_Method.ToCommaString(i_value));
			}
			if (priceDiscountPercentage > 0)
			{
				UIHelper.SetLabelText(base.Label_DiscountPrice, Vespa_Int_Extension_Method.ToCommaString(i_value));
			}
			UIHelper.SetLabelText(base.Label_MileageValue, Vespa_Int_Extension_Method.ToCommaString(i_value2));
		}
	}

	public void SetListener(IListener listener)
	{
		_listener = listener;
	}

	public override void Open(ToggleType toggleType)
	{
		base.Open(toggleType);
		Open();
	}

	public override void Reset()
	{
		CurrentComponentOpenData = WeaponCostumeComponent.OpenData.NullData;
		ClearCostumeComponentList();
	}

	public override void OnChanged(NGame2.NAccount.UserHeroManager.ChangeHeroInfo changeHeroInfo)
	{
		if (changeHeroInfo.ChangeType != NGame2.NAccount.UserHeroManager.HeroChangeType.WeaponCostume)
		{
			return;
		}
		if (changeHeroInfo.NewValue == 0)
		{
			if (changeHeroInfo.HideUniqueWeapon == 0)
			{
				SelectCostume(WeaponCostumeComponent.OpenData.CreateSelectUnique());
			}
			else
			{
				SelectCostume(WeaponCostumeComponent.OpenData.CreateSelectThema());
			}
		}
		UpdateCostumeListState();
		UpdateButtonState();
		UpdateCurrentCostumeStatus();
	}

	public override void OnChanged(PayShopItemEventManager.EventChangedInfo info)
	{
		UpdateCurrentCostumeStatus();
		UpdateCostumeListState();
		if (CurrentComponentOpenData.IsValid())
		{
			UpdateButtonState();
			UpdateCurrentCostumeStatus();
		}
	}

	public override void OnChanged(DropBonusEventManager.DropBonusEventChangeInfo dropBonusEventChangeInfo)
	{
	}

	private HeroCustomizingDataGetter CreateGetter(bool unsetLegendCostume = false)
	{
		HeroCustomizingDataGetter result = new HeroCustomizingDataGetter
		{
			HeroInfoGetter = () => base.HeroInfo,
			WeaponCostumeOpenDataGetter = () => CurrentComponentOpenData
		};
		if (unsetLegendCostume)
		{
			CachedHeroInfo localHeroInfo = _renderFacadeController.GetLocalHeroInfo();
			CostumeData basicMaxStarCostume = GameRoot.Instance.CostumeDataContainer.GetDataByHeroStar(localHeroInfo.HeroIndex, localHeroInfo.Star);
			result.HeroCostumeDataGetter = () => basicMaxStarCostume;
		}
		return result;
	}

	public override void OnUnEquip()
	{
		base.OnUnEquip();
		if (_listener == null)
		{
			return;
		}
		HeroCustomizingDataGetter getter = CreateGetter();
		getter.WeaponCostumeOpenDataGetter = () => WeaponCostumeComponent.OpenData.CreateSelectUnique();
		CostumeData data = GameRoot.Instance.CostumeDataContainer.GetData(base.HeroInfo.CostumeIndex);
		if (data != null && data.CostumeType == EHeroCostumeType.Legend && CurrentComponentOpenData.ComponentType == WeaponCostumeComponent.ComponentType.SelectThema)
		{
			string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
			string stringDirect = LocalizationManager.GetStringDirect("COSTUME_LEGEND_UNEQUIP_NOTICE_DRESSROOM_APPLY");
			MessagePopup3Button.Open(title, stringDirect, null, delegate
			{
				_listener.OnClickUnequipCostume(getter);
			}, NShared.NLocalization.Common.COMMON_NO, NShared.NLocalization.Common.COMMON_YES);
		}
		else
		{
			_listener.OnClickUnequipCostume(getter);
		}
	}

	public override void OnEquip()
	{
		base.OnEquip();
		if (_listener == null)
		{
			return;
		}
		CostumeData data = GameRoot.Instance.CostumeDataContainer.GetData(base.HeroInfo.CostumeIndex);
		if (data != null && data.CostumeType == EHeroCostumeType.Legend)
		{
			string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
			string stringDirect = LocalizationManager.GetStringDirect("COSTUME_LEGEND_UNEQUIP_NOTICE_DRESSROOM_APPLY");
			MessagePopup3Button.Open(title, stringDirect, null, delegate
			{
				_listener.OnClickEquipCostume(CreateGetter(unsetLegendCostume: true));
			}, NShared.NLocalization.Common.COMMON_NO, NShared.NLocalization.Common.COMMON_YES);
		}
		else
		{
			_listener.OnClickEquipCostume(CreateGetter());
		}
	}

	public override void OnBuyHero()
	{
		base.OnBuyHero();
		if (_listener != null)
		{
			_listener.OnClickBuyHero(base.HeroInfo.CreatureData);
		}
	}

	public override void OnSubStory()
	{
		base.OnSubStory();
		if (_listener != null)
		{
			_listener.OnClickSubStory(base.HeroInfo.HeroIndex);
		}
	}

	public override void OnBuyCostume()
	{
		base.OnBuyCostume();
		if (_listener == null)
		{
			return;
		}
		CostumeData data = GameRoot.Instance.CostumeDataContainer.GetData(base.HeroInfo.CostumeIndex);
		if (data != null && data.CostumeType == EHeroCostumeType.Legend)
		{
			string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
			string stringDirect = LocalizationManager.GetStringDirect("COSTUME_LEGEND_UNEQUIP_NOTICE_DRESSROOM_PURCHASE");
			MessagePopup3Button.Open(title, stringDirect, null, delegate
			{
				BuyWeaponCostume(unsetLegendCostume: true);
			}, NShared.NLocalization.Common.COMMON_NO, NShared.NLocalization.Common.COMMON_YES);
		}
		else
		{
			BuyWeaponCostume();
		}
	}

	private void BuyWeaponCostume(bool unsetLegendCostume = false)
	{
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(CurrentComponentOpenData.WeaponCostumeData.NeedCostume))
		{
			CostumeData data = GameRoot.Instance.CostumeDataContainer.GetData(CurrentComponentOpenData.WeaponCostumeData.NeedCostume[0]);
			if (data == null)
			{
				VespaDebug.LogError("needCostumeData is null ( weaponCostumeIndex " + CurrentComponentOpenData.WeaponCostumeData.NeedCostume[0]);
				return;
			}
			string text = $"[{data.CostumeName}]";
			string stringDirect = LocalizationManager.GetStringDirect("CONTENT_CUSTOMIZE_NEED_COSTUME_WEAPON_WINDOW_TITLE");
			string stringDirect2 = LocalizationManager.GetStringDirect("CONTENT_CUSTOMIZE_NEED_COSTUME_WEAPON_WINDOW_DESC", text);
			ProcessBuyNeedCostume(stringDirect, stringDirect2, data);
		}
		else
		{
			_listener.OnClickBuyCostume(CreateGetter(unsetLegendCostume));
		}
	}

	private void ProcessBuyNeedCostume(string title, string message, CostumeData costumeData)
	{
		MessagePopup.Open(title, message, delegate
		{
			_listener.RequestBuyHeroCostume(costumeData, OnCompleteBuyNeedCostume);
		}, null);
	}

	private void OnCompleteBuyNeedCostume(CostumeData buyCostume)
	{
		ApplyCostume(buyCostume);
		_widgetUiController.SwitchToggle(ToggleType.Costume);
		_listener.OnClickEquipCostume(CreateGetter());
	}

	private void ApplyCostume(CostumeData costumeData)
	{
		CachedHeroInfo localHeroInfo = _renderFacadeController.GetLocalHeroInfo();
		localHeroInfo.CostumeIndex = costumeData.CostumeIndex;
		_renderFacadeController.ChangeRenderFacadeOnly(localHeroInfo);
	}
}

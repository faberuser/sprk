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

public class HairCostumeViewWidget : WidgetBase, HairCostumeComponent.IListener
{
	public interface IListener
	{
		void RequestBuyHeroCostume(CostumeData costumeData, Action<CostumeData> OnComplete);

		void OnClickEquipCostume(HeroCustomizingDataGetter getter);

		void OnClickUnequipCostume(HeroCustomizingDataGetter getter);

		void OnClickBuyCostume(HeroCustomizingDataGetter getter);

		void OnClickBuyHero(CreatureData creatureData);
	}

	[Serializable]
	public class CostumeButtonInfo
	{
		public CostumeState State;

		public UIButton Button;
	}

	public UIBaseGrid CostumeGrid;

	private IListener _listener;

	private List<HairCostumeComponent> _componentList = new List<HairCostumeComponent>();

	public HairCostumeComponent.OpenData CurrentComponentOpenData { get; private set; }

	private NGame2.NAccount.UserAccessoryCostumeManager UserAccessoryCostumeManager => GlobalAutoBehaviour<AccountManager>.instance.AccessoryCostumeManager;

	private NGame2.NAccount.UserHeroManager UserHeroManager => GlobalAutoBehaviour<AccountManager>.instance.HeroManager;

	void HairCostumeComponent.IListener.OnClick(HairCostumeComponent component)
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

	private void AddThemaOptionComponent()
	{
		HairCostumeComponent hairCostumeComponent = CostumeGrid.AddGridItem(HairCostumeComponent.PrefabAuto);
		hairCostumeComponent.Open(HairCostumeComponent.OpenData.CreateSelectThema(), this);
		_componentList.Add(hairCostumeComponent);
	}

	private void CreateCostumeList()
	{
		if (base.HeroInfo == null)
		{
			return;
		}
		AddThemaOptionComponent();
		int num = 0;
		foreach (HairCostumeData item in GameRoot.Instance.HairCostumeDataContainer.GetDataListByHeroIndex(base.HeroInfo.HeroIndex))
		{
			if (item.HeroIndex == base.HeroInfo.HeroIndex && item.IsOpen && (!item.PreviewableWhenNeedCostumeOwned || (!__Extension_Method_ICollection__.IsNullOrEmpty(item.NeedCostume) && BaseCostume.IsOwnedCostume(item.NeedCostume))) && (!item.PreviewableWhenNeedHairOwned || GlobalAutoBehaviour<AccountManager>.instance.HairCostumeManager.Get(item.Index) != null))
			{
				HairCostumeComponent hairCostumeComponent = CostumeGrid.AddGridItem(HairCostumeComponent.PrefabAuto);
				hairCostumeComponent.Open(HairCostumeComponent.OpenData.CreateSelectCostume(item), this);
				_componentList.Add(hairCostumeComponent);
				num++;
			}
		}
		if (num == 0)
		{
			HairCostumeComponent hairCostumeComponent2 = CostumeGrid.AddGridItem(HairCostumeComponent.PrefabAuto);
			hairCostumeComponent2.Open(HairCostumeComponent.OpenData.CreatureNoProduct(), this);
			_componentList.Add(hairCostumeComponent2);
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
			HairCostumeComponent hairCostumeComponent = _componentList[i];
			HairCostumeComponent.OpenData currentComponentOpenData = hairCostumeComponent.CurrentComponentOpenData;
			if (currentComponentOpenData.IsValid())
			{
				CostumeState costumeState = HairCostume.GetCostumeState(base.HeroInfo, currentComponentOpenData);
				if (costumeState == CostumeState.UnEquipSelect)
				{
					costumeState = CostumeState.Equipped;
				}
				hairCostumeComponent.SetState(costumeState);
			}
		}
	}

	private void UpdateCostumeListSelectDeselect(HairCostumeComponent.OpenData selectCostumeData, HairCostumeComponent.OpenData DeselectCostumeData, List<HairCostumeComponent> costumeComponentList)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(costumeComponentList); i++)
		{
			HairCostumeComponent.OpenData currentComponentOpenData = costumeComponentList[i].CurrentComponentOpenData;
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
			HairCostumeComponent.OpenData selectedOpenData = new HairCostumeComponent.OpenData
			{
				ComponentType = HairCostumeComponent.ComponentType.SelectThema
			};
			if (base.HeroInfo.HairCostumeIndex != 0)
			{
				selectedOpenData.ComponentType = HairCostumeComponent.ComponentType.SelectCostumeData;
				selectedOpenData.HairCostumeData = GameRoot.Instance.HairCostumeDataContainer.GetData(base.HeroInfo.HairCostumeIndex);
			}
			SelectCostume(selectedOpenData);
		}
	}

	public void SelectCostume(HairCostumeComponent.OpenData selectedOpenData)
	{
		if (!CurrentComponentOpenData.IsValid() && __Extension_Method_ICollection__.COUNT(_componentList) == 0)
		{
			InitCostumeList();
		}
		CachedHeroInfo localHeroInfo = _renderFacadeController.GetLocalHeroInfo();
		if (selectedOpenData.ComponentType != HairCostumeComponent.ComponentType.SelectThema)
		{
			CheckLegendHeroCostume();
		}
		string hairPrefix = string.Empty;
		if (selectedOpenData.ComponentType == HairCostumeComponent.ComponentType.SelectCostumeData)
		{
			localHeroInfo.HairCostumeIndex = selectedOpenData.HairCostumeData.Index;
			hairPrefix = selectedOpenData.HairCostumeData.HairPrefix;
		}
		else
		{
			localHeroInfo.HairCostumeIndex = 0;
		}
		_renderFacadeController.ChangeRenderFacadeHair(hairPrefix);
		HairCostumeComponent.OpenData currentComponentOpenData = CurrentComponentOpenData;
		CurrentComponentOpenData = selectedOpenData;
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

	private void SetCostumeName()
	{
		UIHelper.EnableGameObject(base.Label_CostumeName, enable: true);
		string text = string.Empty;
		if (CurrentComponentOpenData.ComponentType == HairCostumeComponent.ComponentType.SelectThema)
		{
			text = LocalizationManager.GetStringDirect("CONTENT_CUSTOMIZE_LIST_HAIR_THEME_DESC");
		}
		else if (CurrentComponentOpenData.ComponentType == HairCostumeComponent.ComponentType.SelectCostumeData)
		{
			text = ((CurrentComponentOpenData.HairCostumeData == null) ? "(empty)" : CurrentComponentOpenData.HairCostumeData.Name);
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
		CostumeState costumeState = HairCostume.GetCostumeState(base.HeroInfo, CurrentComponentOpenData);
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
				if (CurrentComponentOpenData.HairCostumeData != null && CurrentComponentOpenData.HairCostumeData.ReqBuyGem > 0)
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
		CostumeState costumeState = HairCostume.GetCostumeState(base.HeroInfo, CurrentComponentOpenData, checkHeroOwn: false);
		UIHelper.EnableGameObject(base.WidgetObtainDesc, costumeState == CostumeState.NeedAwaken || costumeState == CostumeState.CannotBuy || costumeState == CostumeState.NeedSubStory);
		UIHelper.SetLabelText(base.Label_Requirement, LocalizationManager.GetStringDirect("COSTUMESHOP_NEEDAWAKE"));
		bool flag = CurrentComponentOpenData.HairCostumeData != null && CurrentComponentOpenData.HairCostumeData.ReqBuyGem > 0;
		UIHelper.EnableGameObject(base.WidgetNeedCostume, costumeState == CostumeState.Buyable && !flag);
		UIHelper.EnableGameObject(base.WidgetPriceDesc, costumeState == CostumeState.Buyable && flag);
		UIHelper.EnableGameObject(base.WidgetMileageDesc, costumeState == CostumeState.Buyable && flag);
		UIHelper.EnableGameObject(base.WidgetOwned, costumeState == CostumeState.Owned && CurrentComponentOpenData.ComponentType == HairCostumeComponent.ComponentType.SelectCostumeData);
		bool enable = costumeState == CostumeState.Equipped || (base.HeroInfo.HairCostumeIndex == 0 && costumeState == CostumeState.UnEquipSelect);
		UIHelper.EnableGameObject(base.WidgetEquipped, enable);
		if (costumeState == CostumeState.Buyable && flag && CurrentComponentOpenData.HairCostumeData != null)
		{
			int priceGem = HairCostume.GetPriceGem(CurrentComponentOpenData.HairCostumeData);
			long i_value = priceGem;
			int obtainMileage = HairCostume.GetObtainMileage(CurrentComponentOpenData.HairCostumeData);
			long i_value2 = obtainMileage;
			int priceDiscountPercentage = BaseCostume.GetPriceDiscountPercentage(CurrentComponentOpenData.HairCostumeData);
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

	private void UpdateCostumeListPrice()
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_componentList); i++)
		{
			HairCostumeComponent hairCostumeComponent = _componentList[i];
			if (hairCostumeComponent.CurrentComponentOpenData.IsValid())
			{
				hairCostumeComponent.SetCostumePrice();
			}
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
		CurrentComponentOpenData = HairCostumeComponent.OpenData.NullData;
		ClearCostumeComponentList();
	}

	public override void OnChanged(NGame2.NAccount.UserHeroManager.ChangeHeroInfo changeHeroInfo)
	{
		if (changeHeroInfo.ChangeType == NGame2.NAccount.UserHeroManager.HeroChangeType.HairCostume)
		{
			if (changeHeroInfo.NewValue == 0)
			{
				GameRoot.Instance.HairCostumeDataContainer.GetDataByHeroStar(base.HeroInfo.HeroIndex, base.HeroInfo.Star);
				UserHeroManager.Get(changeHeroInfo.HeroIndex);
				SelectCostume(new HairCostumeComponent.OpenData
				{
					ComponentType = HairCostumeComponent.ComponentType.SelectThema
				});
			}
			UpdateCostumeListState();
			UpdateButtonState();
			UpdateCurrentCostumeStatus();
		}
	}

	public override void OnChanged(PayShopItemEventManager.EventChangedInfo info)
	{
		UpdateCostumeListPrice();
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
			HairCostumeOpenDataGetter = () => CurrentComponentOpenData
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
		if (_listener != null)
		{
			HeroCustomizingDataGetter getter = CreateGetter();
			getter.HairCostumeOpenDataGetter = () => HairCostumeComponent.OpenData.CreateSelectThema();
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
				BuyHairCostume(unsetLegendCostume: true);
			}, NShared.NLocalization.Common.COMMON_NO, NShared.NLocalization.Common.COMMON_YES);
		}
		else
		{
			BuyHairCostume();
		}
	}

	private void BuyHairCostume(bool unsetLegendCostume = false)
	{
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(CurrentComponentOpenData.HairCostumeData.NeedCostume))
		{
			CostumeData data = GameRoot.Instance.CostumeDataContainer.GetData(CurrentComponentOpenData.HairCostumeData.NeedCostume[0]);
			if (data == null)
			{
				VespaDebug.LogError("needCostumeData is null ( hairCostumeIndex " + CurrentComponentOpenData.HairCostumeData.NeedCostume[0]);
				return;
			}
			string text = $"[{data.CostumeName}]";
			string stringDirect = LocalizationManager.GetStringDirect("CONTENT_CUSTOMIZE_NEED_COSTUME_HAIR_WINDOW_TITLE");
			string stringDirect2 = LocalizationManager.GetStringDirect("CONTENT_CUSTOMIZE_NEED_COSTUME_HAIR_WINDOW_DESC", text);
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

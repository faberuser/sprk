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
using UnityEngine;

namespace NGame2.NUI.NWindow.NCustomizing;

public class AccessoryCostumeViewWidget : WidgetBase, AccessoryCostumeComponent.Listener, DetailViewWidget.IListener
{
	public interface IListener
	{
		void OnClickEquipCostume(HeroCustomizingDataGetter getter);

		void OnClickUnequipCostume(HeroCustomizingDataGetter getter);

		void OnClickBuyCostume(HeroCustomizingDataGetter getter, Action onComplete);

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

	private AccessoryPartType _partType;

	private List<AccessoryCostumeComponent> _componentList = new List<AccessoryCostumeComponent>();

	private Dictionary<int, PositionInfo> _cachedAccesssoryIndexPositionInfoPair = new Dictionary<int, PositionInfo>();

	public AccessoryCostumeComponent.OpenData CurrentComponentOpenData { get; private set; }

	public AccessoryCostumeData CurrentComponentAccesoryCostumeData => CurrentComponentOpenData.AccessoryCostumeData;

	private NGame2.NAccount.UserAccessoryCostumeManager UserAccessoryCostumeManager => GlobalAutoBehaviour<AccountManager>.instance.AccessoryCostumeManager;

	private NGame2.NAccount.UserHeroManager UserHeroManager => GlobalAutoBehaviour<AccountManager>.instance.HeroManager;

	void AccessoryCostumeComponent.Listener.OnClick(AccessoryCostumeComponent component)
	{
		AccessoryCostumeComponent.OpenData componentOpenData = component.ComponentOpenData;
		SelectCostume(componentOpenData);
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

	private void CreateCostumeList()
	{
		if (base.HeroInfo == null)
		{
			return;
		}
		AddUnsetComponent();
		int num = 0;
		foreach (AccessoryCostumeData allDatum in GameRoot.Instance.AccessoryCostumeDataContainer.GetAllData())
		{
			if ((__Extension_Method_ICollection__.IsNullOrEmpty(allDatum.UnableHero) || !TypeIterator.Contains(allDatum.UnableHero, base.HeroInfo.HeroIndex)) && allDatum.IsOpen && allDatum.PartType == _partType && (!allDatum.PreviewableWhenOwned || GlobalAutoBehaviour<AccountManager>.instance.AccessoryCostumeManager.Get(new PlayerAccessoryCostumeInfoKey(base.HeroInfo.HeroIndex, allDatum.Index)) != null))
			{
				AccessoryCostumeComponent accessoryCostumeComponent = CostumeGrid.AddGridItem(AccessoryCostumeComponent.PrefabAuto);
				accessoryCostumeComponent.Open(AccessoryCostumeComponent.OpenData.CreateCostumeType(allDatum), this);
				_componentList.Add(accessoryCostumeComponent);
				num++;
			}
		}
		if (num == 0)
		{
			AccessoryCostumeComponent accessoryCostumeComponent2 = CostumeGrid.AddGridItem(AccessoryCostumeComponent.PrefabAuto);
			accessoryCostumeComponent2.Open(AccessoryCostumeComponent.OpenData.CreateNoProduct(), this);
			_componentList.Add(accessoryCostumeComponent2);
		}
		CostumeGrid.RepositionGrid();
	}

	public AccessoryCostumeComponent.OpenData GetUnsetOpenData()
	{
		return AccessoryCostumeComponent.OpenData.CreateUnsetPart(_partType);
	}

	public void AddUnsetComponent()
	{
		AccessoryCostumeComponent accessoryCostumeComponent = CostumeGrid.AddGridItem(AccessoryCostumeComponent.PrefabAuto);
		accessoryCostumeComponent.Open(AccessoryCostumeComponent.OpenData.CreateUnsetPart(_partType), this);
		_componentList.Add(accessoryCostumeComponent);
	}

	public void UpdateCostumeListState()
	{
		if (base.HeroInfo == null)
		{
			return;
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_componentList); i++)
		{
			AccessoryCostumeComponent accessoryCostumeComponent = _componentList[i];
			AccessoryCostumeComponent.OpenData componentOpenData = accessoryCostumeComponent.ComponentOpenData;
			if (componentOpenData.IsValid())
			{
				CostumeState costumeState = AccessoryCostume.GetCostumeState(base.HeroInfo, componentOpenData, checkHeroOwn: false);
				if (costumeState == CostumeState.UnEquipSelect)
				{
					costumeState = CostumeState.Equipped;
				}
				accessoryCostumeComponent.SetState(costumeState);
			}
		}
	}

	private void UpdateCostumeListPrice()
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_componentList); i++)
		{
			AccessoryCostumeComponent accessoryCostumeComponent = _componentList[i];
			if (accessoryCostumeComponent.ComponentOpenData.IsValid())
			{
				accessoryCostumeComponent.SetCostumePrice();
			}
		}
	}

	private void UpdateCostumeListSelectDeselect(AccessoryCostumeComponent.OpenData selectedComponentData, AccessoryCostumeComponent.OpenData deselectComponentData, List<AccessoryCostumeComponent> accessosryComponentList)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(accessosryComponentList); i++)
		{
			AccessoryCostumeComponent.OpenData componentOpenData = accessosryComponentList[i].ComponentOpenData;
			if (componentOpenData.IsEqual(deselectComponentData))
			{
				accessosryComponentList[i].SetSelected(Selected: false);
			}
			if (componentOpenData.IsEqual(selectedComponentData))
			{
				accessosryComponentList[i].SetSelected(Selected: true);
			}
		}
	}

	public void InitHeroCostumeInfo()
	{
		if (base.HeroInfo != null)
		{
			AccessoryCostumeData data = GameRoot.Instance.AccessoryCostumeDataContainer.GetData(base.HeroInfo.GetAccessoryIndex(_partType));
			AccessoryCostumeComponent.OpenData selectedComponentOpenData = ((data == null) ? AccessoryCostumeComponent.OpenData.CreateUnsetPart(_partType) : AccessoryCostumeComponent.OpenData.CreateCostumeType(data));
			SelectCostume(selectedComponentOpenData);
		}
	}

	public void SelectCostume(AccessoryCostumeComponent.OpenData selectedComponentOpenData)
	{
		CachedHeroInfo localHeroInfo = _renderFacadeController.GetLocalHeroInfo();
		AccessoryCostumeComponent.OpenData currentComponentOpenData = CurrentComponentOpenData;
		CurrentComponentOpenData = selectedComponentOpenData;
		AccessoryCostumeComponent.ComponentType componentType = CurrentComponentOpenData.ComponentType;
		AccessoryCostumeData accessoryCostumeData = CurrentComponentOpenData.AccessoryCostumeData;
		if (componentType == AccessoryCostumeComponent.ComponentType.SelectCostume && accessoryCostumeData != null)
		{
			AccessoryPartType partType = accessoryCostumeData.PartType;
			int index = accessoryCostumeData.Index;
			PositionInfo positionInfo = PositionInfo.Create();
			if (_cachedAccesssoryIndexPositionInfoPair.ContainsKey(accessoryCostumeData.Index))
			{
				positionInfo = _cachedAccesssoryIndexPositionInfoPair[accessoryCostumeData.Index];
			}
			else
			{
				PlayerAccessoryCostumeInfo playerAccessoryCostumeInfo = UserAccessoryCostumeManager.Get(new PlayerAccessoryCostumeInfoKey(localHeroInfo.HeroIndex, accessoryCostumeData.Index));
				if (playerAccessoryCostumeInfo != null)
				{
					positionInfo.TryAssign(playerAccessoryCostumeInfo.PositionInfo);
				}
			}
			localHeroInfo.SetAccessoryCostume(partType, index, positionInfo);
			_renderFacadeController.ChangeAccessoryCostume(accessoryCostumeData.PartType, accessoryCostumeData, positionInfo);
		}
		else
		{
			localHeroInfo.SetAccessoryCostume(_partType, 0, null);
			_renderFacadeController.ChangeAccessoryCostume(_partType, null);
		}
		UpdateCostumeListSelectDeselect(CurrentComponentOpenData, currentComponentOpenData, _componentList);
		SetCostumeName();
		UpdateCostumeListState();
		UpdateButtonState();
		UpdateCurrentCostumeStatus();
		_renderFacadeController.OnSelectCostume();
		SprkAccessories.Preview(this);
	}

	private void SetCostumeName()
	{
		UIHelper.EnableGameObject(base.Label_CostumeName, enable: true);
		string text = string.Empty;
		if (CurrentComponentOpenData.ComponentType == AccessoryCostumeComponent.ComponentType.SelectUnset)
		{
			text = LocalizationManager.GetStringDirect("\u00a0CONTENT_CUSTOMIZE_NO_SELECT_ACCESSORY_DESC");
		}
		else if (CurrentComponentOpenData.ComponentType == AccessoryCostumeComponent.ComponentType.SelectCostume)
		{
			text = ((CurrentComponentAccesoryCostumeData == null) ? "(empty)" : CurrentComponentAccesoryCostumeData.Name);
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
		if (base.HeroInfo == null)
		{
			SprkAccessories.UpdateButtons(this);
			return;
		}
		if (!CurrentComponentOpenData.IsValid())
		{
			SprkAccessories.UpdateButtons(this);
			return;
		}
		CostumeState costumeState = AccessoryCostume.GetCostumeState(base.HeroInfo, CurrentComponentOpenData);
		HeroCostumeView.CostumeButtonInfo costumeButtonInfo = ((base.ButtonInfos != null) ? base.ButtonInfos.FirstOrDefault((HeroCostumeView.CostumeButtonInfo e) => e.State == costumeState) : null);
		if (costumeButtonInfo != null)
		{
			UIHelper.EnableGameObject(costumeButtonInfo.Button, enable: true);
			bool enable = true;
			if (costumeState == CostumeState.Owned)
			{
				enable = true;
			}
			else if (costumeState == CostumeState.NeedHero)
			{
				enable = base.CreatureData != null && base.CreatureData.OpenType == CreatureOpenType.Opened;
			}
			else if (costumeState == CostumeState.Equipped)
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
			else if (costumeState == CostumeState.Buyable)
			{
				UIHelper.SetLabelText(costumeButtonInfo.Button.GetComponentInChildren<UILabel>(), LocalizationManager.GetStringDirect(LocaleKeyBuyButton));
			}
			UIHelper.EnableButton(costumeButtonInfo.Button, enable);
		}
		SprkAccessories.UpdateButtons(this);
	}

	private void UpdateCurrentCostumeStatus()
	{
		UIHelper.EnableGameObject(base.Label_AbilityDesc, enable: false);
		UIHelper.EnableGameObject(base.WidgetNeedCostume, enable: false);
		CostumeState costumeState = AccessoryCostume.GetCostumeState(base.HeroInfo, CurrentComponentOpenData, checkHeroOwn: false);
		UIHelper.EnableGameObject(base.WidgetObtainDesc, costumeState == CostumeState.CannotBuy);
		UIHelper.EnableGameObject(base.WidgetPriceDesc, costumeState == CostumeState.Buyable);
		UIHelper.EnableGameObject(base.WidgetMileageDesc, costumeState == CostumeState.Buyable);
		UIHelper.EnableGameObject(base.WidgetOwned, costumeState == CostumeState.Owned && CurrentComponentOpenData.ComponentType == AccessoryCostumeComponent.ComponentType.SelectCostume);
		bool enable = costumeState == CostumeState.Equipped || (base.HeroInfo.CostumeIndex == 0 && costumeState == CostumeState.UnEquipSelect);
		UIHelper.EnableGameObject(base.WidgetEquipped, enable);
		switch (costumeState)
		{
		case CostumeState.CannotBuy:
			LocalizationManager.GetString(Costume.COSTUME_REQUIREMENT);
			UIHelper.SetLabelText(base.Label_Requirement, string.Empty);
			break;
		case CostumeState.Buyable:
		{
			AccessoryCostumeData accessoryCostumeData = CurrentComponentOpenData.AccessoryCostumeData;
			if (accessoryCostumeData != null)
			{
				int priceGem = AccessoryCostume.GetPriceGem(accessoryCostumeData);
				long i_value = priceGem;
				int obtainMileage = AccessoryCostume.GetObtainMileage(accessoryCostumeData);
				long i_value2 = obtainMileage;
				int priceDiscountPercentage = BaseCostume.GetPriceDiscountPercentage(accessoryCostumeData);
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
			break;
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
		_partType = toggleType.GetAccessoryType();
		Open();
	}

	public override void Reset()
	{
		_cachedAccesssoryIndexPositionInfoPair.Clear();
		CurrentComponentOpenData = AccessoryCostumeComponent.OpenData.NullData;
		ClearCostumeComponentList();
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

	public override void OnChanged(NGame2.NAccount.UserHeroManager.ChangeHeroInfo changeHeroInfo)
	{
		if (changeHeroInfo.ChangeType == NGame2.NAccount.UserHeroManager.HeroChangeType.AccessoryCostume && changeHeroInfo.PartIndex == (int)_partType)
		{
			if (changeHeroInfo.NewValue == 0)
			{
				SelectCostume(AccessoryCostumeComponent.OpenData.CreateUnsetPart(_partType));
			}
			UpdateCostumeListState();
			UpdateButtonState();
			UpdateCurrentCostumeStatus();
		}
	}

	public override void OnChanged(DropBonusEventManager.DropBonusEventChangeInfo dropBonusEventChangeInfo)
	{
	}

	public override void OnUnEquip()
	{
		OnEquip();
	}

	private HeroCustomizingDataGetter CreateCurrentAccessoryCostumeGetter()
	{
		HeroCustomizingDataGetter getter = default(HeroCustomizingDataGetter);
		getter.HeroInfoGetter = () => base.HeroInfo;
		getter.AccessoryCostumeOpenDataGetterList = new List<Func<AccessoryCostumeComponent.OpenData>>();
		getter.AccessoryCostumeOpenDataGetterList.Add(() => CurrentComponentOpenData);
		getter.AccessoryPositionInfoGetter = (AccessoryCostumeData accessoryData) => _renderFacadeController.GetLocalHeroInfo().GetAccessoryPositionInfo(accessoryData.PartType);
		return SprkAccessories.Getter(getter, this);
	}

	public override void OnEquip()
	{
		base.OnEquip();
		if (_listener != null)
		{
			_listener.OnClickEquipCostume(CreateCurrentAccessoryCostumeGetter());
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
		if (_listener != null)
		{
			_listener.OnClickBuyCostume(CreateCurrentAccessoryCostumeGetter(), delegate
			{
			});
		}
	}

	Vector3 DetailViewWidget.IListener.GetPostionVariation()
	{
		if (CurrentComponentAccesoryCostumeData == null)
		{
			VespaDebug.LogError("CurrentComponentAccesoryCostumeData == null");
			return Vector3.zero;
		}
		return CurrentComponentAccesoryCostumeData.GetCustomizingPivotData(base.HeroInfo.HeroIndex).GetPositionVariation();
	}

	Vector3 DetailViewWidget.IListener.GetRotationVariation()
	{
		if (CurrentComponentAccesoryCostumeData == null)
		{
			VespaDebug.LogError("CurrentComponentAccesoryCostumeData == null");
			return Vector3.zero;
		}
		return CurrentComponentAccesoryCostumeData.GetCustomizingPivotData(base.HeroInfo.HeroIndex).GetRoationVariation();
	}

	float DetailViewWidget.IListener.GetScaleVariation()
	{
		if (CurrentComponentAccesoryCostumeData == null)
		{
			VespaDebug.LogError("CurrentComponentAccesoryCostumeData == null");
			return 1f;
		}
		return CurrentComponentAccesoryCostumeData.GetCustomizingPivotData(base.HeroInfo.HeroIndex).ScaleVariation;
	}

	void DetailViewWidget.IListener.SetCostumeTransform(PositionInfo positionInfo)
	{
		if (CurrentComponentAccesoryCostumeData == null)
		{
			VespaDebug.LogError("CurrentComponentAccesoryCostumeData == null");
		}
		int index = CurrentComponentAccesoryCostumeData.Index;
		_cachedAccesssoryIndexPositionInfoPair[index] = positionInfo;
		_renderFacadeController.GetLocalHeroInfo().SetAccessoryCostumePositionInfo(CurrentComponentAccesoryCostumeData.PartType, positionInfo);
		SprkAccessories.EditPosition(this, positionInfo);
	}

	void DetailViewWidget.IListener.OnBuyCostume(Action onComplete)
	{
		if (CurrentComponentAccesoryCostumeData == null)
		{
			VespaDebug.LogError("CurrentComponentAccesoryCostumeData == null");
		}
		if (_listener != null)
		{
			_listener.OnClickBuyCostume(CreateCurrentAccessoryCostumeGetter(), onComplete);
		}
	}

	HeroInfo DetailViewWidget.IListener.GetHeroInfo()
	{
		return base.HeroInfo;
	}

	void DetailViewWidget.IListener.CloseDetaliView()
	{
		_renderFacadeController.OnCloseDetailView();
	}

	AccessoryCostumeData DetailViewWidget.IListener.GetAccessoryCostumeData()
	{
		return CurrentComponentAccesoryCostumeData;
	}
}

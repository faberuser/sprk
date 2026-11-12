using System;
using System.Collections.Generic;
using System.Linq;
using NGame2.NAccount;
using NGame2.NDropBonusEvent;
using NGame2.NPayShopItemEvent;
using NGame2.NUI.NComponent2.NCostume;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow.NHeroCostumeView;
using NGame2.NUtil.NCostume;
using NShared;
using NShared.NLocalization;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow.NCustomizing;

public class HeroCostumeViewWidget : WidgetBase, HeroCostumeComponent.Listener
{
	public interface IListener
	{
		void OnClickEquipCostume(HeroCostumeViewWidget caller);

		void OnClickUnequipCostume(HeroCostumeViewWidget caller);

		void OnClickBuyCostume(HeroCostumeViewWidget caller);

		void OnClickBuyHero(HeroCostumeViewWidget caller);

		void OnClickSubStory(HeroCostumeViewWidget caller);
	}

	[Serializable]
	public class CostumeButtonInfo
	{
		public CostumeState State;

		public UIButton Button;
	}

	public UIToggle Toggle_NormalCostume;

	public UIToggle Toggle_LegendCostume;

	public UIBaseGrid CostumeGrid;

	public DisplayAllCostumeAbilities DisplayAllCostumeAbilities;

	public DisplayLegendCostumeAbilities DisplayLegendCostumeAbilities;

	public UIWidget Widget_NormalBooster;

	public UIWidget Widget_LegendBooster;

	private IListener _listener;

	private List<HeroCostumeComponent> _costumeComponentList = new List<HeroCostumeComponent>();

	public CostumeData CurrentCostumeData { get; private set; }

	public EHeroCostumeType HeroCostumeTabType { get; private set; }

	void HeroCostumeComponent.Listener.OnClick(HeroCostumeComponent component)
	{
		CostumeData costumeData = component.CostumeData;
		SelectCostume(costumeData);
	}

	void HeroCostumeComponent.Listener.OnClickChangeCos(int costumeIndex)
	{
		CostumeData data = GameRoot.Instance.CostumeDataContainer.GetData(costumeIndex);
		ChangeCostume(data);
	}

	private void ClearCostumeComponentList()
	{
		FactoryBase.ReleaseAndClearList(_costumeComponentList);
	}

	private void Open()
	{
		if (CurrentCostumeData != null)
		{
			SetCostumeName(CurrentCostumeData.CostumeName);
			UpdateCostumeListState();
			UpdateButtonState();
			UpdateCurrentCostumeStatus();
			InitCurrentCostumeAbility();
			DisplayAllCostumeAbilities.Refresh();
			DisplayLegendCostumeAbilities.Refresh();
			SetBoosterWidget();
		}
		else
		{
			HeroCostumeTabType = EHeroCostumeType.Normal;
			SetToggle(EHeroCostumeType.Normal);
			InitCostumeList();
			InitHeroCostumeInfo();
			DisplayAllCostumeAbilities.Refresh();
			DisplayLegendCostumeAbilities.Refresh();
			SetBoosterWidget();
		}
	}

	private void InitCostumeList()
	{
		if (base.HeroInfo != null)
		{
			ClearCostumeComponentList();
			CreateCostumeList(base.HeroInfo, CostumeGrid, _costumeComponentList, this);
			UpdateCostumeListState();
		}
	}

	private void SetToggle(EHeroCostumeType heroCostumeType)
	{
		Toggle_NormalCostume.Set(heroCostumeType == EHeroCostumeType.Normal);
		Toggle_LegendCostume.Set(heroCostumeType == EHeroCostumeType.Legend);
	}

	private void CreateCostumeList(HeroInfo heroInfo, UIBaseGrid CostumeGrid, List<HeroCostumeComponent> costumeComponentList, HeroCostumeComponent.Listener listener)
	{
		if (heroInfo == null)
		{
			return;
		}
		foreach (CostumeData allDatum in GameRoot.Instance.CostumeDataContainer.GetAllData())
		{
			if (allDatum.HeroIndex == heroInfo.HeroIndex && allDatum.IsOpen && HeroCostumeTabType == allDatum.CostumeType && IsShowCostume(allDatum, heroInfo) && (!allDatum.PreviewableWhenOwned || GlobalAutoBehaviour<AccountManager>.instance.CostumeManager.Get(allDatum.CostumeIndex) != null))
			{
				HeroCostumeComponent heroCostumeComponent = CostumeGrid.AddGridItem(HeroCostumeComponent.PrefabAuto);
				heroCostumeComponent.Open(allDatum, listener);
				costumeComponentList.Add(heroCostumeComponent);
			}
		}
		if (HeroCostumeTabType == EHeroCostumeType.Legend && __Extension_Method_ICollection__.COUNT(costumeComponentList) == 0)
		{
			HeroCostumeComponent heroCostumeComponent2 = CostumeGrid.AddGridItem(HeroCostumeComponent.PrefabAuto);
			heroCostumeComponent2.Open(null, listener, existProduct: false);
			costumeComponentList.Add(heroCostumeComponent2);
		}
		CostumeGrid.RepositionGrid();
	}

	private bool IsShowCostume(CostumeData costumeData, HeroInfo heroInfo)
	{
		if (costumeData == null)
		{
			return false;
		}
		if (costumeData.CostumeIndex == 180 || costumeData.CostumeIndex == 280 || costumeData.CostumeIndex == 380 || costumeData.CostumeIndex == 480 || costumeData.CostumeIndex == 580 || costumeData.CostumeIndex == 680 || costumeData.CostumeIndex == 780 || costumeData.CostumeIndex == 880 || costumeData.CostumeIndex == 980 || costumeData.CostumeIndex == 1080 || costumeData.CostumeIndex == 1180 || costumeData.CostumeIndex == 1280 || costumeData.CostumeIndex == 1380 || costumeData.CostumeIndex == 1580 || costumeData.CostumeIndex == 1680 || costumeData.CostumeIndex == 1780 || costumeData.CostumeIndex == 1880 || costumeData.CostumeIndex == 1980 || costumeData.CostumeIndex == 2080 || costumeData.CostumeIndex == 2180 || costumeData.CostumeIndex == 2280 || costumeData.CostumeIndex == 2380 || costumeData.CostumeIndex == 2480 || costumeData.CostumeIndex == 2580 || costumeData.CostumeIndex == 2680 || costumeData.CostumeIndex == 2780 || costumeData.CostumeIndex == 2880 || costumeData.CostumeIndex == 2980 || costumeData.CostumeIndex == 3018 || costumeData.CostumeIndex == 3180 || costumeData.CostumeIndex == 3280 || costumeData.CostumeIndex == 3380 || costumeData.CostumeIndex == 3417 || costumeData.CostumeIndex == 3580 || costumeData.CostumeIndex == 3680 || costumeData.CostumeIndex == 3780 || costumeData.CostumeIndex == 3880 || costumeData.CostumeIndex == 3980 || costumeData.CostumeIndex == 4080 || costumeData.CostumeIndex == 4180 || costumeData.CostumeIndex == 4280 || costumeData.CostumeIndex == 4380 || costumeData.CostumeIndex == 4480 || costumeData.CostumeIndex == 4514 || costumeData.CostumeIndex == 4680 || costumeData.CostumeIndex == 4780 || costumeData.CostumeIndex == 4880 || costumeData.CostumeIndex == 4980 || costumeData.CostumeIndex == 5080 || costumeData.CostumeIndex == 5112 || costumeData.CostumeIndex == 5280 || costumeData.CostumeIndex == 5380 || costumeData.CostumeIndex == 5414 || costumeData.CostumeIndex == 5580 || costumeData.CostumeIndex == 5680 || costumeData.CostumeIndex == 5780 || costumeData.CostumeIndex == 5880 || costumeData.CostumeIndex == 5980 || costumeData.CostumeIndex == 6080 || costumeData.CostumeIndex == 6180 || costumeData.CostumeIndex == 6280 || costumeData.CostumeIndex == 6380 || costumeData.CostumeIndex == 6480 || costumeData.CostumeIndex == 6580 || costumeData.CostumeIndex == 6680 || costumeData.CostumeIndex == 6780 || costumeData.CostumeIndex == 6880 || costumeData.CostumeIndex == 6980 || costumeData.CostumeIndex == 7080 || costumeData.CostumeIndex == 7180 || costumeData.CostumeIndex == 7280 || costumeData.CostumeIndex == 7380 || costumeData.CostumeIndex == 7480 || costumeData.CostumeIndex == 7580 || costumeData.CostumeIndex == 7680 || costumeData.CostumeIndex == 7780 || costumeData.CostumeIndex == 7880 || costumeData.CostumeIndex == 7980 || costumeData.CostumeIndex == 8080 || costumeData.CostumeIndex == 8180 || costumeData.CostumeIndex == 8280 || costumeData.CostumeIndex == 8390 || costumeData.CostumeIndex == 8490 || costumeData.CostumeIndex == 8590 || costumeData.CostumeIndex == 8680 || costumeData.CostumeIndex == 8780 || costumeData.CostumeIndex == 8880 || costumeData.CostumeIndex == 8980 || costumeData.CostumeIndex == 9080 || costumeData.CostumeIndex == 9180 || costumeData.CostumeIndex == 9280 || costumeData.CostumeIndex == 9380 || costumeData.CostumeIndex == 9480 || costumeData.CostumeIndex == 9580 || costumeData.CostumeIndex == 9610 || costumeData.CostumeIndex == 9780 || costumeData.CostumeIndex == 9880 || costumeData.CostumeIndex == 9980 || costumeData.CostumeIndex == 10080 || costumeData.CostumeIndex == 10180 || costumeData.CostumeIndex == 10280 || costumeData.CostumeIndex == 11106 || costumeData.CostumeIndex == 190 || costumeData.CostumeIndex == 290 || costumeData.CostumeIndex == 390 || costumeData.CostumeIndex == 490 || costumeData.CostumeIndex == 590 || costumeData.CostumeIndex == 690 || costumeData.CostumeIndex == 790 || costumeData.CostumeIndex == 890 || costumeData.CostumeIndex == 990 || costumeData.CostumeIndex == 1090 || costumeData.CostumeIndex == 1190 || costumeData.CostumeIndex == 1290 || costumeData.CostumeIndex == 1390 || costumeData.CostumeIndex == 1590 || costumeData.CostumeIndex == 1690 || costumeData.CostumeIndex == 1790 || costumeData.CostumeIndex == 1890 || costumeData.CostumeIndex == 1990 || costumeData.CostumeIndex == 2090 || costumeData.CostumeIndex == 2190 || costumeData.CostumeIndex == 2290 || costumeData.CostumeIndex == 2390 || costumeData.CostumeIndex == 2490 || costumeData.CostumeIndex == 2590 || costumeData.CostumeIndex == 2690 || costumeData.CostumeIndex == 2790 || costumeData.CostumeIndex == 2890 || costumeData.CostumeIndex == 2990 || costumeData.CostumeIndex == 3090 || costumeData.CostumeIndex == 3190 || costumeData.CostumeIndex == 3290 || costumeData.CostumeIndex == 3390 || costumeData.CostumeIndex == 3490 || costumeData.CostumeIndex == 3590 || costumeData.CostumeIndex == 3690 || costumeData.CostumeIndex == 3790 || costumeData.CostumeIndex == 3890 || costumeData.CostumeIndex == 3990 || costumeData.CostumeIndex == 4090 || costumeData.CostumeIndex == 4190 || costumeData.CostumeIndex == 4290 || costumeData.CostumeIndex == 4390 || costumeData.CostumeIndex == 4490 || costumeData.CostumeIndex == 4590 || costumeData.CostumeIndex == 4690 || costumeData.CostumeIndex == 4790 || costumeData.CostumeIndex == 4890 || costumeData.CostumeIndex == 4990 || costumeData.CostumeIndex == 5090 || costumeData.CostumeIndex == 5190 || costumeData.CostumeIndex == 5290 || costumeData.CostumeIndex == 5390 || costumeData.CostumeIndex == 5490 || costumeData.CostumeIndex == 5590 || costumeData.CostumeIndex == 5690 || costumeData.CostumeIndex == 5790 || costumeData.CostumeIndex == 5890 || costumeData.CostumeIndex == 5990 || costumeData.CostumeIndex == 6090 || costumeData.CostumeIndex == 6190 || costumeData.CostumeIndex == 6290 || costumeData.CostumeIndex == 6390 || costumeData.CostumeIndex == 6490 || costumeData.CostumeIndex == 6590 || costumeData.CostumeIndex == 6690 || costumeData.CostumeIndex == 6790 || costumeData.CostumeIndex == 6890 || costumeData.CostumeIndex == 6990 || costumeData.CostumeIndex == 7090 || costumeData.CostumeIndex == 7190 || costumeData.CostumeIndex == 7290 || costumeData.CostumeIndex == 7390 || costumeData.CostumeIndex == 7490 || costumeData.CostumeIndex == 7590 || costumeData.CostumeIndex == 7690 || costumeData.CostumeIndex == 7790 || costumeData.CostumeIndex == 7890 || costumeData.CostumeIndex == 7990 || costumeData.CostumeIndex == 8090 || costumeData.CostumeIndex == 8190 || costumeData.CostumeIndex == 8290 || costumeData.CostumeIndex == 8690 || costumeData.CostumeIndex == 8790 || costumeData.CostumeIndex == 8890 || costumeData.CostumeIndex == 8990 || costumeData.CostumeIndex == 9090 || costumeData.CostumeIndex == 9190 || costumeData.CostumeIndex == 9290 || costumeData.CostumeIndex == 9390 || costumeData.CostumeIndex == 9490 || costumeData.CostumeIndex == 9590 || costumeData.CostumeIndex == 9690 || costumeData.CostumeIndex == 9790 || costumeData.CostumeIndex == 9890 || costumeData.CostumeIndex == 9990 || costumeData.CostumeIndex == 10090 || costumeData.CostumeIndex == 10190 || costumeData.CostumeIndex == 10290 || costumeData.CostumeIndex == 11190)
		{
			return GlobalAutoBehaviour<AccountManager>.instance.CostumeManager.Get(costumeData.CostumeIndex) != null;
		}
		if (!costumeData.IsCoupleCostume())
		{
			return true;
		}
		bool num = GlobalAutoBehaviour<AccountManager>.instance.CostumeManager.Get(costumeData.CostumeIndex) != null;
		int costumeIndex = heroInfo.CostumeIndex;
		bool flag = num;
		if (!flag)
		{
			List<CostumeData> costumeDataListByHeroStar = GameRoot.Instance.CostumeDataContainer.GetCostumeDataListByHeroStar(heroInfo.HeroIndex, heroInfo.Star, isFreeCostume: true);
			for (int i = 0; i < costumeDataListByHeroStar.Count; i++)
			{
				if (costumeDataListByHeroStar[i].CostumeIndex == costumeData.CostumeIndex)
				{
					flag = true;
				}
			}
		}
		if (costumeData.IsBonusCostume())
		{
			if (flag)
			{
				return costumeIndex == costumeData.CostumeIndex;
			}
			return false;
		}
		int[] bonusCostumeIndices = costumeData.BonusCostumeIndices;
		foreach (int num2 in bonusCostumeIndices)
		{
			if (costumeIndex == num2)
			{
				return false;
			}
		}
		return true;
	}

	private void UpdateCostumeListState()
	{
		if (base.HeroInfo == null)
		{
			return;
		}
		int costumeIndex = base.HeroInfo.CostumeIndex;
		CostumeData equippedCostumeData = ((costumeIndex != 0) ? GameRoot.Instance.CostumeDataContainer.GetData(costumeIndex) : null);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_costumeComponentList); i++)
		{
			HeroCostumeComponent heroCostumeComponent = _costumeComponentList[i];
			CostumeData costumeData = heroCostumeComponent.CostumeData;
			if (costumeData != null)
			{
				CostumeState costumeState = HeroCostume.GetCostumeState(equippedCostumeData, costumeData, checkHeroOwn: false);
				if (costumeState == CostumeState.UnEquipSelect)
				{
					costumeState = CostumeState.Equipped;
				}
				heroCostumeComponent.SetState(costumeState);
			}
		}
	}

	private void UpdateCostumeListPrice()
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_costumeComponentList); i++)
		{
			HeroCostumeComponent heroCostumeComponent = _costumeComponentList[i];
			if (heroCostumeComponent.CostumeData != null)
			{
				heroCostumeComponent.SetCostumePrice();
			}
		}
	}

	private void UpdateCostumeListSelectDeselect(CostumeData selectCostumeData, CostumeData DeselectCostumeData, List<HeroCostumeComponent> costumeComponentList)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(costumeComponentList); i++)
		{
			CostumeData costumeData = costumeComponentList[i].CostumeData;
			if (Vespa_Common_Class_Extension_Method.EQUALS(costumeData, DeselectCostumeData))
			{
				costumeComponentList[i].SetSelected(Selected: false);
			}
			if (Vespa_Common_Class_Extension_Method.EQUALS(costumeData, selectCostumeData))
			{
				costumeComponentList[i].SetSelected(Selected: true);
			}
		}
	}

	private void InitCoupleCostumeComponent(CostumeData selectCostumeData, List<HeroCostumeComponent> costumeComponentList)
	{
		if (selectCostumeData == null || (selectCostumeData != null && !selectCostumeData.IsCoupleCostume()))
		{
			return;
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(costumeComponentList); i++)
		{
			CostumeData costumeData = costumeComponentList[i].CostumeData;
			if (costumeData != null && Array.Exists(selectCostumeData.BonusCostumeIndices, (int bonusCostumeIndex) => bonusCostumeIndex == costumeData.CostumeIndex))
			{
				costumeComponentList[i].ChangeCostumeData(selectCostumeData);
				break;
			}
		}
	}

	private void InitHeroCostumeInfo()
	{
		if (base.HeroInfo != null)
		{
			CostumeData costumeData = GameRoot.Instance.CostumeDataContainer.GetData(base.HeroInfo.CostumeIndex);
			if (costumeData == null)
			{
				costumeData = GameRoot.Instance.CostumeDataContainer.GetDataByHeroStar(base.HeroInfo.HeroIndex, base.HeroInfo.Star);
			}
			SelectCostume(costumeData);
		}
	}

	public void SelectCostume(CostumeData costumeData)
	{
		if (costumeData != null)
		{
			if (costumeData.CostumeType == EHeroCostumeType.Legend)
			{
				CheckBasicHairWeaponCostume();
			}
			CostumeData currentCostumeData = CurrentCostumeData;
			CurrentCostumeData = costumeData;
			if (HeroCostumeTabType != CurrentCostumeData.CostumeType)
			{
				SetToggle(CurrentCostumeData.CostumeType);
				UpdateByCostumeType(CurrentCostumeData.CostumeType);
			}
			CachedHeroInfo localHeroInfo = _renderFacadeController.GetLocalHeroInfo();
			localHeroInfo.CostumeIndex = ((CurrentCostumeData != null) ? CurrentCostumeData.CostumeIndex : 0);
			_renderFacadeController.ChangeRenderFacadeOnly(localHeroInfo);
			InitCoupleCostumeComponent(CurrentCostumeData, _costumeComponentList);
			UpdateCostumeListSelectDeselect(CurrentCostumeData, currentCostumeData, _costumeComponentList);
			string costumeName = ((CurrentCostumeData == null) ? "(empty)" : CurrentCostumeData.CostumeName);
			SetCostumeName(costumeName);
			UpdateCostumeListState();
			UpdateButtonState();
			UpdateCurrentCostumeStatus();
			InitCurrentCostumeAbility();
			SetBoosterWidget();
			_renderFacadeController.OnSelectCostume();
			_renderFacadeController.OnSignatureMotion();
		}
	}

	private void CheckBasicHairWeaponCostume()
	{
		CachedHeroInfo localHeroInfo = _renderFacadeController.GetLocalHeroInfo();
		if (localHeroInfo.HairCostumeIndex != 0)
		{
			HairCostumeViewWidget hairCostumeViewWidget = _widgetUiController.GetCostumeViewWidget(ToggleType.Hair) as HairCostumeViewWidget;
			if (hairCostumeViewWidget == null)
			{
				return;
			}
			hairCostumeViewWidget.SelectCostume(HairCostumeComponent.OpenData.CreateSelectThema());
		}
		if (localHeroInfo.HideUniqueWeapon == 0 || localHeroInfo.WeaponCostumeIndex != 0)
		{
			WeaponCostumeViewWidget weaponCostumeViewWidget = _widgetUiController.GetCostumeViewWidget(ToggleType.Weapon) as WeaponCostumeViewWidget;
			if (!(weaponCostumeViewWidget == null))
			{
				weaponCostumeViewWidget.SelectCostume(WeaponCostumeComponent.OpenData.CreateSelectThema());
			}
		}
	}

	private void ChangeCostume(CostumeData costumeData)
	{
		if (costumeData == null)
		{
			return;
		}
		CostumeData currentCostumeData = CurrentCostumeData;
		CurrentCostumeData = costumeData;
		CachedHeroInfo localHeroInfo = _renderFacadeController.GetLocalHeroInfo();
		localHeroInfo.CostumeIndex = ((CurrentCostumeData != null) ? CurrentCostumeData.CostumeIndex : 0);
		_renderFacadeController.ChangeRenderFacadeOnly(localHeroInfo);
		string costumeName = ((CurrentCostumeData == null) ? "(empty)" : CurrentCostumeData.CostumeName);
		SetCostumeName(costumeName);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_costumeComponentList); i++)
		{
			if (Vespa_Common_Class_Extension_Method.EQUALS(_costumeComponentList[i].CostumeData, currentCostumeData))
			{
				_costumeComponentList[i].ChangeCostumeData(CurrentCostumeData);
			}
		}
		UpdateCostumeListState();
		UpdateButtonState();
		UpdateCurrentCostumeStatus();
		InitCurrentCostumeAbility();
		_renderFacadeController.OnSelectCostume();
		_renderFacadeController.OnSignatureMotion();
	}

	private void SetCostumeName(string CostumeName)
	{
		UIHelper.EnableGameObject(base.Label_CostumeName, enable: true);
		UIHelper.SetLabelText(base.Label_CostumeName, CostumeName);
		UIHelper.SetLabelText(base.Label_CostumeNameOutline, CostumeName);
	}

	private void UpdateButtonState()
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(base.ButtonInfos); i++)
		{
			UIHelper.EnableGameObject(base.ButtonInfos[i].Button, enable: false);
		}
		if (base.HeroInfo == null || CurrentCostumeData == null)
		{
			return;
		}
		int costumeIndex = base.HeroInfo.CostumeIndex;
		CostumeData data = GameRoot.Instance.CostumeDataContainer.GetData(costumeIndex);
		CostumeState costumeState = HeroCostume.GetCostumeState(data, CurrentCostumeData);
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
		else if (costumeState == CostumeState.Owned)
		{
			if (costumeIndex == CurrentCostumeData.CostumeIndex)
			{
				enable = false;
			}
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
		else if (costumeState == CostumeState.NeedSubStory)
		{
			enable = true;
		}
		else if (costumeState == CostumeState.LegendNotBuyable)
		{
			enable = false;
		}
		else if (costumeState == CostumeState.LegendBuyable)
		{
			enable = true;
		}
		else if (costumeState == CostumeState.Buyable)
		{
			UIHelper.SetLabelText(costumeButtonInfo.Button.GetComponentInChildren<UILabel>(), LocalizationManager.GetStringDirect(LocaleKeyBuyButton));
		}
		UIHelper.EnableButton(costumeButtonInfo.Button, enable);
	}

	private void UpdateCurrentCostumeStatus()
	{
		UIHelper.EnableGameObject(base.WidgetNeedCostume, enable: false);
		int costumeIndex = base.HeroInfo.CostumeIndex;
		CostumeState costumeState = HeroCostume.GetCostumeState(GameRoot.Instance.CostumeDataContainer.GetData(costumeIndex), CurrentCostumeData, checkHeroOwn: false);
		UIHelper.EnableGameObject(base.WidgetObtainDesc, costumeState == CostumeState.NeedAwaken || costumeState == CostumeState.CannotBuy || costumeState == CostumeState.NeedSubStory || costumeState == CostumeState.LegendBuyable);
		UIHelper.EnableGameObject(base.WidgetPriceDesc, costumeState == CostumeState.Buyable);
		UIHelper.EnableGameObject(base.WidgetMileageDesc, costumeState == CostumeState.Buyable);
		UIHelper.EnableGameObject(base.WidgetOwned, costumeState == CostumeState.Owned);
		bool enable = costumeState == CostumeState.Equipped || (base.HeroInfo.CostumeIndex == 0 && costumeState == CostumeState.UnEquipSelect);
		UIHelper.EnableGameObject(base.WidgetEquipped, enable);
		switch (costumeState)
		{
		case CostumeState.NeedAwaken:
		case CostumeState.CannotBuy:
		case CostumeState.NeedSubStory:
		{
			string text = LocalizationManager.GetString(Costume.COSTUME_REQUIREMENT);
			string text2 = ((costumeState == CostumeState.NeedSubStory) ? LocalizationManager.GetStringDirect("COSTUME_OBTAIN_DESC_DETAIL_SUBSTORY") : CurrentCostumeData.ObtainDescription);
			UIHelper.SetLabelText(base.Label_Requirement, text + ":" + text2);
			break;
		}
		case CostumeState.LegendBuyable:
			UIHelper.SetLabelText(base.Label_Requirement, LocalizationManager.GetStringDirect("COSTUME_LEGEND_PRE_ORDER"));
			break;
		case CostumeState.Buyable:
		{
			int priceGem = HeroCostume.GetPriceGem(CurrentCostumeData);
			long i_value = priceGem;
			int obtainMileage = HeroCostume.GetObtainMileage(CurrentCostumeData);
			long i_value2 = obtainMileage;
			int priceDiscountPercentage = BaseCostume.GetPriceDiscountPercentage(CurrentCostumeData);
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
			break;
		}
		}
	}

	private void InitCurrentCostumeAbility()
	{
		UIHelper.EnableGameObject(base.Label_AbilityDesc, enable: false);
		if (CurrentCostumeData == null)
		{
			return;
		}
		List<CostumeAbilityInfo> costumeAbilities = CurrentCostumeData.GetCostumeAbilities();
		if (__Extension_Method_ICollection__.COUNT(costumeAbilities) == 0)
		{
			if (!CurrentCostumeData.IsCoupleCostume() || !CurrentCostumeData.IsBonusCostume())
			{
				return;
			}
			costumeAbilities = GameRoot.Instance.CostumeDataContainer.GetData(CurrentCostumeData.MainIndex).GetCostumeAbilities();
			if (__Extension_Method_ICollection__.COUNT(costumeAbilities) == 0)
			{
				return;
			}
		}
		int num = __Extension_Method_ICollection__.COUNT(costumeAbilities);
		string text = "";
		for (int i = 0; i < num; i++)
		{
			CostumeAbilityInfo costumeAbilityInfo = costumeAbilities[i];
			if (costumeAbilityInfo.Type == CostumeAbilityType.Booster)
			{
				string valueString = ArgParser.GetValueString(costumeAbilityInfo.Value, 0);
				int valueInt = ArgParser.GetValueInt(costumeAbilityInfo.Value, 1);
				BoosterDefineData data = GameRoot.Instance.BoosterDefineDataContainer.GetData(valueString);
				if (data != null && valueInt > 0)
				{
					string stringDirect = LocalizationManager.GetStringDirect(data.TooltipDesc, valueInt);
					text = ((!String_Extension_Method.IsNullOrEmpty(text)) ? $"{text} / {stringDirect}" : stringDirect);
				}
			}
		}
		if (!String_Extension_Method.IsNullOrEmpty(text))
		{
			UIHelper.EnableGameObject(base.Label_AbilityDesc, enable: true);
			UIHelper.SetLabelText(base.Label_AbilityDesc, text);
		}
	}

	public void SetListener(IListener listener)
	{
		_listener = listener;
	}

	private void SetBoosterWidget()
	{
		bool flag = CurrentCostumeData != null && CurrentCostumeData.CostumeType == EHeroCostumeType.Legend;
		int effectiveLegendCostumeBoosterCount = GlobalAutoBehaviour<BoosterManager>.instance.GetEffectiveLegendCostumeBoosterCount();
		UIHelper.EnableGameObject(Widget_LegendBooster, flag && effectiveLegendCostumeBoosterCount > 0);
		UIHelper.EnableGameObject(Widget_NormalBooster, !flag);
	}

	public override void OnChanged(PayShopItemEventManager.EventChangedInfo info)
	{
		UpdateCostumeListPrice();
		UpdateCostumeListState();
		InitCurrentCostumeAbility();
		UpdateButtonState();
		UpdateCurrentCostumeStatus();
	}

	public override void OnChanged(NGame2.NAccount.UserHeroManager.ChangeHeroInfo changeHeroInfo)
	{
		if (changeHeroInfo.ChangeType == NGame2.NAccount.UserHeroManager.HeroChangeType.Costume)
		{
			if (changeHeroInfo.NewValue == 0)
			{
				CostumeData dataByHeroStar = GameRoot.Instance.CostumeDataContainer.GetDataByHeroStar(base.HeroInfo.HeroIndex, base.HeroInfo.Star);
				SelectCostume(dataByHeroStar);
			}
			UpdateCostumeListState();
			UpdateButtonState();
			DisplayAllCostumeAbilities.Refresh();
			UpdateCurrentCostumeStatus();
		}
	}

	public override void OnChanged(DropBonusEventManager.DropBonusEventChangeInfo dropBonusEventChangeInfo)
	{
		UpdateCostumeListPrice();
		UpdateCostumeListState();
		InitCurrentCostumeAbility();
		UpdateButtonState();
		UpdateCurrentCostumeStatus();
	}

	public void OnChangedCostumeType()
	{
		EHeroCostumeType heroCostumeType = EHeroCostumeType.None;
		if (Toggle_NormalCostume.value)
		{
			heroCostumeType = EHeroCostumeType.Normal;
		}
		else if (Toggle_LegendCostume.value)
		{
			heroCostumeType = EHeroCostumeType.Legend;
		}
		UpdateByCostumeType(heroCostumeType);
	}

	private void UpdateByCostumeType(EHeroCostumeType heroCostumeType)
	{
		if (heroCostumeType != EHeroCostumeType.None && HeroCostumeTabType != heroCostumeType)
		{
			HeroCostumeTabType = heroCostumeType;
			InitCostumeList();
			UpdateCostumeListSelectDeselect(CurrentCostumeData, null, _costumeComponentList);
		}
	}

	public override void Open(ToggleType toggleType)
	{
		base.Open(toggleType);
		Open();
	}

	public override void Reset()
	{
		CurrentCostumeData = null;
		HeroCostumeTabType = EHeroCostumeType.None;
		ClearCostumeComponentList();
		DisplayAllCostumeAbilities.Reset();
		DisplayLegendCostumeAbilities.Reset();
	}

	public void OnClickLegendBoosterTooltip()
	{
		LegendCostumeTotalBuffPopup.OpenPopup();
	}

	public override void OnUnEquip()
	{
		base.OnUnEquip();
		_listener?.OnClickUnequipCostume(this);
	}

	public override void OnEquip()
	{
		base.OnEquip();
		IListener listener = _listener;
		if (listener == null)
		{
			return;
		}
		bool flag = CheckUnsetLegendCostume();
		CostumeData dataByHeroStar = GameRoot.Instance.CostumeDataContainer.GetDataByHeroStar(base.HeroInfo.HeroIndex, base.HeroInfo.Star);
		if (base.HeroInfo.CostumeIndex != 0 && Vespa_Common_Class_Extension_Method.EQUALS(dataByHeroStar, CurrentCostumeData))
		{
			if (flag)
			{
				OpenUnEquipLegendCostumePopup(delegate
				{
					listener.OnClickUnequipCostume(this);
				});
			}
			else
			{
				listener.OnClickUnequipCostume(this);
			}
		}
		else if (flag)
		{
			OpenUnEquipLegendCostumePopup(delegate
			{
				listener.OnClickEquipCostume(this);
			});
		}
		else
		{
			listener.OnClickEquipCostume(this);
		}
	}

	private void OpenUnEquipLegendCostumePopup(Action onYes)
	{
		string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
		string stringDirect = LocalizationManager.GetStringDirect("COSTUME_LEGEND_UNEQUIP_NOTICE_DRESSROOM_APPLY");
		MessagePopup3Button.Open(title, stringDirect, null, onYes, NShared.NLocalization.Common.COMMON_NO, NShared.NLocalization.Common.COMMON_YES);
	}

	private bool CheckUnsetLegendCostume()
	{
		CostumeData data = GameRoot.Instance.CostumeDataContainer.GetData(base.HeroInfo.CostumeIndex);
		if (data != null && CurrentCostumeData != null && data.CostumeType == EHeroCostumeType.Legend && CurrentCostumeData.CostumeType != EHeroCostumeType.Legend)
		{
			return true;
		}
		return false;
	}

	public override void OnBuyHero()
	{
		base.OnBuyHero();
		_listener?.OnClickBuyHero(this);
	}

	public override void OnBuyCostume()
	{
		base.OnBuyCostume();
		IListener listener = _listener;
		if (listener == null)
		{
			return;
		}
		CostumeData data = GameRoot.Instance.CostumeDataContainer.GetData(base.HeroInfo.CostumeIndex);
		if (data != null && CurrentCostumeData != null && data.CostumeType == EHeroCostumeType.Legend && CurrentCostumeData.CostumeType != EHeroCostumeType.Legend)
		{
			string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
			string stringDirect = LocalizationManager.GetStringDirect("COSTUME_LEGEND_UNEQUIP_NOTICE_DRESSROOM_PURCHASE");
			MessagePopup3Button.Open(title, stringDirect, null, delegate
			{
				listener.OnClickBuyCostume(this);
			}, NShared.NLocalization.Common.COMMON_NO, NShared.NLocalization.Common.COMMON_YES);
		}
		else
		{
			listener.OnClickBuyCostume(this);
		}
	}

	public override void OnSubStory()
	{
		base.OnSubStory();
		_listener?.OnClickSubStory(this);
	}
}

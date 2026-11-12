using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NContentsInfo;
using NGame2.NEventTrigger;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NComponent2.NStat;
using NGame2.NUI.NManager;
using NShared;
using NShared.NEvent;
using NVespa;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class ValanceTierUpgradeWindow : ValanceEnhanceBase, IUserObserver<ItemInfo>, IUserObserver, IUserObserver<UserManager.UserStatInfo>, IUserObserver<NGame2.NAccount.UserEquipItemManager.EquipStatInfo>
{
	public interface IListener
	{
		void OnClickTierUpgrade(int slotIndex);
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Valance/ValanceTierUpgradeWindow";

	public Transform ResultEquipItemRoot;

	public Transform MaterialEquipItemRoot;

	public UILabel LabelReqMaterialCount;

	public UILabel LabelReqGold;

	public UIButton Button_TierUpgrade;

	public UIPanel StatChangePanel;

	public UIScrollView StatChangeScrollView;

	public UIWidget IconProxyRoot;

	public Color ColorGoldEnough = Color.white;

	public Color ColorGoldNotEnough = Color.red;

	private IListener _listener;

	private ItemIconComponent _resultComp;

	private ItemIconComponent _materialComp;

	private StatListComponent _statListComponent;

	private IconProxy _iconProxy;

	public EquipUpgradeTierData EquipUpgradeTierData { get; private set; }

	public StatData BeforeStat { get; private set; }

	public StatData AfterStat { get; private set; }

	public override bool InvenDefaultFilter(EquipItemInfo info)
	{
		if (info.GetInventoryType() != InventoryType.Inventory)
		{
			return false;
		}
		if (info.GetEquippedHeroIndex() != -1)
		{
			return false;
		}
		if (!Item_Equip_MaxStar.CanTechnoTierUpgrade(info))
		{
			return false;
		}
		return EquipItemFilter.Filter(info.EquipItemData);
	}

	public override void ItemDisabledAction(ItemIconComponent obj)
	{
		EquipItemInfo equipItemInfo = obj.EquipItemInfo;
		if (equipItemInfo != null)
		{
			bool flag = Item_Equip_MaxStar.CanTechnoTierUpgrade(equipItemInfo);
			obj.SetDisabled(!flag);
		}
	}

	bool IUserObserver.IsGlobal()
	{
		return false;
	}

	void IUserObserver<ItemInfo>.OnChanged(ItemInfo changedInfo)
	{
		if (changedInfo != null && !(TargetComponent == null) && EquipUpgradeTierData != null)
		{
			ItemData data = GameRoot.Instance.ItemDataContainer.GetData(changedInfo.ItemIndex);
			if (data != null && EquipUpgradeTierData.MaterialItemCode1 == data.Code)
			{
				SetupMaterialIcon();
				SetButton();
			}
		}
	}

	void IUserObserver<UserManager.UserStatInfo>.OnChanged(UserManager.UserStatInfo changedInfo)
	{
		if (!(TargetComponent == null) && changedInfo.UserStatType == UserManager.UserStatType.Gold)
		{
			SetReqGoldLabel();
			SetButton();
		}
	}

	void IUserObserver<NGame2.NAccount.UserEquipItemManager.EquipStatInfo>.OnChanged(NGame2.NAccount.UserEquipItemManager.EquipStatInfo changedInfo)
	{
		if (changedInfo == null || TargetComponent == null || base.EquipItemInfo == null)
		{
			return;
		}
		if (changedInfo.StatType == NGame2.NAccount.UserEquipItemManager.EquipStatType.Add)
		{
			EquipItemInfo equipItemInfo = changedInfo.EquipItemInfo;
			if (equipItemInfo != null && Item_Equip_MaxStar.CanTechnoTierUpgrade(equipItemInfo))
			{
				SetInvenItemList();
			}
		}
		else if (changedInfo.StatType == NGame2.NAccount.UserEquipItemManager.EquipStatType.Remove)
		{
			Refresh();
		}
	}

	public void Open(IListener listener)
	{
		_listener = listener;
		base.Open(delegate
		{
			Init();
		}, null);
	}

	public void OpenCustomCinema(EquipItemInfo itemInfo, IListener listener)
	{
		_listener = listener;
		OpenCustomCinema("ValanceTierUpgradeWindowInven", delegate
		{
			Init(itemInfo);
			ItemListPanel.ExternalClickItemIcon(itemInfo, isRefreshTab: true);
		}, null);
	}

	protected override void NodeTreeOpenOnStart()
	{
		base.NodeTreeOpenOnEnd();
		SceneAutoBehaviour<ValanceManagement>.instance.ManageOnOtherWindow();
	}

	protected override void NodeTreeCloseOnEnd()
	{
		base.NodeTreeCloseOnEnd();
		SceneAutoBehaviour<ValanceManagement>.instance.ManageReOpen();
	}

	public new void Init(EquipItemInfo equipItemInfo = null)
	{
		base.Init(equipItemInfo);
		SetIconProxy();
		SetReqGoldLabel();
		SetButton();
		GlobalAutoBehaviour<AccountManager>.instance.AddUserObserver(this);
		OpenValanceRestore triggerEvent = new OpenValanceRestore();
		SingletonAuto<EventTriggerManager>.instance.RaiseEvent(triggerEvent);
	}

	public override void SelectedTargetItem(EquipItemInfo itemInfo)
	{
		base.SelectedTargetItem(itemInfo);
		SetupResultIcon();
		SetupMaterialIcon();
		SetReqGoldLabel();
		SetItemStat();
		SetButton();
	}

	public void SetIconProxy()
	{
		if (!(IconProxyRoot == null))
		{
			if (_iconProxy == null)
			{
				_iconProxy = IconProxy.Create(IconProxyRoot);
			}
			if (_iconProxy != null)
			{
				_iconProxy.SetupCurrencyNoBg(CurrencyType.Gold);
			}
		}
	}

	private void SetupResultIcon()
	{
		if (base.EquipItemInfo != null)
		{
			if (_resultComp == null)
			{
				_resultComp = ItemIconComponent.PrefabAuto.Factory.Create(ResultEquipItemRoot);
				Vespa_Unity_Transform_Extension_Method.ResetLocalTM(_resultComp.transform);
			}
			EquipUpgradeTierData = GameRoot.Instance.EquipUpgradeTierDataContainer.GetData(base.EquipItemInfo.ItemIndex);
			EquipItemInfo resultItem = GetResultItem();
			BeforeStat = StatCalculator.GetEquipItemStatData(base.EquipItemInfo.ItemIndex, base.EquipItemInfo.Star, base.EquipItemInfo.Level);
			AfterStat = StatCalculator.GetEquipItemStatData(resultItem.ItemIndex, resultItem.Star, resultItem.Level);
			_resultComp.Open(resultItem, null);
		}
	}

	private void SetupMaterialIcon()
	{
		string text = ((EquipUpgradeTierData != null) ? EquipUpgradeTierData.MaterialItemCode1 : "");
		ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(text);
		if (dataByCode == null)
		{
			UIHelper.SetLabelText(LabelReqMaterialCount, "");
			return;
		}
		if (_materialComp == null)
		{
			_materialComp = ItemIconComponent.PrefabAuto.Factory.Create(MaterialEquipItemRoot);
			_materialComp.transform.localScale = Vector3.one;
			_materialComp.transform.localPosition = Vector3.zero;
		}
		_materialComp.Open(dataByCode);
		int itemCount = GlobalAutoBehaviour<AccountManager>.instance.ItemManager.GetItemCount(text);
		int materialItemCount = EquipUpgradeTierData.MaterialItemCount1;
		Color color = ((materialItemCount <= itemCount) ? ColorGoldEnough : ColorGoldNotEnough);
		string text2 = $"{itemCount} / {materialItemCount}";
		UIHelper.SetLabelColor(LabelReqMaterialCount, color);
		UIHelper.SetLabelText(LabelReqMaterialCount, text2);
	}

	public void SetReqGoldLabel()
	{
		if (TargetComponent == null)
		{
			UIHelper.SetLabelText(LabelReqGold, 0);
			UIHelper.SetLabelColor(LabelReqGold, ColorGoldEnough);
			return;
		}
		int num = ((EquipUpgradeTierData != null) ? EquipUpgradeTierData.CostValue : 0);
		long num2 = num;
		Color color = ((num2 <= GlobalAutoBehaviour<AccountManager>.instance.UserInfo.Gold) ? ColorGoldEnough : ColorGoldNotEnough);
		UIHelper.SetLabelColor(LabelReqGold, color);
		UIHelper.SetLabelText(LabelReqGold, Vespa_Int_Extension_Method.ToCommaString(num2));
	}

	public void SetItemStat()
	{
		if (StatChangeScrollView == null || StatChangePanel == null)
		{
			return;
		}
		if (_statListComponent != null)
		{
			ClearOptitonStat();
		}
		_statListComponent = StatListComponent.Create(StatChangePanel, 2, isValance: true);
		_statListComponent.AddStatChange(BeforeStat, AfterStat, all: false, StatListComponent.EStatChangeTemplate.ForStarUpgrade);
		if (base.EquipItemData != null)
		{
			for (int i = 1; base.EquipItemData.GetFixedOptionIndex(i) != 0; i++)
			{
			}
		}
		if ((bool)StatChangeScrollView)
		{
			StatChangeScrollView.ResetPosition();
		}
	}

	public void SetButton()
	{
		bool flag = TargetComponent != null;
		bool flag2 = IsEnoughGold();
		bool flag3 = IsEnoughMaterial();
		UIHelper.EnableButton(Button_TierUpgrade, flag && flag2 && flag3, grayscaleIfDisable: true);
	}

	private EquipItemInfo GetResultItem()
	{
		ItemData<EquipItemData> data = GameRoot.Instance.ItemDataContainer.GetData<EquipItemData>(EquipUpgradeTierData.ResultItemCode);
		EquipItemInfo equipItemInfo = base.EquipItemInfo.Clone();
		equipItemInfo.ItemIndex = data.Index;
		equipItemInfo.Star = 0;
		equipItemInfo.Level = 0;
		equipItemInfo.Exp = 0;
		equipItemInfo.UpgradeStarFailBonus = 0;
		equipItemInfo.Identified = 1;
		int randomOptionCount = GameRoot.Instance.EquipItemDetailDataContainer.GetRandomOptionCount(base.EquipItemData.DetailIndex);
		int randomOptionCount2 = GameRoot.Instance.EquipItemDetailDataContainer.GetRandomOptionCount(data.SubData.DetailIndex);
		int num = System.Math.Min(randomOptionCount, randomOptionCount2);
		short tier = base.EquipItemData.Tier;
		short tier2 = data.SubData.Tier;
		List<int> totalEquipOptionIndices = GameRoot.Instance.EquipOptionGroupDataContainer.GetTotalEquipOptionIndices(base.EquipItemData);
		List<int> totalEquipOptionIndices2 = GameRoot.Instance.EquipOptionGroupDataContainer.GetTotalEquipOptionIndices(data.SubData);
		for (int i = 1; i <= num; i++)
		{
			int slotIndex = i;
			int optionIndex = base.EquipItemInfo.GetOptionIndex(i);
			base.EquipItemInfo.GetOptionStep(i);
			int ChangeOptionIndex = GameRoot.Instance.EquipOptionChangeDataContainer.GetChageOptionIndex(optionIndex, tier, tier2);
			if (ChangeOptionIndex != 0 && totalEquipOptionIndices2.FindIndex((int x) => x == ChangeOptionIndex) != -1)
			{
				SetOption(equipItemInfo, slotIndex, ChangeOptionIndex);
				continue;
			}
			if (totalEquipOptionIndices.FindIndex((int x) => x == optionIndex) != -1)
			{
				SetOption(equipItemInfo, slotIndex, optionIndex);
				continue;
			}
			int index = UnityEngine.Random.Range(0, totalEquipOptionIndices2.Count);
			ChangeOptionIndex = totalEquipOptionIndices2[index];
			totalEquipOptionIndices2.RemoveAt(index);
			SetOption(equipItemInfo, slotIndex, ChangeOptionIndex);
		}
		return equipItemInfo;
	}

	private void SetOption(EquipItemInfo equipItemInfo, int slotIndex, int optionIndex)
	{
		switch (slotIndex)
		{
		case 1:
			equipItemInfo.OptionIndex1 = optionIndex;
			break;
		case 2:
			equipItemInfo.OptionIndex2 = optionIndex;
			break;
		case 3:
			equipItemInfo.OptionIndex3 = optionIndex;
			break;
		case 4:
			equipItemInfo.OptionIndex4 = optionIndex;
			break;
		case 5:
			equipItemInfo.OptionIndex5 = optionIndex;
			break;
		}
	}

	public override void Refresh()
	{
		ClearSeletedTarget();
		base.Refresh();
	}

	public override void OnClosing()
	{
		base.OnClosing();
		ReleaseIconProxy();
		GlobalAutoBehaviour<AccountManager>.instance.RemoveUserObserver(this);
	}

	public override void ClearSeletedTarget()
	{
		base.ClearSeletedTarget();
		SetReqGoldLabel();
		SetButton();
		ClearSeletedResult();
	}

	public void ClearSeletedResult()
	{
		if (EquipUpgradeTierData != null)
		{
			EquipUpgradeTierData = null;
		}
		ClearResultIcon();
		ClearMaterialIcon();
		ClearOptitonStat();
	}

	private void ClearResultIcon()
	{
		FactoryBase.SafeReleaseComponent(ref _resultComp);
	}

	private void ClearMaterialIcon()
	{
		UIHelper.SetLabelText(LabelReqMaterialCount, "");
		FactoryBase.SafeReleaseComponent(ref _materialComp);
	}

	private void ClearOptitonStat()
	{
		FactoryBase.SafeReleaseComponent(ref _statListComponent);
	}

	private void ReleaseIconProxy()
	{
		if (_iconProxy != null)
		{
			FactoryBase.SafeReleaseComponent(ref _iconProxy);
		}
	}

	private bool IsEnoughMaterial()
	{
		if (EquipUpgradeTierData == null)
		{
			return false;
		}
		return GlobalAutoBehaviour<AccountManager>.instance.ItemManager.GetItemCount(EquipUpgradeTierData.MaterialItemCode1) >= EquipUpgradeTierData.MaterialItemCount1;
	}

	private bool IsEnoughGold()
	{
		if (EquipUpgradeTierData == null)
		{
			return false;
		}
		return GlobalAutoBehaviour<AccountManager>.instance.UserInfo.Gold >= EquipUpgradeTierData.CostValue;
	}

	public void OnClickTierUpgrade()
	{
		if (TargetComponent == null)
		{
			MessagePopup.Open("알림", "아이템이 없어요");
		}
		else if (!IsEnoughMaterial())
		{
			MessagePopup.Open("알림", "재료가 없어요");
		}
		else if (_listener != null)
		{
			int slotIndex = TargetComponent.EquipItemInfo.SlotIndex;
			_listener.OnClickTierUpgrade(slotIndex);
		}
	}
}

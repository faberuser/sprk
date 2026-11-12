using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NShared;
using NShared.NLocalization;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class EquipTierUpgrade : EquipEnhanceBase
{
	public interface Listener
	{
		void OnUpgrade(EquipTierUpgrade window);
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Equip/EquipTierUpgrade";

	[Header("-EquipTierUpgrade")]
	public UILabel TitleLabel;

	public UISimpleGrid ResultItemGrid;

	public UIScrollGrid MaterialScrollGrid;

	public UILabel CostLabel;

	private Color CostColor;

	private Listener _listener;

	private ItemIconComponent ResultItemIcon;

	private List<TierUpgradeMaterialIconComponent> MaterialItems = new List<TierUpgradeMaterialIconComponent>();

	public bool IsEnoughMaterial { get; private set; }

	public bool IsEnoughCost { get; private set; }

	public override void Awake()
	{
		base.Awake();
		CostColor = ((CostLabel != null) ? CostLabel.color : Color.white);
	}

	public void Init(Listener listener)
	{
		InitBase();
		_listener = listener;
		SetCost(0);
		SetTitleMessage(null);
	}

	public override void Reset()
	{
		base.Reset();
		ClearMaterialList();
		ClearResultItem();
		IsEnoughMaterial = false;
		SetCost(0);
		SetTitleMessage(null);
	}

	private void ClearMaterialList()
	{
		FactoryBase.ReleaseAndClearList(MaterialItems);
	}

	private void ClearResultItem()
	{
		FactoryBase.SafeReleaseComponent(ref ResultItemIcon);
	}

	protected override ItemIconComponent CreateInventoryItemComponent(EquipItemInfo equipItemInfo, bool noTechnomagic = false)
	{
		if (CanEnhance(equipItemInfo))
		{
			return base.CreateInventoryItemComponent(equipItemInfo, noTechnomagic);
		}
		return null;
	}

	public override bool CanEnhance(EquipItemInfo equipItemInfo)
	{
		if (GameRoot.Instance.EquipUpgradeTierDataContainer.GetData(equipItemInfo.ItemIndex) == null)
		{
			return false;
		}
		return true;
	}

	public override void OnTargetItemSelected(EquipItemInfo equipItemInfo)
	{
		base.OnTargetItemSelected(equipItemInfo);
		if (equipItemInfo == null)
		{
			ClearMaterialList();
			ClearResultItem();
			SetCost(0);
			SetTitleMessage(null);
			return;
		}
		EquipUpgradeTierData data = GameRoot.Instance.EquipUpgradeTierDataContainer.GetData(equipItemInfo.ItemIndex);
		List<EquipUpgradeTierData.Material> materialList = data.GetMaterialList();
		ClearMaterialList();
		IsEnoughMaterial = true;
		foreach (EquipUpgradeTierData.Material item in materialList)
		{
			if (String_Extension_Method.IsNullOrEmpty(item.ItemCode))
			{
				break;
			}
			TierUpgradeMaterialIconComponent tierUpgradeMaterialIconComponent = UIHelper.AddGridItem(MaterialScrollGrid, TierUpgradeMaterialIconComponent.PrefabAuto);
			if (!(tierUpgradeMaterialIconComponent == null))
			{
				tierUpgradeMaterialIconComponent.Open(item);
				if (!tierUpgradeMaterialIconComponent.IsEnough && IsEnoughMaterial)
				{
					IsEnoughMaterial = false;
				}
				MaterialItems.Add(tierUpgradeMaterialIconComponent);
			}
		}
		UIHelper.RepositionGrid(MaterialScrollGrid);
		ClearResultItem();
		EquipItemInfo equipItemInfo2 = CloneNextTierEquipItemInfo(equipItemInfo, data);
		if (equipItemInfo2 != null)
		{
			ItemIconComponent itemIconComponent = UIHelper.AddGridItem(ResultItemGrid, ItemIconComponent.PrefabAuto);
			itemIconComponent.Open(equipItemInfo2, null);
			ResultItemIcon = itemIconComponent;
			SetCost(data.CostValue);
			SetTitleMessage(equipItemInfo);
		}
	}

	private EquipItemInfo CloneNextTierEquipItemInfo(EquipItemInfo equipItemInfo, EquipUpgradeTierData tierUpgradeData)
	{
		EquipItemInfo equipItemInfo2 = new EquipItemInfo();
		ItemData<EquipItemData> data = GameRoot.Instance.ItemDataContainer.GetData<EquipItemData>(equipItemInfo.ItemIndex);
		ItemData<EquipItemData> data2 = GameRoot.Instance.ItemDataContainer.GetData<EquipItemData>(tierUpgradeData.ResultItemCode);
		if (data2 == null)
		{
			return null;
		}
		equipItemInfo2.SlotIndex = equipItemInfo.SlotIndex;
		equipItemInfo2.ItemIndex = data2.Index;
		equipItemInfo2.Star = 0;
		equipItemInfo2.Level = 0;
		equipItemInfo2.Exp = 0;
		equipItemInfo2.RuneSlotCount = equipItemInfo.RuneSlotCount;
		equipItemInfo2.RuneItemIndex1 = equipItemInfo.RuneItemIndex1;
		equipItemInfo2.RuneItemIndex2 = equipItemInfo.RuneItemIndex2;
		equipItemInfo2.RuneItemIndex3 = equipItemInfo.RuneItemIndex3;
		equipItemInfo2.CreatedTime = equipItemInfo.CreatedTime;
		equipItemInfo2.UpgradeStarFailBonus = 0;
		equipItemInfo2.Locked = equipItemInfo.Locked;
		equipItemInfo2.OptionIndex1 = equipItemInfo.OptionIndex1;
		equipItemInfo2.OptionStep1 = equipItemInfo.OptionStep1;
		equipItemInfo2.OptionRenewCount1 = equipItemInfo.OptionRenewCount1;
		equipItemInfo2.OptionIndex2 = equipItemInfo.OptionIndex2;
		equipItemInfo2.OptionStep2 = equipItemInfo.OptionStep2;
		equipItemInfo2.OptionRenewCount2 = equipItemInfo.OptionRenewCount2;
		equipItemInfo2.OptionIndex3 = equipItemInfo.OptionIndex3;
		equipItemInfo2.OptionStep3 = equipItemInfo.OptionStep3;
		equipItemInfo2.OptionRenewCount3 = equipItemInfo.OptionRenewCount3;
		equipItemInfo2.OptionIndex4 = equipItemInfo.OptionIndex4;
		equipItemInfo2.OptionStep4 = equipItemInfo.OptionStep4;
		equipItemInfo2.OptionRenewCount4 = equipItemInfo.OptionRenewCount4;
		equipItemInfo2.OptionIndex5 = equipItemInfo.OptionIndex5;
		equipItemInfo2.OptionStep5 = equipItemInfo.OptionStep5;
		equipItemInfo2.OptionRenewCount5 = equipItemInfo.OptionRenewCount5;
		int randomOptionCount = GameRoot.Instance.EquipItemDetailDataContainer.GetRandomOptionCount(data.SubData.DetailIndex);
		int randomOptionCount2 = GameRoot.Instance.EquipItemDetailDataContainer.GetRandomOptionCount(data2.SubData.DetailIndex);
		int num = Math.Min(randomOptionCount, randomOptionCount2);
		short tier = data.SubData.Tier;
		short tier2 = data2.SubData.Tier;
		List<int> totalEquipOptionIndices = GameRoot.Instance.EquipOptionGroupDataContainer.GetTotalEquipOptionIndices(data.SubData);
		List<int> totalEquipOptionIndices2 = GameRoot.Instance.EquipOptionGroupDataContainer.GetTotalEquipOptionIndices(data2.SubData);
		for (int i = 1; i <= num; i++)
		{
			int slotIndex = i;
			int optionIndex = equipItemInfo.GetOptionIndex(i);
			equipItemInfo.GetOptionStep(i);
			int ChangeOptionIndex = GameRoot.Instance.EquipOptionChangeDataContainer.GetChageOptionIndex(optionIndex, tier, tier2);
			if (ChangeOptionIndex != 0 && totalEquipOptionIndices2.FindIndex((int x) => x == ChangeOptionIndex) != -1)
			{
				SetOption(equipItemInfo2, slotIndex, ChangeOptionIndex);
				continue;
			}
			if (totalEquipOptionIndices.FindIndex((int x) => x == optionIndex) != -1)
			{
				SetOption(equipItemInfo2, slotIndex, optionIndex);
				continue;
			}
			int index = UnityEngine.Random.Range(0, totalEquipOptionIndices2.Count);
			ChangeOptionIndex = totalEquipOptionIndices2[index];
			totalEquipOptionIndices2.RemoveAt(index);
			SetOption(equipItemInfo2, slotIndex, ChangeOptionIndex);
		}
		return equipItemInfo2;
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

	private void SetCost(int cost)
	{
		UIHelper.SetLabelText(CostLabel, Vespa_Int_Extension_Method.ToCommaString(cost));
		bool isEnoughCost = false;
		if (GlobalAutoBehaviour<AccountManager>.isValidInstance && GlobalAutoBehaviour<AccountManager>.instance.UserManager != null)
		{
			isEnoughCost = GlobalAutoBehaviour<AccountManager>.instance.UserManager.Gold >= cost;
		}
		IsEnoughCost = isEnoughCost;
		if (IsEnoughCost)
		{
			UIHelper.SetLabelColor(CostLabel, CostColor);
		}
		else
		{
			UIHelper.SetLabelColor(CostLabel, Color.red);
		}
	}

	private void SetTitleMessage(EquipItemInfo equipItemInfo)
	{
		string text = "";
		bool flag = false;
		if (equipItemInfo == null)
		{
			text = LocalizationManager.GetString(NShared.NLocalization.EquipTierUpgrade.SELECT_5_STAR_EQUIPMENT);
		}
		else if (!equipItemInfo.CLI_CanTierUpgrade())
		{
			text = LocalizationManager.GetString(NShared.NLocalization.EquipTierUpgrade.TIER_UPGRADE_ONLY_5_STAR_EQUIPMENT);
			flag = true;
		}
		else if (!IsEnoughMaterial)
		{
			text = LocalizationManager.GetString(NShared.NLocalization.EquipTierUpgrade.NOT_ENOUGH_MATERIAL_ITEM);
			flag = true;
		}
		else
		{
			text = LocalizationManager.GetString(NShared.NLocalization.EquipTierUpgrade.TOUCH_TIER_UPGRADE_BUTTON);
		}
		UIHelper.SetLabelText(TitleLabel, text);
		UIHelper.SetLabelColor(TitleLabel, flag ? Color.red : Color.white);
	}

	public void OnClickTierUpgrade()
	{
		if (_listener != null)
		{
			_listener.OnUpgrade(this);
		}
	}

	public void OnFinishUpgrade()
	{
		InitEnhanceableEquipItem();
		DeselectTargetEquipItem();
	}
}

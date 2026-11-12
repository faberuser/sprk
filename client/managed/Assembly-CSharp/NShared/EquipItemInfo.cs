using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NHelper.NEquip;
using NVespa;
using NVespa.NSingleton;
using NVespa.NUtil;

namespace NShared;

[JsonSerializable(Javascript = true)]
public class EquipItemInfo : IAccountAttrKey<int>, IItemInfo
{
	private EquipScore _equipScore;

	public DateTime CreateDateTime = DateTime.MinValue;

	private int _equippedHeroIndex = -1;

	private CachedEquipItemData _cachedEquipItemData;

	int IAccountAttrKey<int>.Key => SlotIndex;

	int IItemInfo.ItemIndex => ItemIndex;

	int IItemInfo.ItemCount => 1;

	public int SlotIndex { get; set; }

	public int ItemIndex { get; set; }

	public int Star { get; set; }

	public int Level { get; set; }

	public int Exp { get; set; }

	public int OptionIndex1 { get; set; }

	public int OptionStep1 { get; set; }

	public int OptionRenewCount1 { get; set; }

	public int IsRenewedOption1 { get; set; }

	public int OptionIndex2 { get; set; }

	public int OptionStep2 { get; set; }

	public int OptionRenewCount2 { get; set; }

	public int IsRenewedOption2 { get; set; }

	public int OptionIndex3 { get; set; }

	public int OptionStep3 { get; set; }

	public int OptionRenewCount3 { get; set; }

	public int IsRenewedOption3 { get; set; }

	public int OptionIndex4 { get; set; }

	public int OptionStep4 { get; set; }

	public int OptionRenewCount4 { get; set; }

	public int IsRenewedOption4 { get; set; }

	public int OptionIndex5 { get; set; }

	public int OptionStep5 { get; set; }

	public int OptionRenewCount5 { get; set; }

	public int IsRenewedOption5 { get; set; }

	public int RuneSlotCount { get; set; }

	public int RuneItemIndex1 { get; set; }

	public int RuneItemIndex2 { get; set; }

	public int RuneItemIndex3 { get; set; }

	public string CreatedTime { get; set; }

	public int UpgradeStarFailBonus { get; set; }

	public string Uid { get; set; }

	public byte Locked { get; set; }

	public int EnchantOptionIndex1 { get; set; }

	public int EnchantOptionStep1 { get; set; }

	public int EnchantOptionIndex2 { get; set; }

	public int EnchantOptionStep2 { get; set; }

	public int EnchantOptionIndex3 { get; set; }

	public int EnchantOptionStep3 { get; set; }

	public byte RenewEnchantOptionSlotIndex { get; set; }

	public int InventoryType { get; set; }

	public int ApplyRunePage { get; set; }

	[NonDbSchema]
	public SoulWeaponInfo SoulWeaponInfo { get; set; }

	[NonDbSchema]
	public int Identified { get; set; }

	[NonDbSchema]
	public int ExtraOptionIndex1 { get; set; }

	[NonDbSchema]
	public int ExtraOptionStep1 { get; set; }

	[NonDbSchema]
	public int ExtraOptionRenewCount1 { get; set; }

	[NonDbSchema]
	public int IsRenewedExtraOption1 { get; set; }

	[NonDbSchema]
	public int ExtraOptionIndex2 { get; set; }

	[NonDbSchema]
	public int ExtraOptionStep2 { get; set; }

	[NonDbSchema]
	public int ExtraOptionRenewCount2 { get; set; }

	[NonDbSchema]
	public int IsRenewedExtraOption2 { get; set; }

	[NonDbSchema]
	public PunishmentRuneOptionInfo PunishmentRuneOption { get; set; }

	public EquipScore EquipScore
	{
		get
		{
			if (_equipScore == null)
			{
				_equipScore = new EquipScore(this);
			}
			return _equipScore;
		}
	}

	public CachedEquipItemData CachedEquipItemData
	{
		get
		{
			if (_cachedEquipItemData == null)
			{
				_cachedEquipItemData = new CachedEquipItemData();
				_cachedEquipItemData.Init(ItemIndex);
			}
			return _cachedEquipItemData;
		}
	}

	public EquipItemData EquipItemData
	{
		get
		{
			ItemData<EquipItemData> data = GameRoot.Instance.ItemDataContainer.GetData<EquipItemData>(ItemIndex);
			if (data == null)
			{
				return null;
			}
			if (data.SubData != null)
			{
				return data.SubData;
			}
			return null;
		}
	}

	public ItemData ItemData
	{
		get
		{
			ItemData data = GameRoot.Instance.ItemDataContainer.GetData(ItemIndex);
			if (data != null)
			{
				return data;
			}
			return null;
		}
	}

	public bool IsNew => GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.IsNew(Uid);

	public bool IsEquipped
	{
		get
		{
			if (EquippedHeroInfo != null)
			{
				return true;
			}
			return false;
		}
	}

	public CachedHeroInfo EquippedHeroInfo => GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(GetEquippedHeroIndex());

	public bool IsUniqueWeapon
	{
		get
		{
			if (CachedEquipItemData.PartType != EquipPartType.Weapon || !CachedEquipItemData.IsUnique)
			{
				return false;
			}
			return true;
		}
	}

	public bool IsUniqueWeaponWithoutClassWeapon
	{
		get
		{
			if (CachedEquipItemData.PartType != EquipPartType.Weapon || !CachedEquipItemData.IsUnique)
			{
				return false;
			}
			if (CachedEquipItemData.EquipItemDetailType == EquipItemDetailType.UNIQUE_CLASSWEAPON)
			{
				return false;
			}
			return true;
		}
	}

	public bool IsUniqueTreasure
	{
		get
		{
			if (CachedEquipItemData.PartType != EquipPartType.Treasure || !CachedEquipItemData.IsUnique)
			{
				return false;
			}
			return true;
		}
	}

	public bool IsBreakable
	{
		get
		{
			if (!ItemData.Breakable)
			{
				return false;
			}
			if (IsEquipped)
			{
				return false;
			}
			if (Locked == 1)
			{
				return false;
			}
			if (IsSoulWeapon())
			{
				return false;
			}
			return true;
		}
	}

	public bool IsSellable
	{
		get
		{
			if (ItemData.NotForSale)
			{
				return false;
			}
			if (IsEquipped)
			{
				return false;
			}
			if (IsUniqueWeapon)
			{
				return false;
			}
			if (IsUniqueTreasure)
			{
				return false;
			}
			if (Locked == 1)
			{
				return false;
			}
			return true;
		}
	}

	public bool IsKeepable
	{
		get
		{
			if (IsEquipped)
			{
				return false;
			}
			return true;
		}
	}

	public bool IsTechnoMagic => CachedEquipItemData.EquipType == EquipItemEquipType.TechnoMagic;

	public bool IsIdentified
	{
		get
		{
			if (CachedEquipItemData.EquipType != EquipItemEquipType.TechnoMagic)
			{
				return true;
			}
			return Identified != 0;
		}
	}

	public bool IsAdvanced => CachedEquipItemData.EquipGrade == EquipItemEquipGrade.Advanced;

	public int GetEquippedHeroIndex()
	{
		return _equippedHeroIndex;
	}

	public void SetEquippedHeroIndex_InMainThread(int heroIndex)
	{
		_equippedHeroIndex = heroIndex;
		GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.NewIconRemove(this);
	}

	public void SetEquippedHeroIndex(int heroIndex)
	{
		_equippedHeroIndex = heroIndex;
		SingletonUpdater<Executer>.instance.NextRun(delegate
		{
			GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.NewIconRemove(this);
		});
	}

	public void UpdateCreateTime()
	{
		Vespa_String_Extension_Method2.TryParseNetworkTime2(CreatedTime, out CreateDateTime);
	}

	public override string ToString()
	{
		return "SlotIndex=" + SlotIndex + " ItemIndex=" + ItemIndex;
	}

	public EquipItemOptionInfo GetOptionInfo(int slotIndex)
	{
		int optionIndex = GetOptionIndex(slotIndex);
		if (optionIndex == 0)
		{
			return null;
		}
		return new EquipItemOptionInfo
		{
			SlotIndex = slotIndex,
			OptionIndex = optionIndex,
			IsOptionOpened = IsOptionOpened(slotIndex),
			OptionStep = GetOptionStep(slotIndex),
			OptionRenewCount = GetOptionRenewCount(slotIndex),
			IsRenewedOption = IsRenewedOption(slotIndex)
		};
	}

	public List<EquipItemOptionInfo> GetOptionInfoList(bool isOpenedOnly = false)
	{
		List<EquipItemOptionInfo> list = new List<EquipItemOptionInfo>();
		int num = 1;
		while (true)
		{
			EquipItemOptionInfo optionInfo = GetOptionInfo(num);
			if (optionInfo == null || (isOpenedOnly && !IsOptionOpened(num)))
			{
				break;
			}
			list.Add(optionInfo);
			num++;
		}
		return list;
	}

	public int GetOptionIndex(int slotIndex)
	{
		return slotIndex switch
		{
			1 => OptionIndex1, 
			2 => OptionIndex2, 
			3 => OptionIndex3, 
			4 => OptionIndex4, 
			5 => OptionIndex5, 
			_ => 0, 
		};
	}

	public int GetOptionSlotIndex(int optinIndex)
	{
		if (OptionIndex1 == optinIndex)
		{
			return 1;
		}
		if (OptionIndex2 == optinIndex)
		{
			return 2;
		}
		if (OptionIndex3 == optinIndex)
		{
			return 3;
		}
		if (OptionIndex4 == optinIndex)
		{
			return 4;
		}
		if (OptionIndex5 == optinIndex)
		{
			return 5;
		}
		return 0;
	}

	public bool HasOption()
	{
		for (int i = 1; i <= 5; i++)
		{
			if (GetOptionIndex(i) > 0)
			{
				return true;
			}
		}
		return false;
	}

	public List<int> GetAllOptionIndex(bool openedOnly)
	{
		List<int> list = new List<int>();
		int num = 1;
		while (true)
		{
			int optionIndex = GetOptionIndex(num);
			if (optionIndex == 0 || (openedOnly && !IsOptionOpened(num)))
			{
				break;
			}
			list.Add(optionIndex);
			num++;
		}
		return list;
	}

	public int GetOptionStep(int slotIndex)
	{
		return slotIndex switch
		{
			1 => OptionStep1, 
			2 => OptionStep2, 
			3 => OptionStep3, 
			4 => OptionStep4, 
			5 => OptionStep5, 
			_ => 0, 
		};
	}

	public int GetOptionRenewCount(int slotIndex)
	{
		return slotIndex switch
		{
			1 => OptionRenewCount1, 
			2 => OptionRenewCount2, 
			3 => OptionRenewCount3, 
			4 => OptionRenewCount4, 
			5 => OptionRenewCount5, 
			_ => 0, 
		};
	}

	public bool IsRenewedOption(int slotIndex)
	{
		return slotIndex switch
		{
			1 => IsRenewedOption1 == 1, 
			2 => IsRenewedOption2 == 1, 
			3 => IsRenewedOption3 == 1, 
			4 => IsRenewedOption4 == 1, 
			5 => IsRenewedOption5 == 1, 
			_ => false, 
		};
	}

	public int GetEnchantOptionIndex(int slotIndex)
	{
		return slotIndex switch
		{
			1 => EnchantOptionIndex1, 
			2 => EnchantOptionIndex2, 
			3 => EnchantOptionIndex3, 
			_ => 0, 
		};
	}

	public int GetEnchantOptionStep(int slotIndex)
	{
		return slotIndex switch
		{
			1 => EnchantOptionStep1, 
			2 => EnchantOptionStep2, 
			3 => EnchantOptionStep3, 
			_ => 0, 
		};
	}

	public bool EnchantOptionExists()
	{
		return GetEnchantOptionIndex(1) != 0;
	}

	public bool IsMaxOptionStep()
	{
		int num = 1;
		while (true)
		{
			EquipItemOptionInfo optionInfo = GetOptionInfo(num);
			if (optionInfo == null)
			{
				break;
			}
			int num2 = GameRoot.Instance.EquipOptionDataContainer.GetData(optionInfo.OptionIndex)?.OptionValueStep ?? 0;
			if (optionInfo.OptionStep < num2)
			{
				return false;
			}
			num++;
		}
		return true;
	}

	public bool IsEndOptionStep()
	{
		ItemData<EquipItemData> data = GameRoot.Instance.ItemDataContainer.GetData<EquipItemData>(ItemIndex);
		if (data == null)
		{
			return false;
		}
		EquipItemDetailData data2 = GameRoot.Instance.EquipItemDetailDataContainer.GetData(data.SubData.DetailIndex);
		if (data2 == null || data2.TotalOptionStepUpgradeCount > GetTotalOptionStep())
		{
			return false;
		}
		return true;
	}

	public void SetOptionIndex(int slotIndex, int optionIndex, int optionStep)
	{
		switch (slotIndex)
		{
		case 1:
			OptionIndex1 = optionIndex;
			OptionStep1 = optionStep;
			break;
		case 2:
			OptionIndex2 = optionIndex;
			OptionStep2 = optionStep;
			break;
		case 3:
			OptionIndex3 = optionIndex;
			OptionStep3 = optionStep;
			break;
		case 4:
			OptionIndex4 = optionIndex;
			OptionStep4 = optionStep;
			break;
		case 5:
			OptionIndex5 = optionIndex;
			OptionStep5 = optionStep;
			break;
		}
	}

	public bool IsOptionOpened(int slotIndex)
	{
		if (slotIndex < 1)
		{
			return false;
		}
		ItemData<EquipItemData> data = GameRoot.Instance.ItemDataContainer.GetData<EquipItemData>(ItemIndex);
		if (data == null)
		{
			return false;
		}
		if (GameRoot.Instance.EquipItemDetailDataContainer.GetData(data.SubData.DetailIndex).AllOptionOpen)
		{
			return true;
		}
		int equipItemOptionOpen = GameRoot.Instance.EquipItemLevelDataContainer.GetEquipItemOptionOpen(Level);
		return slotIndex <= equipItemOptionOpen;
	}

	public int GetOptionOpenLevel(int slotIndex)
	{
		bool flag = false;
		ItemData<EquipItemData> data = GameRoot.Instance.ItemDataContainer.GetData<EquipItemData>(ItemIndex);
		if (data != null)
		{
			EquipItemDetailData data2 = GameRoot.Instance.EquipItemDetailDataContainer.GetData(data.SubData.DetailIndex);
			if (data2 != null)
			{
				flag = data2.AllOptionOpen;
			}
		}
		if (flag)
		{
			return 0;
		}
		return GameRoot.Instance.EquipItemLevelDataContainer.GetEquipItemOptionLevel(slotIndex);
	}

	public EquipSetBonusData GetEquipSetBonusData()
	{
		ItemData<EquipItemData> data = GameRoot.Instance.ItemDataContainer.GetData<EquipItemData>(ItemIndex);
		if (data == null)
		{
			return null;
		}
		int setIndex = data.SubData.SetIndex;
		if (setIndex == 0)
		{
			return null;
		}
		return GameRoot.Instance.EquipSetBonusDataContainer.GetData(setIndex);
	}

	public int GetTotalOptionStep()
	{
		int num = 0;
		int num2 = 1;
		while (true)
		{
			EquipItemOptionInfo optionInfo = GetOptionInfo(num2);
			if (optionInfo == null)
			{
				break;
			}
			int optionStep = optionInfo.OptionStep;
			num += optionStep;
			num2++;
		}
		return num;
	}

	public int GetRenewedOptionCount()
	{
		int num = 0;
		int num2 = 1;
		while (true)
		{
			EquipItemOptionInfo optionInfo = GetOptionInfo(num2);
			if (optionInfo == null)
			{
				break;
			}
			if (optionInfo.IsRenewedOption)
			{
				num++;
			}
			num2++;
		}
		return num;
	}

	public int GetRenewedSkillCount()
	{
		int num = 0;
		int num2 = 1;
		while (true)
		{
			EquipItemOptionInfo extraOptionInfo = GetExtraOptionInfo(num2);
			if (extraOptionInfo == null)
			{
				break;
			}
			if (extraOptionInfo.IsRenewedOption)
			{
				num++;
			}
			num2++;
		}
		return num;
	}

	public List<int> GetRuneItemIndexList()
	{
		List<int> list = new List<int>();
		if (RuneItemIndex1 > 0)
		{
			list.Add(RuneItemIndex1);
		}
		if (RuneItemIndex2 > 0)
		{
			list.Add(RuneItemIndex2);
		}
		if (RuneItemIndex3 > 0)
		{
			list.Add(RuneItemIndex3);
		}
		return list;
	}

	public int GetRuneItemIndex(int slotIndex)
	{
		return slotIndex switch
		{
			1 => RuneItemIndex1, 
			2 => RuneItemIndex2, 
			3 => RuneItemIndex3, 
			_ => 0, 
		};
	}

	public void SetRuneItemIndex(int slotIndex, int itemIndex)
	{
		switch (slotIndex)
		{
		case 1:
			RuneItemIndex1 = itemIndex;
			break;
		case 2:
			RuneItemIndex2 = itemIndex;
			break;
		case 3:
			RuneItemIndex3 = itemIndex;
			break;
		}
	}

	public List<EquipItemOptionInfo> GetExtraOptionInfoList(bool isOpenedOnly = false)
	{
		List<EquipItemOptionInfo> list = new List<EquipItemOptionInfo>();
		int num = 1;
		while (true)
		{
			EquipItemOptionInfo extraOptionInfo = GetExtraOptionInfo(num);
			if (extraOptionInfo == null || (isOpenedOnly && !extraOptionInfo.IsOptionOpened))
			{
				break;
			}
			list.Add(extraOptionInfo);
			num++;
		}
		return list;
	}

	public EquipItemOptionInfo GetExtraOptionInfo(int slotIndex)
	{
		int extraOptionIndex = GetExtraOptionIndex(slotIndex);
		if (extraOptionIndex == 0)
		{
			return null;
		}
		return new EquipItemOptionInfo
		{
			SlotIndex = slotIndex,
			OptionIndex = extraOptionIndex,
			IsOptionOpened = (Identified == 1),
			OptionStep = GetExtraOptionStep(slotIndex),
			OptionRenewCount = GetExtraOptionRenewCount(slotIndex),
			IsRenewedOption = IsRenewedExtraOption(slotIndex)
		};
	}

	public int GetExtraOptionIndex(int slotIndex)
	{
		return slotIndex switch
		{
			1 => ExtraOptionIndex1, 
			2 => ExtraOptionIndex2, 
			_ => 0, 
		};
	}

	public int GetExtraOptionStep(int slotIndex)
	{
		return slotIndex switch
		{
			1 => ExtraOptionStep1, 
			2 => ExtraOptionStep2, 
			_ => 0, 
		};
	}

	public int GetExtraOptionRenewCount(int slotIndex)
	{
		return slotIndex switch
		{
			1 => ExtraOptionRenewCount1, 
			2 => ExtraOptionRenewCount1, 
			_ => 0, 
		};
	}

	public bool IsRenewedExtraOption(int slotIndex)
	{
		return slotIndex switch
		{
			1 => IsRenewedExtraOption1 == 1, 
			2 => IsRenewedExtraOption2 == 1, 
			_ => false, 
		};
	}

	public InventoryType GetInventoryType()
	{
		return (InventoryType)InventoryType;
	}

	public void SetInventoryType(InventoryType _InventoryType)
	{
		InventoryType = (int)_InventoryType;
	}

	public void ResetEquipItemScore()
	{
		EquipScore.ClearCache();
	}

	public bool IsSoulWeapon()
	{
		if (SoulWeaponInfo == null)
		{
			return false;
		}
		if (SoulWeaponInfo.ItemIndex == 0 || SoulWeaponInfo.EquipItemSlotIndex == 0)
		{
			return false;
		}
		if (SoulWeaponInfo.ItemIndex != ItemIndex)
		{
			return false;
		}
		return true;
	}

	public void SetSoulWeaponInfo(SoulWeaponInfo soulWeaponInfo)
	{
		SoulWeaponInfo = soulWeaponInfo;
	}

	public EquipItemInfo Clone()
	{
		return new EquipItemInfo
		{
			SlotIndex = SlotIndex,
			ItemIndex = ItemIndex,
			Star = Star,
			Level = Level,
			Exp = Exp,
			OptionIndex1 = OptionIndex1,
			OptionStep1 = OptionStep1,
			OptionRenewCount1 = OptionRenewCount1,
			IsRenewedOption1 = IsRenewedOption1,
			OptionIndex2 = OptionIndex2,
			OptionStep2 = OptionStep2,
			OptionRenewCount2 = OptionRenewCount2,
			IsRenewedOption2 = IsRenewedOption2,
			OptionIndex3 = OptionIndex3,
			OptionStep3 = OptionStep3,
			OptionRenewCount3 = OptionRenewCount3,
			IsRenewedOption3 = IsRenewedOption3,
			OptionIndex4 = OptionIndex4,
			OptionStep4 = OptionStep4,
			OptionRenewCount4 = OptionRenewCount4,
			IsRenewedOption4 = IsRenewedOption4,
			OptionIndex5 = OptionIndex5,
			OptionStep5 = OptionStep5,
			OptionRenewCount5 = OptionRenewCount5,
			IsRenewedOption5 = IsRenewedOption5,
			RuneSlotCount = RuneSlotCount,
			RuneItemIndex1 = RuneItemIndex1,
			RuneItemIndex2 = RuneItemIndex2,
			RuneItemIndex3 = RuneItemIndex3,
			UpgradeStarFailBonus = UpgradeStarFailBonus,
			Uid = Uid,
			Locked = Locked,
			EnchantOptionIndex1 = EnchantOptionIndex1,
			EnchantOptionStep1 = EnchantOptionStep1,
			EnchantOptionIndex2 = EnchantOptionIndex2,
			EnchantOptionStep2 = EnchantOptionStep2,
			EnchantOptionIndex3 = EnchantOptionIndex3,
			EnchantOptionStep3 = EnchantOptionStep3,
			RenewEnchantOptionSlotIndex = RenewEnchantOptionSlotIndex,
			InventoryType = InventoryType,
			ApplyRunePage = ApplyRunePage,
			Identified = Identified,
			ExtraOptionIndex1 = ExtraOptionIndex1,
			ExtraOptionStep1 = ExtraOptionStep1,
			IsRenewedExtraOption1 = IsRenewedExtraOption1,
			ExtraOptionIndex2 = ExtraOptionIndex2,
			ExtraOptionStep2 = ExtraOptionStep2,
			IsRenewedExtraOption2 = IsRenewedExtraOption2
		};
	}

	public void GetEquipOptionDataList(List<EquipOptionCreatedData> openedOptionDataList, List<EquipOptionCreatedData> closedOptionDataList, List<EquipOptionCreatedData> fixedOptionDataList)
	{
		int num = 1;
		while (true)
		{
			EquipItemOptionInfo optionInfo = GetOptionInfo(num);
			if (optionInfo == null)
			{
				break;
			}
			EquipOptionData data = GameRoot.Instance.EquipOptionDataContainer.GetData(optionInfo.OptionIndex);
			if (data != null)
			{
				(IsOptionOpened(num) ? openedOptionDataList : closedOptionDataList)?.Add(new EquipOptionCreatedData(data, Star, optionInfo.OptionStep));
			}
			num++;
		}
		int num2 = 1;
		while (true)
		{
			ItemData<EquipItemData> data2 = GameRoot.Instance.ItemDataContainer.GetData<EquipItemData>(ItemIndex);
			if (data2 == null)
			{
				break;
			}
			int fixedOptionIndex = data2.SubData.GetFixedOptionIndex(num2);
			if (fixedOptionIndex == 0)
			{
				break;
			}
			EquipOptionData data3 = GameRoot.Instance.EquipOptionDataContainer.GetData(fixedOptionIndex);
			if (data3 != null)
			{
				fixedOptionDataList?.Add(new EquipOptionCreatedData(data3, Star, 0));
			}
			num2++;
		}
		int num3 = 1;
		while (true)
		{
			int enchantOptionIndex = GetEnchantOptionIndex(num3);
			if (enchantOptionIndex != 0)
			{
				EquipOptionData data4 = GameRoot.Instance.EquipOptionDataContainer.GetData(enchantOptionIndex);
				if (data4 != null)
				{
					int enchantOptionStep = GetEnchantOptionStep(num3);
					openedOptionDataList?.Add(new EquipOptionCreatedData(data4, Star, enchantOptionStep));
				}
				num3++;
				continue;
			}
			break;
		}
	}
}

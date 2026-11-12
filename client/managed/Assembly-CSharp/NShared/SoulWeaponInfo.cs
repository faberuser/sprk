using System.Collections.Generic;

namespace NShared;

[JsonSerializable(Javascript = true)]
public class SoulWeaponInfo : IAccountAttrKey<int>
{
	private string _itemCode;

	int IAccountAttrKey<int>.Key => EquipItemSlotIndex;

	public int EquipItemSlotIndex { get; set; }

	public int ItemIndex { get; set; }

	public int Grade { get; set; }

	public int ReinforceLevel { get; set; }

	public int ReinforceRatio { get; set; }

	public int Exp { get; set; }

	public int OptionRatio1 { get; set; }

	public int OptionRatio2 { get; set; }

	public int OptionBonusRatio1 { get; set; }

	public int OptionBonusRatio2 { get; set; }

	public int RenewOptionRatio1 { get; set; }

	public int RenewOptionRatio2 { get; set; }

	public int RenewOptionBonusRatio1 { get; set; }

	public int RenewOptionBonusRatio2 { get; set; }

	public int RenewOptionCount { get; set; }

	public string CreatedTime { get; set; }

	[NonDbSchema]
	public long OptionStat1 { get; set; }

	[NonDbSchema]
	public long OptionStat2 { get; set; }

	[NonDbSchema]
	public long BonusOptionStat1 { get; set; }

	[NonDbSchema]
	public long BonusOptionStat2 { get; set; }

	[NonDbSchema]
	public float MaxOptionRatio => 700f;

	[NonDbSchema]
	public float MinOptionRatio => 300f;

	[NonDbSchema]
	public float MaxBonusOptionRatio => 200f;

	private void InitItemCode()
	{
		if (string.IsNullOrEmpty(_itemCode))
		{
			ItemData data = GameRoot.Instance.ItemDataContainer.GetData(ItemIndex);
			if (data != null)
			{
				_itemCode = data.Code;
			}
		}
	}

	public string GetItemCode()
	{
		InitItemCode();
		return _itemCode;
	}

	private ItemData GetItemData()
	{
		if (string.IsNullOrEmpty(GetItemCode()))
		{
			return null;
		}
		return GameRoot.Instance.ItemDataContainer.GetData(ItemIndex);
	}

	public SoulWeaponOptionData GetSoulWeaponOptionData()
	{
		string itemCode = GetItemCode();
		if (string.IsNullOrEmpty(itemCode))
		{
			return null;
		}
		EquipItemData data = GameRoot.Instance.EquipItemDataContainer.GetData(itemCode);
		if (data == null)
		{
			return null;
		}
		CreatureTagType[] tagType = data.TagType;
		if (tagType == null || tagType.Length == 0)
		{
			return null;
		}
		CreatureTagType key = tagType[0];
		SoulWeaponOptionData data2 = GameRoot.Instance.SoulWeaponOptionDataContainer.GetData(key);
		if (data2 != null)
		{
			return data2;
		}
		return null;
	}

	public SoulWeaponReinforceData GetSoulWeaponReinforceData()
	{
		return GameRoot.Instance.SoulWeaponReinforceDataContainer.GetData(ReinforceLevel);
	}

	public SoulWeaponUpgradePriceData GetSoulWeaponUpgradePriceData()
	{
		return GameRoot.Instance.SoulWeaponUpgradePriceDataContainer.GetData(ItemIndex, Grade);
	}

	public SoulWeaponAbilityRewardData GetSoulWeaponAbilityRewardData(ESoulWeaponAbilityTicketType soulWeaponAbilityTicketType)
	{
		ItemData itemData = GetItemData();
		if (itemData == null)
		{
			return null;
		}
		EquipItemData data = GameRoot.Instance.EquipItemDataContainer.GetData(itemData.Code);
		if (data == null)
		{
			return null;
		}
		if (data.CreatureIndex == null || data.CreatureIndex.Length == 0)
		{
			return null;
		}
		CreatureData data2 = GameRoot.Instance.CreatureDataContainer.GetData(data.CreatureIndex[0]);
		if (data2 == null)
		{
			return null;
		}
		int grade = 0;
		int reinforceLevel = 0;
		switch (soulWeaponAbilityTicketType)
		{
		case ESoulWeaponAbilityTicketType.Grade:
			grade = Grade;
			break;
		case ESoulWeaponAbilityTicketType.Ether:
			reinforceLevel = ReinforceLevel;
			break;
		case ESoulWeaponAbilityTicketType.Jumping:
			grade = Grade;
			reinforceLevel = ReinforceLevel;
			break;
		default:
			return null;
		}
		return GameRoot.Instance.SoulWeaponAbilityTicketRewardDataContainer.GetData(soulWeaponAbilityTicketType, data2.TagType, grade, reinforceLevel);
	}

	public SoulBreakData GetSoulBreakData()
	{
		ItemData itemData = GetItemData();
		if (itemData == null)
		{
			return null;
		}
		EquipItemData data = GameRoot.Instance.EquipItemDataContainer.GetData(itemData.Code);
		if (data == null)
		{
			return null;
		}
		if (data.CreatureIndex == null || data.CreatureIndex.Length == 0)
		{
			return null;
		}
		CreatureData data2 = GameRoot.Instance.CreatureDataContainer.GetData(data.CreatureIndex[0]);
		if (data2 == null)
		{
			return null;
		}
		return GameRoot.Instance.SoulBreakDataContainer.GetData(data2.TagType, Grade, ReinforceLevel);
	}

	public SoulWeaponUpgradeData GetSoulWeaponUpgradeData()
	{
		return GameRoot.Instance.SoulWeaponUpgradeDataContainer.GetData(Grade);
	}

	public SoulWeaponSkillData GetSoulWeaponSkillData()
	{
		string itemCode = GetItemCode();
		if (string.IsNullOrEmpty(itemCode))
		{
			return null;
		}
		return GameRoot.Instance.SoulWeaponSkillDataContainer.GetData(itemCode);
	}

	public int GetBasicSkillIndex()
	{
		return GetSoulWeaponSkillData()?.BasicSkillIndex ?? 0;
	}

	public int[] GetAddSkillIndices()
	{
		SoulWeaponSkillData soulWeaponSkillData = GetSoulWeaponSkillData();
		if (soulWeaponSkillData == null)
		{
			return null;
		}
		List<int> list = new List<int>();
		for (int i = 1; i <= Grade; i++)
		{
			int addSkillIndex = soulWeaponSkillData.GetAddSkillIndex(i);
			if (addSkillIndex != 0)
			{
				list.Add(addSkillIndex);
			}
		}
		return list.ToArray();
	}

	public int GetOptionRatio(ESoulWeaponStatType statType)
	{
		return statType switch
		{
			ESoulWeaponStatType.Attack => OptionRatio1, 
			ESoulWeaponStatType.Hp => OptionRatio2, 
			_ => -1, 
		};
	}

	public int GetOptionBonusRatio(ESoulWeaponStatType statType)
	{
		return statType switch
		{
			ESoulWeaponStatType.Attack => OptionBonusRatio1, 
			ESoulWeaponStatType.Hp => OptionBonusRatio2, 
			_ => -1, 
		};
	}

	public int GetRenewOptionRatio(ESoulWeaponStatType statType)
	{
		return statType switch
		{
			ESoulWeaponStatType.Attack => RenewOptionRatio1, 
			ESoulWeaponStatType.Hp => RenewOptionRatio2, 
			_ => -1, 
		};
	}

	public int GetRenewOptionBonusRatio(ESoulWeaponStatType statType)
	{
		return statType switch
		{
			ESoulWeaponStatType.Attack => RenewOptionBonusRatio1, 
			ESoulWeaponStatType.Hp => RenewOptionBonusRatio2, 
			_ => -1, 
		};
	}

	public long GetBaseStat(ESoulWeaponStatType statType, bool isRenew)
	{
		SoulWeaponOptionData soulWeaponOptionData = GetSoulWeaponOptionData();
		if (soulWeaponOptionData == null)
		{
			return 0L;
		}
		SoulWeaponReinforceData soulWeaponReinforceData = GetSoulWeaponReinforceData();
		if (soulWeaponReinforceData == null)
		{
			return 0L;
		}
		SoulWeaponUpgradeData soulWeaponUpgradeData = GetSoulWeaponUpgradeData();
		if (soulWeaponUpgradeData == null)
		{
			return 0L;
		}
		return StatCalculator.GetSoulWeaponBaseStat(isRenew ? GetRenewOptionRatio(statType) : GetOptionRatio(statType), soulWeaponOptionData.GetOptionRatioPerValue(statType), soulWeaponReinforceData.StatRate, soulWeaponUpgradeData.UpgradeRate);
	}

	public long GetBonusStat(long baseStat, ESoulWeaponStatType statType, bool isRenew)
	{
		return StatCalculator.GetSoulWeaponBonusStat((int)baseStat, isRenew ? GetRenewOptionBonusRatio(statType) : GetOptionBonusRatio(statType));
	}

	public long GetMaxBonusStat(long baseStat, ESoulWeaponStatType statTyep)
	{
		return StatCalculator.GetSoulWeaponBonusStat((int)baseStat, (int)MaxBonusOptionRatio);
	}

	public long GetMaxStat(ESoulWeaponStatType statType)
	{
		SoulWeaponOptionData soulWeaponOptionData = GetSoulWeaponOptionData();
		if (soulWeaponOptionData == null)
		{
			return 0L;
		}
		SoulWeaponReinforceData soulWeaponReinforceData = GetSoulWeaponReinforceData();
		if (soulWeaponReinforceData == null)
		{
			return 0L;
		}
		SoulWeaponUpgradeData soulWeaponUpgradeData = GetSoulWeaponUpgradeData();
		if (soulWeaponUpgradeData == null)
		{
			return 0L;
		}
		return StatCalculator.GetSoulWeaponBaseStat((int)MaxOptionRatio, soulWeaponOptionData.GetOptionRatioPerValue(statType), soulWeaponReinforceData.StatRate, soulWeaponUpgradeData.UpgradeRate);
	}

	public long GetMinStat(ESoulWeaponStatType statType)
	{
		SoulWeaponOptionData soulWeaponOptionData = GetSoulWeaponOptionData();
		if (soulWeaponOptionData == null)
		{
			return 0L;
		}
		SoulWeaponReinforceData soulWeaponReinforceData = GetSoulWeaponReinforceData();
		if (soulWeaponReinforceData == null)
		{
			return 0L;
		}
		SoulWeaponUpgradeData soulWeaponUpgradeData = GetSoulWeaponUpgradeData();
		if (soulWeaponUpgradeData == null)
		{
			return 0L;
		}
		return StatCalculator.GetSoulWeaponBaseStat((int)MinOptionRatio, soulWeaponOptionData.GetOptionRatioPerValue(statType), soulWeaponReinforceData.StatRate, soulWeaponUpgradeData.UpgradeRate);
	}

	public int GetStatCount()
	{
		return 2;
	}

	public SoulWeaponInfo Clone()
	{
		int[] valueArray = GameRoot.Instance.ConstantDataContainer.GetValueArray<int>("FixedRatios");
		if (valueArray == null || valueArray.Length < 2)
		{
			return null;
		}
		return new SoulWeaponInfo
		{
			EquipItemSlotIndex = EquipItemSlotIndex,
			ItemIndex = ItemIndex,
			Grade = Grade,
			ReinforceLevel = ReinforceLevel,
			Exp = Exp,
			OptionRatio1 = ((OptionRatio1 == 0) ? valueArray[0] : OptionRatio1),
			OptionRatio2 = ((OptionRatio2 == 0) ? valueArray[1] : OptionRatio2),
			OptionBonusRatio1 = OptionBonusRatio1,
			OptionBonusRatio2 = OptionBonusRatio2
		};
	}
}

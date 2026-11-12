using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class EquipItemData : StatData, IItemSubData, IDataKeySelector<string>, IValanceEquipFilterInfo, IValanceFilterInfo
{
	[Key(103)]
	public int TableOrder;

	[Key(79)]
	public HashString EquipCode { get; set; }

	[Key(80)]
	public EquipPartType PartType { get; set; }

	[Key(81)]
	public CreatureTagType[] TagType { get; set; }

	[Key(82)]
	public EquipItemSubType SubType { get; set; }

	[Key(83)]
	public bool IsOpenSoulWeapon { get; set; }

	[Key(84)]
	public int[] CreatureIndex { get; set; }

	[Key(85)]
	public short Tier { get; set; }

	[Key(86)]
	public short ReqLevel { get; set; }

	[Key(87)]
	public int[] OptionIndex { get; set; }

	[Key(88)]
	public int[] OptionGroupIndex { get; set; }

	[Key(89)]
	public bool EnableDuplicationOption { get; set; }

	[Key(90)]
	public int FixedOptionIndex1 { get; set; }

	[Key(91)]
	public int FixedOptionIndex2 { get; set; }

	[Key(92)]
	public int FixedOptionIndex3 { get; set; }

	[Key(93)]
	public int FixedOptionIndex4 { get; set; }

	[Key(94)]
	public int FixedOptionIndex5 { get; set; }

	[Key(95)]
	public int SetIndex { get; set; }

	[Key(96)]
	public string BodyPrefix { get; set; }

	[Key(97)]
	public string WeaponPrefix { get; set; }

	[Key(98)]
	public int MinRuneCount { get; set; }

	[Key(99)]
	public int MaxRuneCount { get; set; }

	[Key(100)]
	public short Artifact { get; set; }

	[Key(101)]
	public string[] EnchantOptionItemCodes { get; set; }

	[Key(102)]
	public int DetailIndex { get; set; }

	[Key(104)]
	public int[] UniqueOptionIndex { get; set; }

	[Key(105)]
	public int[] UniqueOptionGroupIndex { get; set; }

	[Key(106)]
	public bool EnableDuplicationUniqueOption { get; set; }

	[Key(107)]
	public EquipItemWorkPlace WorkPlace { get; set; }

	[Key(108)]
	public EquipItemEquipType EquipType { get; set; }

	[Key(109)]
	public EquipItemEquipGrade EquipGrade { get; set; }

	[Key(110)]
	public EquipItemSetGroupType SetGroupType { get; set; }

	string IDataKeySelector<string>.Key => EquipCode;

	string IItemSubData.ItemCode => EquipCode;

	[IgnoreMember]
	public bool IsSpecialWeapon => SubType == EquipItemSubType.SpecialWeapon;

	[IgnoreMember]
	public bool IsSpecialTreasure
	{
		get
		{
			if (SubType != EquipItemSubType.SpecialTreasure_1 && SubType != EquipItemSubType.SpecialTreasure_2 && SubType != EquipItemSubType.SpecialTreasure_3)
			{
				return SubType == EquipItemSubType.SpecialTreasure_4;
			}
			return true;
		}
	}

	EquipItemSetGroupType IValanceEquipFilterInfo.SetType => SetGroupType;

	bool IValanceEquipFilterInfo.IsCreatureTagTypeArray => true;

	CreatureTagType IValanceEquipFilterInfo.CreatureTagType => CreatureTagType.None;

	CreatureTagType[] IValanceEquipFilterInfo.CreatureTagTypes => TagType;

	EquipPartType IValanceEquipFilterInfo.EquipPartType => PartType;

	EquipItemSubType IValanceEquipFilterInfo.EquipItemSubType => SubType;

	int IValanceEquipFilterInfo.DisplayEquipOptionIndex => 0;

	public override string ToString()
	{
		return "EquipCode=" + EquipCode;
	}

	public bool GetMainStat(out StatType statType, out long statValue, int star = 0, int detailIndex = 0, int level = 0)
	{
		if (!GetMainStat(PartType, out statType, out statValue))
		{
			return false;
		}
		EquipItemStarData data = GameRoot.Instance.EquipItemStarDataContainer.GetData(star);
		EquipUpgradeStatData data2 = GameRoot.Instance.EquipUpgradeStatDataContainer.GetData(star, detailIndex, level);
		if (data2 != null && data != null)
		{
			statValue = StatCalculator.GetEquipFinalStat(statType, statValue, data, data2);
		}
		return true;
	}

	public int GetFixedOptionIndex(int slotIndex)
	{
		return slotIndex switch
		{
			1 => FixedOptionIndex1, 
			2 => FixedOptionIndex2, 
			3 => FixedOptionIndex3, 
			4 => FixedOptionIndex4, 
			5 => FixedOptionIndex5, 
			_ => 0, 
		};
	}

	public string GetCreatureLimitString()
	{
		if (CreatureIndex == null || CreatureIndex.Length == 0)
		{
			return string.Empty;
		}
		string text = "";
		for (int i = 0; i < CreatureIndex.Length; i++)
		{
			int key = CreatureIndex[i];
			CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(key);
			if (data != null)
			{
				text += (string.IsNullOrEmpty(text) ? "" : ",");
				text += data.Name;
			}
		}
		return text;
	}
}

using System;
using System.Collections.Generic;
using MessagePack;
using NShared.NLocalization;

namespace NShared;

[MessagePackObject(false)]
public sealed class RuneItemData : StatData, IItemSubData, IDataKeySelector<string>
{
	[Key(79)]
	public string ItemCode { get; set; }

	[Key(80)]
	public string GroupCode { get; set; }

	[Key(81)]
	public EquipPartType[] PartTypes { get; set; }

	[Key(82)]
	public RuneColorType ColorType { get; set; }

	[Key(83)]
	public RuneSlotType[] SlotTypes { get; set; }

	[Key(84)]
	public string[] Tag { get; set; }

	[Key(85)]
	public EquipPartTypeFlag PartTypeFlag { get; set; }

	string IDataKeySelector<string>.Key => ItemCode;

	public override string ToString()
	{
		return "ItemCode=" + ItemCode + " (PartTypes=" + PartTypeFlag.ToString() + ")";
	}

	public Dictionary<Enum, long> GetStatTypeAndValue()
	{
		Dictionary<Enum, long> dictionary = new Dictionary<Enum, long>();
		_ = string.Empty;
		StatType[] visibleStats = StatData.GetVisibleStats();
		foreach (StatType statType in visibleStats)
		{
			long stat = GetStat(statType);
			if (stat == 0L)
			{
				continue;
			}
			if (StatTypeChecker.IsContainEqualStatGroup(statType))
			{
				StatType statType2 = StatTypeChecker.ReverseStatType(statType);
				if (dictionary.ContainsKey(statType2))
				{
					continue;
				}
			}
			RelatedStatData data = GameRoot.Instance.RelatedStatDataContainer.GetData(statType);
			if (data != null && dictionary.ContainsKey(data.RelatedStatType) && stat == dictionary[data.RelatedStatType])
			{
				dictionary.Remove(data.RelatedStatType);
				dictionary.Add(data.ChangedStatType, stat);
			}
			else
			{
				dictionary.Add(statType, stat);
			}
		}
		return dictionary;
	}

	public string GetStatDisplayString(Func<Enum, string> statDisplayer)
	{
		string text = string.Empty;
		Dictionary<Enum, long> statTypeAndValue = GetStatTypeAndValue();
		int num = 0;
		foreach (KeyValuePair<Enum, long> item in statTypeAndValue)
		{
			string text2 = statDisplayer(item.Key);
			string statDisplayValue = StatData.GetStatDisplayValue(item.Key, item.Value);
			text = text + text2 + " " + statDisplayValue;
			if (num < statTypeAndValue.Count - 1)
			{
				text += "\r\n";
			}
			num++;
		}
		return text;
	}

	public Enum GetPartTypeLocalizationKey()
	{
		return PartTypeFlag switch
		{
			EquipPartTypeFlag.Weapon | EquipPartTypeFlag.Armor | EquipPartTypeFlag.SecondGear => Rune.RUNE_WEAPON_ARMOR_SECONDGEAR, 
			EquipPartTypeFlag.Weapon | EquipPartTypeFlag.Armor => Rune.RUNE_WEAPON_ARMOR, 
			EquipPartTypeFlag.Weapon | EquipPartTypeFlag.SecondGear => Rune.RUNE_WEAPON_SECONDGEAR, 
			EquipPartTypeFlag.Armor | EquipPartTypeFlag.SecondGear => Rune.RUNE_ARMOR_SECONDGEAR, 
			EquipPartTypeFlag.Weapon => Rune.RUNE_TYPE_WEAPON_ONLY, 
			EquipPartTypeFlag.Armor => Rune.RUNE_TYPE_ARMOR_ONLY, 
			EquipPartTypeFlag.Accessory => Rune.RUNE_TYPE_ACCESSORY_ONLY, 
			EquipPartTypeFlag.SecondGear => Rune.RUNE_TYPE_SECONDGEAR_ONLY, 
			EquipPartTypeFlag.Artifact => Rune.RUNE_TYPE_ARTIFACT_ONLY, 
			EquipPartTypeFlag.Orb => Rune.RUNE_TYPE_ORB_ONLY, 
			EquipPartTypeFlag.Treasure => Rune.RUNE_TYPE_TREASURE_ONLY, 
			_ => Rune.RUNE_TYPE_WEAPON_ONLY, 
		};
	}

	public void SetEquipPartTypeFlag()
	{
		PartTypeFlag = EquipPartTypeFlag.None;
		EquipPartType[] partTypes = PartTypes;
		for (int i = 0; i < partTypes.Length; i++)
		{
			switch (partTypes[i])
			{
			case EquipPartType.Weapon:
				PartTypeFlag |= EquipPartTypeFlag.Weapon;
				break;
			case EquipPartType.Armor:
				PartTypeFlag |= EquipPartTypeFlag.Armor;
				break;
			case EquipPartType.Accessory:
				PartTypeFlag |= EquipPartTypeFlag.Accessory;
				break;
			case EquipPartType.SecondGear:
				PartTypeFlag |= EquipPartTypeFlag.SecondGear;
				break;
			case EquipPartType.Artifact:
				PartTypeFlag |= EquipPartTypeFlag.Artifact;
				break;
			case EquipPartType.Orb:
				PartTypeFlag |= EquipPartTypeFlag.Orb;
				break;
			case EquipPartType.Treasure:
				PartTypeFlag |= EquipPartTypeFlag.Treasure;
				break;
			}
		}
	}

	public EquipPartType GetEquipPartTypeByFlag()
	{
		return PartTypeFlag switch
		{
			EquipPartTypeFlag.Weapon | EquipPartTypeFlag.Armor | EquipPartTypeFlag.SecondGear => EquipPartType.WeaponArmorSecondGear, 
			EquipPartTypeFlag.Weapon | EquipPartTypeFlag.Armor => EquipPartType.WeaponArmor, 
			EquipPartTypeFlag.Weapon | EquipPartTypeFlag.SecondGear => EquipPartType.WeaponSecondGear, 
			EquipPartTypeFlag.Armor | EquipPartTypeFlag.SecondGear => EquipPartType.ArmorSecondGear, 
			EquipPartTypeFlag.Weapon => EquipPartType.Weapon, 
			EquipPartTypeFlag.Armor => EquipPartType.Armor, 
			EquipPartTypeFlag.Accessory => EquipPartType.Accessory, 
			EquipPartTypeFlag.SecondGear => EquipPartType.SecondGear, 
			EquipPartTypeFlag.Artifact => EquipPartType.Artifact, 
			EquipPartTypeFlag.Orb => EquipPartType.Orb, 
			EquipPartTypeFlag.Treasure => EquipPartType.Treasure, 
			_ => EquipPartType.None, 
		};
	}
}

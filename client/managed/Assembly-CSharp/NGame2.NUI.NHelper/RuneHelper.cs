using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NDropBonusEvent;
using NShared;
using NVespa.NGlobal;
using NVespa.NSingleton;

namespace NGame2.NUI.NHelper;

public class RuneHelper
{
	public static int GetMaxRunePage()
	{
		return HeroInfo.GetMaxRunePage();
	}

	public static int GetDefaultRunePage()
	{
		return HeroInfo.GetDefaultRunePage();
	}

	public static int GetPunishmentRuneItemBreakDefaultIndex()
	{
		return GameRoot.Instance.ConstantDataContainer.GetValueInt("PunishmentRuneBreakIndex", 1);
	}

	public static int GetMaxRuneSlot()
	{
		return HeroInfo.GetMaxRuneSlot();
	}

	public static int GetRunePagePrice()
	{
		return GameRoot.Instance.ConstantDataContainer.GetValueInt("ExtendRunePageGem");
	}

	public static RuneSlotType[] GetRuneSlotTypes(ItemData itemData)
	{
		if (itemData == null)
		{
			return null;
		}
		if (itemData.Type != ItemType.Rune && itemData.Type != ItemType.PunishmentRune)
		{
			return null;
		}
		return itemData.Type switch
		{
			ItemType.Rune => (itemData as ItemData<RuneItemData>).SubData.SlotTypes, 
			ItemType.PunishmentRune => GameRoot.Instance.PunishmentRuneDataContainer.GetData(itemData.Index)?.SlotTypes, 
			_ => null, 
		};
	}

	public static bool IsCanEquipSlotTypes(ItemData itemData, RuneSlotType slotType)
	{
		if (itemData == null || slotType == RuneSlotType.None)
		{
			return false;
		}
		if (itemData.Type != ItemType.Rune && itemData.Type != ItemType.PunishmentRune)
		{
			return false;
		}
		RuneSlotType[] runeSlotTypes = GetRuneSlotTypes(itemData);
		if (runeSlotTypes == null)
		{
			return false;
		}
		RuneSlotType[] array = runeSlotTypes;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] == slotType)
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsEmptyRunePage(CachedHeroInfo heroInfo, int runePage)
	{
		if (heroInfo == null)
		{
			return true;
		}
		foreach (HeroRunePageInfo runePageInfo in heroInfo.GetRunePageInfos(runePage))
		{
			if (runePageInfo.ItemIndex != 0)
			{
				return false;
			}
		}
		return true;
	}

	public static RuneColorType GetRuneColorType(ItemData itemData)
	{
		if (itemData == null)
		{
			return RuneColorType.None;
		}
		if (itemData.Type != ItemType.Rune && itemData.Type != ItemType.PunishmentRune)
		{
			return RuneColorType.None;
		}
		return itemData.Type switch
		{
			ItemType.Rune => (itemData as ItemData<RuneItemData>).SubData.ColorType, 
			ItemType.PunishmentRune => GameRoot.Instance.PunishmentRuneDataContainer.GetData(itemData.Index)?.ColorType ?? RuneColorType.None, 
			_ => RuneColorType.None, 
		};
	}

	public static RunePriceInfo GetRunePriceInfo(ItemData itemData)
	{
		if (itemData == null || (itemData.Type != ItemType.Rune && itemData.Type != ItemType.PunishmentRune))
		{
			return null;
		}
		int num = 0;
		switch (itemData.Type)
		{
		case ItemType.Rune:
			num = GameRoot.Instance.ConstantDataContainer.GetValueInt("RunePreservePrice");
			break;
		case ItemType.PunishmentRune:
			num = GameRoot.Instance.ConstantDataContainer.GetValueInt("PunishmentRunePreservePrice");
			break;
		}
		int num2 = num;
		bool isDiscountEvent = false;
		DropBonusEvent effectiveEvent = GlobalAutoBehaviour<DropBonusEventManager>.instance.GetEffectiveEvent(EventBonusType.DiscountKeepRune, string.Empty);
		if (effectiveEvent != null)
		{
			int num3 = (int)Math.Floor((float)(num2 * effectiveEvent.BonusPercentage) / 100f);
			num2 = Math.Max(0, num2 - num3);
			isDiscountEvent = true;
		}
		int effectiveBoosterAddPercent = GlobalAutoBehaviour<AccountManager>.instance.ItemTimeDurationManager.GetEffectiveBoosterAddPercent(EventBonusType.DiscountKeepRune);
		if (effectiveBoosterAddPercent > 0)
		{
			int num4 = (int)Math.Floor((float)(num2 * effectiveBoosterAddPercent) / 100f);
			num2 = Math.Max(0, num2 - num4);
		}
		bool isFreeKeepRune = num2 == 0;
		return new RunePriceInfo(isDiscountEvent, isFreeKeepRune, num2);
	}

	public static List<HeroRunePageInfo> GetAppliedRunePageInfoList(CachedHeroInfo heroInfo)
	{
		if (heroInfo == null)
		{
			return null;
		}
		new List<HeroRunePageInfo>();
		int runePage = ((heroInfo.ApplyRunePage == 0) ? GetDefaultRunePage() : heroInfo.ApplyRunePage);
		return heroInfo.GetRunePageInfos(runePage);
	}

	public static Dictionary<Enum, long> GetAppliedRuneTotalStat(CachedHeroInfo heroInfo, int forceRunePage = -1)
	{
		if (heroInfo == null)
		{
			return null;
		}
		List<HeroRunePageInfo> list = ((forceRunePage == -1) ? GetAppliedRunePageInfoList(heroInfo) : heroInfo.GetRunePageInfos(forceRunePage));
		if (__Extension_Method_ICollection__.IsNullOrEmpty(list))
		{
			return null;
		}
		Dictionary<Enum, long> dictionary = new Dictionary<Enum, long>();
		foreach (HeroRunePageInfo item in list)
		{
			ItemData data = GameRoot.Instance.ItemDataContainer.GetData(item.ItemIndex);
			AddRuneStatInDic(dictionary, data, heroInfo.GetPunishmentRuneOptionInfo(item));
		}
		return dictionary;
	}

	public static List<string> GetAppliedRuneTotalAddSkill(CachedHeroInfo heroInfo, int forceRunePage = -1)
	{
		if (heroInfo == null)
		{
			return null;
		}
		List<HeroRunePageInfo> list = ((forceRunePage == -1) ? GetAppliedRunePageInfoList(heroInfo) : heroInfo.GetRunePageInfos(forceRunePage));
		if (__Extension_Method_ICollection__.IsNullOrEmpty(list))
		{
			return null;
		}
		CreatureData creatureData = heroInfo.CreatureData;
		if (creatureData == null)
		{
			return null;
		}
		List<string> list2 = new List<string>();
		foreach (HeroRunePageInfo item in list)
		{
			string runeAddSkill = GetRuneAddSkill(GameRoot.Instance.ItemDataContainer.GetData(item.ItemIndex), heroInfo.GetPunishmentRuneOptionInfo(item), creatureData.TagType);
			if (!String_Extension_Method.IsNullOrEmpty(runeAddSkill))
			{
				list2.Add(runeAddSkill);
			}
		}
		return list2;
	}

	public static Dictionary<Enum, long> GetRuneStat(ItemData itemData, PunishmentRuneOptionInfo optionInfo)
	{
		Dictionary<Enum, long> dictionary = new Dictionary<Enum, long>();
		AddRuneStatInDic(dictionary, itemData, optionInfo);
		return dictionary;
	}

	public static string GetRuneAddSkill(ItemData itemData, PunishmentRuneOptionInfo punishmentOptionInfo, CreatureTagType tagType = CreatureTagType.None)
	{
		if (itemData == null || punishmentOptionInfo == null || itemData.Type != ItemType.PunishmentRune)
		{
			return string.Empty;
		}
		PunishmentRuneData data = GameRoot.Instance.PunishmentRuneDataContainer.GetData(itemData.Index);
		if (data == null)
		{
			return string.Empty;
		}
		if (tagType == CreatureTagType.None)
		{
			string text = string.Empty;
			for (int i = 1; i <= 3; i++)
			{
				int extraSkillIndex = punishmentOptionInfo.GetExtraSkillIndex(i);
				int extraSkillValue = punishmentOptionInfo.GetExtraSkillValue(i);
				SkillData data2 = GameRoot.Instance.SkillDataContainer.GetData(extraSkillIndex);
				if (data2 != null)
				{
					string displaySkillValue = GetDisplaySkillValue(extraSkillValue, data.ExtraSkillFraction);
					string punishmentRuneEquipClassification = GetPunishmentRuneEquipClassification(i);
					text = text + punishmentRuneEquipClassification + data2.BuildLocalize_CustomDescription(displaySkillValue) + "\n";
				}
			}
			return text;
		}
		int optionIndex = PunishmentRuneData.GetOptionIndex(tagType);
		int extraSkillIndex2 = punishmentOptionInfo.GetExtraSkillIndex(optionIndex);
		int extraSkillValue2 = punishmentOptionInfo.GetExtraSkillValue(optionIndex);
		SkillData data3 = GameRoot.Instance.SkillDataContainer.GetData(extraSkillIndex2);
		if (data3 == null)
		{
			return string.Empty;
		}
		string displaySkillValue2 = GetDisplaySkillValue(extraSkillValue2, data.ExtraSkillFraction);
		return data3.BuildLocalize_CustomDescription(displaySkillValue2);
	}

	private static void AddRuneStatInDic(Dictionary<Enum, long> dic, ItemData itemData, PunishmentRuneOptionInfo punishmentRuneOptionInfo)
	{
		if (itemData == null || (itemData.Type != ItemType.Rune && itemData.Type != ItemType.PunishmentRune))
		{
			return;
		}
		switch (itemData.Type)
		{
		case ItemType.Rune:
		{
			foreach (KeyValuePair<Enum, long> item in itemData.GetSubData<RuneItemData>().GetStatTypeAndValue())
			{
				Enum key2 = item.Key;
				long value2 = item.Value;
				if (!dic.ContainsKey(key2))
				{
					dic.Add(key2, 0L);
				}
				dic[key2] += value2;
			}
			break;
		}
		case ItemType.PunishmentRune:
		{
			if (GameRoot.Instance.PunishmentRuneDataContainer.GetData(itemData.Index) == null || punishmentRuneOptionInfo == null)
			{
				break;
			}
			PunishmentRuneOptionData data = GameRoot.Instance.PunishmentRuneOptionDataContainer.GetData(punishmentRuneOptionInfo.MainOptionIndex);
			if (data == null)
			{
				break;
			}
			{
				foreach (KeyValuePair<Enum, long> item2 in GetStatTypeAndValue(data.StatTypes, punishmentRuneOptionInfo.MainOptionValue))
				{
					Enum key = item2.Key;
					long value = item2.Value;
					if (!dic.ContainsKey(key))
					{
						dic.Add(key, 0L);
					}
					dic[key] += value;
				}
				break;
			}
		}
		}
	}

	public static Dictionary<Enum, long> GetStatTypeAndValue(StatType[] runeStatTypes, long statValue)
	{
		Dictionary<Enum, long> dictionary = new Dictionary<Enum, long>();
		if (runeStatTypes == null)
		{
			return dictionary;
		}
		_ = string.Empty;
		foreach (StatType statType in runeStatTypes)
		{
			if (statValue == 0L)
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
			if (data != null && dictionary.ContainsKey(data.RelatedStatType) && statValue == dictionary[data.RelatedStatType])
			{
				dictionary.Remove(data.RelatedStatType);
				dictionary.Add(data.ChangedStatType, statValue);
			}
			else
			{
				dictionary.Add(statType, statValue);
			}
		}
		return dictionary;
	}

	public static string GetDisplaySkillValue(int value, int fraction)
	{
		double num = value;
		num /= (double)Math.Max(fraction, 1);
		GameRoot.Instance.LocalizationValueDataContainer.GetCultureInfo();
		return num.ToString("0.#");
	}

	public static string GetPunishmentRuneEquipClassification(int index)
	{
		return index switch
		{
			1 => LocalizationManager.GetStringDirect("PUNISHMENT_RUNE_EQUIP_CLASSIFICATION_1"), 
			2 => LocalizationManager.GetStringDirect("PUNISHMENT_RUNE_EQUIP_CLASSIFICATION_2"), 
			3 => LocalizationManager.GetStringDirect("PUNISHMENT_RUNE_EQUIP_CLASSIFICATION_3"), 
			_ => string.Empty, 
		};
	}
}

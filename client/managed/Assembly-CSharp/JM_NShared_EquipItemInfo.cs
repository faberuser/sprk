using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_EquipItemInfo
{
	public static EquipItemInfo Parse(string s)
	{
		return Parse(s, new EquipItemInfo());
	}

	public static EquipItemInfo Parse(string s, EquipItemInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static EquipItemInfo Parse(IDictionary d)
	{
		return Parse(d, new EquipItemInfo());
	}

	public static EquipItemInfo Parse(IDictionary d, EquipItemInfo o)
	{
		o.SlotIndex = BaseJsonMarshaler.ParseValueInt(d, "SlotIndex");
		o.ItemIndex = BaseJsonMarshaler.ParseValueInt(d, "ItemIndex");
		o.Star = BaseJsonMarshaler.ParseValueInt(d, "Star");
		o.Level = BaseJsonMarshaler.ParseValueInt(d, "Level");
		o.Exp = BaseJsonMarshaler.ParseValueInt(d, "Exp");
		o.OptionIndex1 = BaseJsonMarshaler.ParseValueInt(d, "OptionIndex1");
		o.OptionStep1 = BaseJsonMarshaler.ParseValueInt(d, "OptionStep1");
		o.OptionRenewCount1 = BaseJsonMarshaler.ParseValueInt(d, "OptionRenewCount1");
		o.IsRenewedOption1 = BaseJsonMarshaler.ParseValueInt(d, "IsRenewedOption1");
		o.OptionIndex2 = BaseJsonMarshaler.ParseValueInt(d, "OptionIndex2");
		o.OptionStep2 = BaseJsonMarshaler.ParseValueInt(d, "OptionStep2");
		o.OptionRenewCount2 = BaseJsonMarshaler.ParseValueInt(d, "OptionRenewCount2");
		o.IsRenewedOption2 = BaseJsonMarshaler.ParseValueInt(d, "IsRenewedOption2");
		o.OptionIndex3 = BaseJsonMarshaler.ParseValueInt(d, "OptionIndex3");
		o.OptionStep3 = BaseJsonMarshaler.ParseValueInt(d, "OptionStep3");
		o.OptionRenewCount3 = BaseJsonMarshaler.ParseValueInt(d, "OptionRenewCount3");
		o.IsRenewedOption3 = BaseJsonMarshaler.ParseValueInt(d, "IsRenewedOption3");
		o.OptionIndex4 = BaseJsonMarshaler.ParseValueInt(d, "OptionIndex4");
		o.OptionStep4 = BaseJsonMarshaler.ParseValueInt(d, "OptionStep4");
		o.OptionRenewCount4 = BaseJsonMarshaler.ParseValueInt(d, "OptionRenewCount4");
		o.IsRenewedOption4 = BaseJsonMarshaler.ParseValueInt(d, "IsRenewedOption4");
		o.OptionIndex5 = BaseJsonMarshaler.ParseValueInt(d, "OptionIndex5");
		o.OptionStep5 = BaseJsonMarshaler.ParseValueInt(d, "OptionStep5");
		o.OptionRenewCount5 = BaseJsonMarshaler.ParseValueInt(d, "OptionRenewCount5");
		o.IsRenewedOption5 = BaseJsonMarshaler.ParseValueInt(d, "IsRenewedOption5");
		o.RuneSlotCount = BaseJsonMarshaler.ParseValueInt(d, "RuneSlotCount");
		o.RuneItemIndex1 = BaseJsonMarshaler.ParseValueInt(d, "RuneItemIndex1");
		o.RuneItemIndex2 = BaseJsonMarshaler.ParseValueInt(d, "RuneItemIndex2");
		o.RuneItemIndex3 = BaseJsonMarshaler.ParseValueInt(d, "RuneItemIndex3");
		o.CreatedTime = BaseJsonMarshaler.ParseValueString(d, "CreatedTime");
		o.UpgradeStarFailBonus = BaseJsonMarshaler.ParseValueInt(d, "UpgradeStarFailBonus");
		o.Uid = BaseJsonMarshaler.ParseValueString(d, "Uid");
		o.Locked = BaseJsonMarshaler.ParseValueByte(d, "Locked");
		o.EnchantOptionIndex1 = BaseJsonMarshaler.ParseValueInt(d, "EnchantOptionIndex1");
		o.EnchantOptionStep1 = BaseJsonMarshaler.ParseValueInt(d, "EnchantOptionStep1");
		o.EnchantOptionIndex2 = BaseJsonMarshaler.ParseValueInt(d, "EnchantOptionIndex2");
		o.EnchantOptionStep2 = BaseJsonMarshaler.ParseValueInt(d, "EnchantOptionStep2");
		o.EnchantOptionIndex3 = BaseJsonMarshaler.ParseValueInt(d, "EnchantOptionIndex3");
		o.EnchantOptionStep3 = BaseJsonMarshaler.ParseValueInt(d, "EnchantOptionStep3");
		o.RenewEnchantOptionSlotIndex = BaseJsonMarshaler.ParseValueByte(d, "RenewEnchantOptionSlotIndex");
		o.InventoryType = BaseJsonMarshaler.ParseValueInt(d, "InventoryType");
		o.ApplyRunePage = BaseJsonMarshaler.ParseValueInt(d, "ApplyRunePage");
		o.SoulWeaponInfo = BaseJsonMarshaler.ParseObject(d, "SoulWeaponInfo", JM_NShared_SoulWeaponInfo.Parse);
		o.Identified = BaseJsonMarshaler.ParseValueInt(d, "Identified");
		o.ExtraOptionIndex1 = BaseJsonMarshaler.ParseValueInt(d, "ExtraOptionIndex1");
		o.ExtraOptionStep1 = BaseJsonMarshaler.ParseValueInt(d, "ExtraOptionStep1");
		o.ExtraOptionRenewCount1 = BaseJsonMarshaler.ParseValueInt(d, "ExtraOptionRenewCount1");
		o.IsRenewedExtraOption1 = BaseJsonMarshaler.ParseValueInt(d, "IsRenewedExtraOption1");
		o.ExtraOptionIndex2 = BaseJsonMarshaler.ParseValueInt(d, "ExtraOptionIndex2");
		o.ExtraOptionStep2 = BaseJsonMarshaler.ParseValueInt(d, "ExtraOptionStep2");
		o.ExtraOptionRenewCount2 = BaseJsonMarshaler.ParseValueInt(d, "ExtraOptionRenewCount2");
		o.IsRenewedExtraOption2 = BaseJsonMarshaler.ParseValueInt(d, "IsRenewedExtraOption2");
		o.PunishmentRuneOption = BaseJsonMarshaler.ParseObject(d, "PunishmentRuneOption", JM_NShared_PunishmentRuneOptionInfo.Parse);
		return o;
	}

	public static string EncodeJsonString(EquipItemInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(EquipItemInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("SlotIndex", o.SlotIndex.ToString());
		dictionary.Add("ItemIndex", o.ItemIndex.ToString());
		dictionary.Add("Star", o.Star.ToString());
		dictionary.Add("Level", o.Level.ToString());
		dictionary.Add("Exp", o.Exp.ToString());
		dictionary.Add("OptionIndex1", o.OptionIndex1.ToString());
		dictionary.Add("OptionStep1", o.OptionStep1.ToString());
		dictionary.Add("OptionRenewCount1", o.OptionRenewCount1.ToString());
		dictionary.Add("IsRenewedOption1", o.IsRenewedOption1.ToString());
		dictionary.Add("OptionIndex2", o.OptionIndex2.ToString());
		dictionary.Add("OptionStep2", o.OptionStep2.ToString());
		dictionary.Add("OptionRenewCount2", o.OptionRenewCount2.ToString());
		dictionary.Add("IsRenewedOption2", o.IsRenewedOption2.ToString());
		dictionary.Add("OptionIndex3", o.OptionIndex3.ToString());
		dictionary.Add("OptionStep3", o.OptionStep3.ToString());
		dictionary.Add("OptionRenewCount3", o.OptionRenewCount3.ToString());
		dictionary.Add("IsRenewedOption3", o.IsRenewedOption3.ToString());
		dictionary.Add("OptionIndex4", o.OptionIndex4.ToString());
		dictionary.Add("OptionStep4", o.OptionStep4.ToString());
		dictionary.Add("OptionRenewCount4", o.OptionRenewCount4.ToString());
		dictionary.Add("IsRenewedOption4", o.IsRenewedOption4.ToString());
		dictionary.Add("OptionIndex5", o.OptionIndex5.ToString());
		dictionary.Add("OptionStep5", o.OptionStep5.ToString());
		dictionary.Add("OptionRenewCount5", o.OptionRenewCount5.ToString());
		dictionary.Add("IsRenewedOption5", o.IsRenewedOption5.ToString());
		dictionary.Add("RuneSlotCount", o.RuneSlotCount.ToString());
		dictionary.Add("RuneItemIndex1", o.RuneItemIndex1.ToString());
		dictionary.Add("RuneItemIndex2", o.RuneItemIndex2.ToString());
		dictionary.Add("RuneItemIndex3", o.RuneItemIndex3.ToString());
		if (o.CreatedTime != null)
		{
			dictionary.Add("CreatedTime", o.CreatedTime);
		}
		dictionary.Add("UpgradeStarFailBonus", o.UpgradeStarFailBonus.ToString());
		if (o.Uid != null)
		{
			dictionary.Add("Uid", o.Uid);
		}
		dictionary.Add("Locked", o.Locked.ToString());
		dictionary.Add("EnchantOptionIndex1", o.EnchantOptionIndex1.ToString());
		dictionary.Add("EnchantOptionStep1", o.EnchantOptionStep1.ToString());
		dictionary.Add("EnchantOptionIndex2", o.EnchantOptionIndex2.ToString());
		dictionary.Add("EnchantOptionStep2", o.EnchantOptionStep2.ToString());
		dictionary.Add("EnchantOptionIndex3", o.EnchantOptionIndex3.ToString());
		dictionary.Add("EnchantOptionStep3", o.EnchantOptionStep3.ToString());
		dictionary.Add("RenewEnchantOptionSlotIndex", o.RenewEnchantOptionSlotIndex.ToString());
		dictionary.Add("InventoryType", o.InventoryType.ToString());
		dictionary.Add("ApplyRunePage", o.ApplyRunePage.ToString());
		dictionary.Add("SoulWeaponInfo", JM_NShared_SoulWeaponInfo.EncodeJsonObject(o.SoulWeaponInfo));
		dictionary.Add("Identified", o.Identified.ToString());
		dictionary.Add("ExtraOptionIndex1", o.ExtraOptionIndex1.ToString());
		dictionary.Add("ExtraOptionStep1", o.ExtraOptionStep1.ToString());
		dictionary.Add("ExtraOptionRenewCount1", o.ExtraOptionRenewCount1.ToString());
		dictionary.Add("IsRenewedExtraOption1", o.IsRenewedExtraOption1.ToString());
		dictionary.Add("ExtraOptionIndex2", o.ExtraOptionIndex2.ToString());
		dictionary.Add("ExtraOptionStep2", o.ExtraOptionStep2.ToString());
		dictionary.Add("ExtraOptionRenewCount2", o.ExtraOptionRenewCount2.ToString());
		dictionary.Add("IsRenewedExtraOption2", o.IsRenewedExtraOption2.ToString());
		dictionary.Add("PunishmentRuneOption", JM_NShared_PunishmentRuneOptionInfo.EncodeJsonObject(o.PunishmentRuneOption));
		return dictionary;
	}
}

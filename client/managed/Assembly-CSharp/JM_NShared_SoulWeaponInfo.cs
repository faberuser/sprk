using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_SoulWeaponInfo
{
	public static SoulWeaponInfo Parse(string s)
	{
		return Parse(s, new SoulWeaponInfo());
	}

	public static SoulWeaponInfo Parse(string s, SoulWeaponInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static SoulWeaponInfo Parse(IDictionary d)
	{
		return Parse(d, new SoulWeaponInfo());
	}

	public static SoulWeaponInfo Parse(IDictionary d, SoulWeaponInfo o)
	{
		o.EquipItemSlotIndex = BaseJsonMarshaler.ParseValueInt(d, "EquipItemSlotIndex");
		o.ItemIndex = BaseJsonMarshaler.ParseValueInt(d, "ItemIndex");
		o.Grade = BaseJsonMarshaler.ParseValueInt(d, "Grade");
		o.ReinforceLevel = BaseJsonMarshaler.ParseValueInt(d, "ReinforceLevel");
		o.ReinforceRatio = BaseJsonMarshaler.ParseValueInt(d, "ReinforceRatio");
		o.Exp = BaseJsonMarshaler.ParseValueInt(d, "Exp");
		o.OptionRatio1 = BaseJsonMarshaler.ParseValueInt(d, "OptionRatio1");
		o.OptionRatio2 = BaseJsonMarshaler.ParseValueInt(d, "OptionRatio2");
		o.OptionBonusRatio1 = BaseJsonMarshaler.ParseValueInt(d, "OptionBonusRatio1");
		o.OptionBonusRatio2 = BaseJsonMarshaler.ParseValueInt(d, "OptionBonusRatio2");
		o.RenewOptionRatio1 = BaseJsonMarshaler.ParseValueInt(d, "RenewOptionRatio1");
		o.RenewOptionRatio2 = BaseJsonMarshaler.ParseValueInt(d, "RenewOptionRatio2");
		o.RenewOptionBonusRatio1 = BaseJsonMarshaler.ParseValueInt(d, "RenewOptionBonusRatio1");
		o.RenewOptionBonusRatio2 = BaseJsonMarshaler.ParseValueInt(d, "RenewOptionBonusRatio2");
		o.RenewOptionCount = BaseJsonMarshaler.ParseValueInt(d, "RenewOptionCount");
		o.CreatedTime = BaseJsonMarshaler.ParseValueString(d, "CreatedTime");
		o.OptionStat1 = BaseJsonMarshaler.ParseValueLong(d, "OptionStat1");
		o.OptionStat2 = BaseJsonMarshaler.ParseValueLong(d, "OptionStat2");
		o.BonusOptionStat1 = BaseJsonMarshaler.ParseValueLong(d, "BonusOptionStat1");
		o.BonusOptionStat2 = BaseJsonMarshaler.ParseValueLong(d, "BonusOptionStat2");
		return o;
	}

	public static string EncodeJsonString(SoulWeaponInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(SoulWeaponInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("EquipItemSlotIndex", o.EquipItemSlotIndex.ToString());
		dictionary.Add("ItemIndex", o.ItemIndex.ToString());
		dictionary.Add("Grade", o.Grade.ToString());
		dictionary.Add("ReinforceLevel", o.ReinforceLevel.ToString());
		dictionary.Add("ReinforceRatio", o.ReinforceRatio.ToString());
		dictionary.Add("Exp", o.Exp.ToString());
		dictionary.Add("OptionRatio1", o.OptionRatio1.ToString());
		dictionary.Add("OptionRatio2", o.OptionRatio2.ToString());
		dictionary.Add("OptionBonusRatio1", o.OptionBonusRatio1.ToString());
		dictionary.Add("OptionBonusRatio2", o.OptionBonusRatio2.ToString());
		dictionary.Add("RenewOptionRatio1", o.RenewOptionRatio1.ToString());
		dictionary.Add("RenewOptionRatio2", o.RenewOptionRatio2.ToString());
		dictionary.Add("RenewOptionBonusRatio1", o.RenewOptionBonusRatio1.ToString());
		dictionary.Add("RenewOptionBonusRatio2", o.RenewOptionBonusRatio2.ToString());
		dictionary.Add("RenewOptionCount", o.RenewOptionCount.ToString());
		if (o.CreatedTime != null)
		{
			dictionary.Add("CreatedTime", o.CreatedTime);
		}
		dictionary.Add("OptionStat1", o.OptionStat1.ToString());
		dictionary.Add("OptionStat2", o.OptionStat2.ToString());
		dictionary.Add("BonusOptionStat1", o.BonusOptionStat1.ToString());
		dictionary.Add("BonusOptionStat2", o.BonusOptionStat2.ToString());
		return dictionary;
	}
}

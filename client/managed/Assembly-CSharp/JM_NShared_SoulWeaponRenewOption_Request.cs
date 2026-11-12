using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.SoulWeaponRenewOption;

public static class JM_NShared_SoulWeaponRenewOption_Request
{
	public static NShared.SoulWeaponRenewOption.Request Parse(string s)
	{
		return Parse(s, new NShared.SoulWeaponRenewOption.Request());
	}

	public static NShared.SoulWeaponRenewOption.Request Parse(string s, NShared.SoulWeaponRenewOption.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.SoulWeaponRenewOption.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.SoulWeaponRenewOption.Request());
	}

	public static NShared.SoulWeaponRenewOption.Request Parse(IDictionary d, NShared.SoulWeaponRenewOption.Request o)
	{
		o.EquipItemSlotIndex = BaseJsonMarshaler.ParseValueInt(d, "EquipItemSlotIndex");
		o.CouponItemIndex = BaseJsonMarshaler.ParseValueInt(d, "CouponItemIndex");
		o.OptionStat1 = BaseJsonMarshaler.ParseValueLong(d, "OptionStat1");
		o.OptionStat2 = BaseJsonMarshaler.ParseValueLong(d, "OptionStat2");
		o.BonusOptionStat1 = BaseJsonMarshaler.ParseValueLong(d, "BonusOptionStat1");
		o.BonusOptionStat2 = BaseJsonMarshaler.ParseValueLong(d, "BonusOptionStat2");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.SoulWeaponRenewOption.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.SoulWeaponRenewOption.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("EquipItemSlotIndex", o.EquipItemSlotIndex.ToString());
		dictionary.Add("CouponItemIndex", o.CouponItemIndex.ToString());
		dictionary.Add("OptionStat1", o.OptionStat1.ToString());
		dictionary.Add("OptionStat2", o.OptionStat2.ToString());
		dictionary.Add("BonusOptionStat1", o.BonusOptionStat1.ToString());
		dictionary.Add("BonusOptionStat2", o.BonusOptionStat2.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}

using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_PunishmentRuneOptionInfo
{
	public static PunishmentRuneOptionInfo Parse(string s)
	{
		return Parse(s, new PunishmentRuneOptionInfo());
	}

	public static PunishmentRuneOptionInfo Parse(string s, PunishmentRuneOptionInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static PunishmentRuneOptionInfo Parse(IDictionary d)
	{
		return Parse(d, new PunishmentRuneOptionInfo());
	}

	public static PunishmentRuneOptionInfo Parse(IDictionary d, PunishmentRuneOptionInfo o)
	{
		o.EquipItemSlotIndex = BaseJsonMarshaler.ParseValueInt(d, "EquipItemSlotIndex");
		o.MainOptionIndex = BaseJsonMarshaler.ParseValueInt(d, "MainOptionIndex");
		o.MainOptionValue = BaseJsonMarshaler.ParseValueInt(d, "MainOptionValue");
		o.ExtraSkillIndex1 = BaseJsonMarshaler.ParseValueInt(d, "ExtraSkillIndex1");
		o.ExtraSkillIndex2 = BaseJsonMarshaler.ParseValueInt(d, "ExtraSkillIndex2");
		o.ExtraSkillIndex3 = BaseJsonMarshaler.ParseValueInt(d, "ExtraSkillIndex3");
		o.ExtraSkillValue1 = BaseJsonMarshaler.ParseValueInt(d, "ExtraSkillValue1");
		o.ExtraSkillValue2 = BaseJsonMarshaler.ParseValueInt(d, "ExtraSkillValue2");
		o.ExtraSkillValue3 = BaseJsonMarshaler.ParseValueInt(d, "ExtraSkillValue3");
		return o;
	}

	public static string EncodeJsonString(PunishmentRuneOptionInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(PunishmentRuneOptionInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("EquipItemSlotIndex", o.EquipItemSlotIndex.ToString());
		dictionary.Add("MainOptionIndex", o.MainOptionIndex.ToString());
		dictionary.Add("MainOptionValue", o.MainOptionValue.ToString());
		dictionary.Add("ExtraSkillIndex1", o.ExtraSkillIndex1.ToString());
		dictionary.Add("ExtraSkillIndex2", o.ExtraSkillIndex2.ToString());
		dictionary.Add("ExtraSkillIndex3", o.ExtraSkillIndex3.ToString());
		dictionary.Add("ExtraSkillValue1", o.ExtraSkillValue1.ToString());
		dictionary.Add("ExtraSkillValue2", o.ExtraSkillValue2.ToString());
		dictionary.Add("ExtraSkillValue3", o.ExtraSkillValue3.ToString());
		return dictionary;
	}
}

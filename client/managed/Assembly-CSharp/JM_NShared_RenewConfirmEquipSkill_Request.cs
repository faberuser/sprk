using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.RenewConfirmEquipSkill;

public static class JM_NShared_RenewConfirmEquipSkill_Request
{
	public static NShared.RenewConfirmEquipSkill.Request Parse(string s)
	{
		return Parse(s, new NShared.RenewConfirmEquipSkill.Request());
	}

	public static NShared.RenewConfirmEquipSkill.Request Parse(string s, NShared.RenewConfirmEquipSkill.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.RenewConfirmEquipSkill.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.RenewConfirmEquipSkill.Request());
	}

	public static NShared.RenewConfirmEquipSkill.Request Parse(IDictionary d, NShared.RenewConfirmEquipSkill.Request o)
	{
		o.EquipItemSlotIndex = BaseJsonMarshaler.ParseValueInt(d, "EquipItemSlotIndex");
		o.ConfirmSkillIndex = BaseJsonMarshaler.ParseValueInt(d, "ConfirmSkillIndex");
		o.SkillSlotIndex = BaseJsonMarshaler.ParseValueInt(d, "SkillSlotIndex");
		o.IsNew = BaseJsonMarshaler.ParseValueBool(d, "IsNew");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.RenewConfirmEquipSkill.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.RenewConfirmEquipSkill.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("EquipItemSlotIndex", o.EquipItemSlotIndex.ToString());
		dictionary.Add("ConfirmSkillIndex", o.ConfirmSkillIndex.ToString());
		dictionary.Add("SkillSlotIndex", o.SkillSlotIndex.ToString());
		dictionary.Add("IsNew", o.IsNew.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}

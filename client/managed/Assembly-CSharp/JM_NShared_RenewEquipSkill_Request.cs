using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.RenewEquipSkill;

public static class JM_NShared_RenewEquipSkill_Request
{
	public static NShared.RenewEquipSkill.Request Parse(string s)
	{
		return Parse(s, new NShared.RenewEquipSkill.Request());
	}

	public static NShared.RenewEquipSkill.Request Parse(string s, NShared.RenewEquipSkill.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.RenewEquipSkill.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.RenewEquipSkill.Request());
	}

	public static NShared.RenewEquipSkill.Request Parse(IDictionary d, NShared.RenewEquipSkill.Request o)
	{
		o.EquipItemSlotIndex = BaseJsonMarshaler.ParseValueInt(d, "EquipItemSlotIndex");
		o.SkillSlotIndex = BaseJsonMarshaler.ParseValueInt(d, "SkillSlotIndex");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.RenewEquipSkill.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.RenewEquipSkill.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("EquipItemSlotIndex", o.EquipItemSlotIndex.ToString());
		dictionary.Add("SkillSlotIndex", o.SkillSlotIndex.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}

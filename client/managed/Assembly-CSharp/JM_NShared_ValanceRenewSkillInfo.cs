using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_ValanceRenewSkillInfo
{
	public static ValanceRenewSkillInfo Parse(string s)
	{
		return Parse(s, new ValanceRenewSkillInfo());
	}

	public static ValanceRenewSkillInfo Parse(string s, ValanceRenewSkillInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static ValanceRenewSkillInfo Parse(IDictionary d)
	{
		return Parse(d, new ValanceRenewSkillInfo());
	}

	public static ValanceRenewSkillInfo Parse(IDictionary d, ValanceRenewSkillInfo o)
	{
		o.RenewSkillEquipItemSlotIndex = BaseJsonMarshaler.ParseValueInt(d, "RenewSkillEquipItemSlotIndex");
		o.RenewSkillResultIndex = BaseJsonMarshaler.ParseValueInt(d, "RenewSkillResultIndex");
		o.RenewSkillResultStep = BaseJsonMarshaler.ParseValueInt(d, "RenewSkillResultStep");
		o.RenewSkillSlotIndex = BaseJsonMarshaler.ParseValueInt(d, "RenewSkillSlotIndex");
		return o;
	}

	public static string EncodeJsonString(ValanceRenewSkillInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(ValanceRenewSkillInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("RenewSkillEquipItemSlotIndex", o.RenewSkillEquipItemSlotIndex.ToString());
		dictionary.Add("RenewSkillResultIndex", o.RenewSkillResultIndex.ToString());
		dictionary.Add("RenewSkillResultStep", o.RenewSkillResultStep.ToString());
		dictionary.Add("RenewSkillSlotIndex", o.RenewSkillSlotIndex.ToString());
		return dictionary;
	}
}

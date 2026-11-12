using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.ConfirmEquipEnchantOption;

public static class JM_NShared_ConfirmEquipEnchantOption_Request
{
	public static NShared.ConfirmEquipEnchantOption.Request Parse(string s)
	{
		return Parse(s, new NShared.ConfirmEquipEnchantOption.Request());
	}

	public static NShared.ConfirmEquipEnchantOption.Request Parse(string s, NShared.ConfirmEquipEnchantOption.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.ConfirmEquipEnchantOption.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.ConfirmEquipEnchantOption.Request());
	}

	public static NShared.ConfirmEquipEnchantOption.Request Parse(IDictionary d, NShared.ConfirmEquipEnchantOption.Request o)
	{
		o.EquipItemSlotIndex = BaseJsonMarshaler.ParseValueInt(d, "EquipItemSlotIndex");
		o.ConfirmEnchantOptionIndex = BaseJsonMarshaler.ParseValueInt(d, "ConfirmEnchantOptionIndex");
		o.IsNew = BaseJsonMarshaler.ParseValueBool(d, "IsNew");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.ConfirmEquipEnchantOption.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.ConfirmEquipEnchantOption.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("EquipItemSlotIndex", o.EquipItemSlotIndex.ToString());
		dictionary.Add("ConfirmEnchantOptionIndex", o.ConfirmEnchantOptionIndex.ToString());
		dictionary.Add("IsNew", o.IsNew.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}

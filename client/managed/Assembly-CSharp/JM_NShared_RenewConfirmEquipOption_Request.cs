using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.RenewConfirmEquipOption;

public static class JM_NShared_RenewConfirmEquipOption_Request
{
	public static NShared.RenewConfirmEquipOption.Request Parse(string s)
	{
		return Parse(s, new NShared.RenewConfirmEquipOption.Request());
	}

	public static NShared.RenewConfirmEquipOption.Request Parse(string s, NShared.RenewConfirmEquipOption.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.RenewConfirmEquipOption.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.RenewConfirmEquipOption.Request());
	}

	public static NShared.RenewConfirmEquipOption.Request Parse(IDictionary d, NShared.RenewConfirmEquipOption.Request o)
	{
		o.EquipItemSlotIndex = BaseJsonMarshaler.ParseValueInt(d, "EquipItemSlotIndex");
		o.ConfirmOptionIndex = BaseJsonMarshaler.ParseValueInt(d, "ConfirmOptionIndex");
		o.OptionSlotIndex = BaseJsonMarshaler.ParseValueInt(d, "OptionSlotIndex");
		o.IsNew = BaseJsonMarshaler.ParseValueBool(d, "IsNew");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.RenewConfirmEquipOption.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.RenewConfirmEquipOption.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("EquipItemSlotIndex", o.EquipItemSlotIndex.ToString());
		dictionary.Add("ConfirmOptionIndex", o.ConfirmOptionIndex.ToString());
		dictionary.Add("OptionSlotIndex", o.OptionSlotIndex.ToString());
		dictionary.Add("IsNew", o.IsNew.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}

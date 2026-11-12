using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.ConfirmValanceEnchant;

public static class JM_NShared_ConfirmValanceEnchant_Request
{
	public static NShared.ConfirmValanceEnchant.Request Parse(string s)
	{
		return Parse(s, new NShared.ConfirmValanceEnchant.Request());
	}

	public static NShared.ConfirmValanceEnchant.Request Parse(string s, NShared.ConfirmValanceEnchant.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.ConfirmValanceEnchant.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.ConfirmValanceEnchant.Request());
	}

	public static NShared.ConfirmValanceEnchant.Request Parse(IDictionary d, NShared.ConfirmValanceEnchant.Request o)
	{
		o.EquipItemSlotIndex = BaseJsonMarshaler.ParseValueInt(d, "EquipItemSlotIndex");
		o.IsNew = BaseJsonMarshaler.ParseValueBool(d, "IsNew");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.ConfirmValanceEnchant.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.ConfirmValanceEnchant.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("EquipItemSlotIndex", o.EquipItemSlotIndex.ToString());
		dictionary.Add("IsNew", o.IsNew.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}

using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.ValanceEnchant;

public static class JM_NShared_ValanceEnchant_Request
{
	public static NShared.ValanceEnchant.Request Parse(string s)
	{
		return Parse(s, new NShared.ValanceEnchant.Request());
	}

	public static NShared.ValanceEnchant.Request Parse(string s, NShared.ValanceEnchant.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.ValanceEnchant.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.ValanceEnchant.Request());
	}

	public static NShared.ValanceEnchant.Request Parse(IDictionary d, NShared.ValanceEnchant.Request o)
	{
		o.EquipItemSlotIndex = BaseJsonMarshaler.ParseValueInt(d, "EquipItemSlotIndex");
		o.ConsumeItemIndex = BaseJsonMarshaler.ParseValueInt(d, "ConsumeItemIndex");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.ValanceEnchant.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.ValanceEnchant.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("EquipItemSlotIndex", o.EquipItemSlotIndex.ToString());
		dictionary.Add("ConsumeItemIndex", o.ConsumeItemIndex.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		SprkEnchantUntil.Encode(dictionary, o);
		return dictionary;
	}
}

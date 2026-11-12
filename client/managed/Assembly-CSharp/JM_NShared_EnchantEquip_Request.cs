using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.EnchantEquip;

public static class JM_NShared_EnchantEquip_Request
{
	public static NShared.EnchantEquip.Request Parse(string s)
	{
		return Parse(s, new NShared.EnchantEquip.Request());
	}

	public static NShared.EnchantEquip.Request Parse(string s, NShared.EnchantEquip.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.EnchantEquip.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.EnchantEquip.Request());
	}

	public static NShared.EnchantEquip.Request Parse(IDictionary d, NShared.EnchantEquip.Request o)
	{
		o.EquipItemSlotIndex = BaseJsonMarshaler.ParseValueInt(d, "EquipItemSlotIndex");
		o.EnchantItemIndex = BaseJsonMarshaler.ParseValueInt(d, "EnchantItemIndex");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.EnchantEquip.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.EnchantEquip.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("EquipItemSlotIndex", o.EquipItemSlotIndex.ToString());
		dictionary.Add("EnchantItemIndex", o.EnchantItemIndex.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}

using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.SoulWeaponAbilityTicket;

public static class JM_NShared_SoulWeaponAbilityTicket_Request
{
	public static NShared.SoulWeaponAbilityTicket.Request Parse(string s)
	{
		return Parse(s, new NShared.SoulWeaponAbilityTicket.Request());
	}

	public static NShared.SoulWeaponAbilityTicket.Request Parse(string s, NShared.SoulWeaponAbilityTicket.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.SoulWeaponAbilityTicket.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.SoulWeaponAbilityTicket.Request());
	}

	public static NShared.SoulWeaponAbilityTicket.Request Parse(IDictionary d, NShared.SoulWeaponAbilityTicket.Request o)
	{
		o.ItemIndex = BaseJsonMarshaler.ParseValueInt(d, "ItemIndex");
		o.TargetEquipItemSlotIndex = BaseJsonMarshaler.ParseValueInt(d, "TargetEquipItemSlotIndex");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.SoulWeaponAbilityTicket.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.SoulWeaponAbilityTicket.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ItemIndex", o.ItemIndex.ToString());
		dictionary.Add("TargetEquipItemSlotIndex", o.TargetEquipItemSlotIndex.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}

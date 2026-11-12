using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.SoulWeaponTransitionTicket;

public static class JM_NShared_SoulWeaponTransitionTicket_Request
{
	public static NShared.SoulWeaponTransitionTicket.Request Parse(string s)
	{
		return Parse(s, new NShared.SoulWeaponTransitionTicket.Request());
	}

	public static NShared.SoulWeaponTransitionTicket.Request Parse(string s, NShared.SoulWeaponTransitionTicket.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.SoulWeaponTransitionTicket.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.SoulWeaponTransitionTicket.Request());
	}

	public static NShared.SoulWeaponTransitionTicket.Request Parse(IDictionary d, NShared.SoulWeaponTransitionTicket.Request o)
	{
		o.ItemIndex = BaseJsonMarshaler.ParseValueInt(d, "ItemIndex");
		o.SourceEquipItemSlotIndex = BaseJsonMarshaler.ParseValueInt(d, "SourceEquipItemSlotIndex");
		o.TargetEquipItemSlotIndex = BaseJsonMarshaler.ParseValueInt(d, "TargetEquipItemSlotIndex");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.SoulWeaponTransitionTicket.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.SoulWeaponTransitionTicket.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ItemIndex", o.ItemIndex.ToString());
		dictionary.Add("SourceEquipItemSlotIndex", o.SourceEquipItemSlotIndex.ToString());
		dictionary.Add("TargetEquipItemSlotIndex", o.TargetEquipItemSlotIndex.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}

using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.SoulWeaponTransition;

public static class JM_NShared_SoulWeaponTransition_Request
{
	public static NShared.SoulWeaponTransition.Request Parse(string s)
	{
		return Parse(s, new NShared.SoulWeaponTransition.Request());
	}

	public static NShared.SoulWeaponTransition.Request Parse(string s, NShared.SoulWeaponTransition.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.SoulWeaponTransition.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.SoulWeaponTransition.Request());
	}

	public static NShared.SoulWeaponTransition.Request Parse(IDictionary d, NShared.SoulWeaponTransition.Request o)
	{
		o.EquipItemSlotIndex = BaseJsonMarshaler.ParseValueInt(d, "EquipItemSlotIndex");
		o.ReceiveEquipItemSlotIndex = BaseJsonMarshaler.ParseValueInt(d, "ReceiveEquipItemSlotIndex");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.SoulWeaponTransition.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.SoulWeaponTransition.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("EquipItemSlotIndex", o.EquipItemSlotIndex.ToString());
		dictionary.Add("ReceiveEquipItemSlotIndex", o.ReceiveEquipItemSlotIndex.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}

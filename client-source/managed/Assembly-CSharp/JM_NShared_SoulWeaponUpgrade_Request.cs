using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.SoulWeaponUpgrade;

public static class JM_NShared_SoulWeaponUpgrade_Request
{
	public static NShared.SoulWeaponUpgrade.Request Parse(string s)
	{
		return Parse(s, new NShared.SoulWeaponUpgrade.Request());
	}

	public static NShared.SoulWeaponUpgrade.Request Parse(string s, NShared.SoulWeaponUpgrade.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.SoulWeaponUpgrade.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.SoulWeaponUpgrade.Request());
	}

	public static NShared.SoulWeaponUpgrade.Request Parse(IDictionary d, NShared.SoulWeaponUpgrade.Request o)
	{
		o.EquipItemSlotIndex = BaseJsonMarshaler.ParseValueInt(d, "EquipItemSlotIndex");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.SoulWeaponUpgrade.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.SoulWeaponUpgrade.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("EquipItemSlotIndex", o.EquipItemSlotIndex.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}

using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.SoulWeaponliberation;

public static class JM_NShared_SoulWeaponliberation_Request
{
	public static NShared.SoulWeaponliberation.Request Parse(string s)
	{
		return Parse(s, new NShared.SoulWeaponliberation.Request());
	}

	public static NShared.SoulWeaponliberation.Request Parse(string s, NShared.SoulWeaponliberation.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.SoulWeaponliberation.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.SoulWeaponliberation.Request());
	}

	public static NShared.SoulWeaponliberation.Request Parse(IDictionary d, NShared.SoulWeaponliberation.Request o)
	{
		o.EquipItemSlotIndex = BaseJsonMarshaler.ParseValueInt(d, "EquipItemSlotIndex");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.SoulWeaponliberation.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.SoulWeaponliberation.Request o)
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

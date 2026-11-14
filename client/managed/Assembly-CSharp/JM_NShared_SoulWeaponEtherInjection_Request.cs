using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.SoulWeaponEtherInjection;

public static class JM_NShared_SoulWeaponEtherInjection_Request
{
	public static NShared.SoulWeaponEtherInjection.Request Parse(string s)
	{
		return Parse(s, new NShared.SoulWeaponEtherInjection.Request());
	}

	public static NShared.SoulWeaponEtherInjection.Request Parse(string s, NShared.SoulWeaponEtherInjection.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.SoulWeaponEtherInjection.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.SoulWeaponEtherInjection.Request());
	}

	public static NShared.SoulWeaponEtherInjection.Request Parse(IDictionary d, NShared.SoulWeaponEtherInjection.Request o)
	{
		o.EquipItemSlotIndex = BaseJsonMarshaler.ParseValueInt(d, "EquipItemSlotIndex");
		o.InjectionItemInfos = BaseJsonMarshaler.ParseArrayCast(d, "InjectionItemInfos", BaseJsonMarshaler.CastToInt);
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.SoulWeaponEtherInjection.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.SoulWeaponEtherInjection.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("EquipItemSlotIndex", o.EquipItemSlotIndex.ToString());
		dictionary.Add("InjectionItemInfos", BaseJsonMarshaler.EncodeArray(o.InjectionItemInfos));
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		SprkSoulWeaponAutoEnhance.Encode(dictionary, o);
		return dictionary;
	}
}

using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.SoulWeaponLimitBreak;

public static class JM_NShared_SoulWeaponLimitBreak_Request
{
	public static NShared.SoulWeaponLimitBreak.Request Parse(string s)
	{
		return Parse(s, new NShared.SoulWeaponLimitBreak.Request());
	}

	public static NShared.SoulWeaponLimitBreak.Request Parse(string s, NShared.SoulWeaponLimitBreak.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.SoulWeaponLimitBreak.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.SoulWeaponLimitBreak.Request());
	}

	public static NShared.SoulWeaponLimitBreak.Request Parse(IDictionary d, NShared.SoulWeaponLimitBreak.Request o)
	{
		o.EquipItemSlotIndex = BaseJsonMarshaler.ParseValueInt(d, "EquipItemSlotIndex");
		o.MaterialSlotIndices = BaseJsonMarshaler.ParseArrayCast(d, "MaterialSlotIndices", BaseJsonMarshaler.CastToInt);
		o.MaterialItemInfors = BaseJsonMarshaler.ParseArrayCast(d, "MaterialItemInfors", BaseJsonMarshaler.CastToInt);
		o.Admin = BaseJsonMarshaler.ParseArrayCast(d, "Admin", BaseJsonMarshaler.CastToString);
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.SoulWeaponLimitBreak.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.SoulWeaponLimitBreak.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("EquipItemSlotIndex", o.EquipItemSlotIndex.ToString());
		dictionary.Add("MaterialSlotIndices", BaseJsonMarshaler.EncodeArray(o.MaterialSlotIndices));
		dictionary.Add("MaterialItemInfors", BaseJsonMarshaler.EncodeArray(o.MaterialItemInfors));
		dictionary.Add("Admin", BaseJsonMarshaler.EncodeArray(o.Admin));
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}

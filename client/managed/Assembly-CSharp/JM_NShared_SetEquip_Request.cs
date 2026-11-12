using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.SetEquip;

public static class JM_NShared_SetEquip_Request
{
	public static NShared.SetEquip.Request Parse(string s)
	{
		return Parse(s, new NShared.SetEquip.Request());
	}

	public static NShared.SetEquip.Request Parse(string s, NShared.SetEquip.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.SetEquip.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.SetEquip.Request());
	}

	public static NShared.SetEquip.Request Parse(IDictionary d, NShared.SetEquip.Request o)
	{
		o.HeroIndex = BaseJsonMarshaler.ParseValueInt(d, "HeroIndex");
		o.HeroPartIndex = BaseJsonMarshaler.ParseArrayCast(d, "HeroPartIndex", BaseJsonMarshaler.CastToInt);
		o.EquipItemSlotIndex = BaseJsonMarshaler.ParseArrayCast(d, "EquipItemSlotIndex", BaseJsonMarshaler.CastToInt);
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.SetEquip.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.SetEquip.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("HeroIndex", o.HeroIndex.ToString());
		dictionary.Add("HeroPartIndex", BaseJsonMarshaler.EncodeArray(o.HeroPartIndex));
		dictionary.Add("EquipItemSlotIndex", BaseJsonMarshaler.EncodeArray(o.EquipItemSlotIndex));
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}

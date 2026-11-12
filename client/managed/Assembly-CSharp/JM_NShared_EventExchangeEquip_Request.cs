using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.EventExchangeEquip;

public static class JM_NShared_EventExchangeEquip_Request
{
	public static NShared.EventExchangeEquip.Request Parse(string s)
	{
		return Parse(s, new NShared.EventExchangeEquip.Request());
	}

	public static NShared.EventExchangeEquip.Request Parse(string s, NShared.EventExchangeEquip.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.EventExchangeEquip.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.EventExchangeEquip.Request());
	}

	public static NShared.EventExchangeEquip.Request Parse(IDictionary d, NShared.EventExchangeEquip.Request o)
	{
		o.EquipItemSlotIndices = BaseJsonMarshaler.ParseArrayCast(d, "EquipItemSlotIndices", BaseJsonMarshaler.CastToInt);
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.EventExchangeEquip.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.EventExchangeEquip.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("EquipItemSlotIndices", BaseJsonMarshaler.EncodeArray(o.EquipItemSlotIndices));
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}

using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.SellEquip;

public static class JM_NShared_SellEquip_Request
{
	public static NShared.SellEquip.Request Parse(string s)
	{
		return Parse(s, new NShared.SellEquip.Request());
	}

	public static NShared.SellEquip.Request Parse(string s, NShared.SellEquip.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.SellEquip.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.SellEquip.Request());
	}

	public static NShared.SellEquip.Request Parse(IDictionary d, NShared.SellEquip.Request o)
	{
		o.EquipItemSlotIndices = BaseJsonMarshaler.ParseArrayCast(d, "EquipItemSlotIndices", BaseJsonMarshaler.CastToInt);
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.SellEquip.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.SellEquip.Request o)
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
